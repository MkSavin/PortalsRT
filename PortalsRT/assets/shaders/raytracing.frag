#version 330

#define RAY_ITERATIONS 3
#define MSAA 2

#define PI 3.14159265359

const float gammaCorrection = 2.2;
const float epsilon = 4e-4;

uniform vec3 screenSize;
uniform vec3 camera_rotation;
uniform vec3 camera_position;

out vec4 outputColor;

//** Scene **


//** Properties **
struct Material
{
    vec3 color;
    // float shadowIntensity;
    float reflectionIntensity;
    float specularPower;
    float specularIntensity;
    float glowness;
};

//** Scene Objects **
struct Camera
{
    // float fov;
    float focal;
    // bool ortho;
    vec3 rotation;
    vec3 position;
};

/*
struct Light
{
    float intensity;
    vec3 position;
    vec3 color;
};
*/

struct SunLight
{
    float intensity;
    vec3 direction;
    vec3 color;
};

struct Sphere
{
	float radius;
    vec3 position;
    Material material;
};

struct Plane
{
    vec2 size;
    vec3 rotation;
    vec3 position;
    Material material;
};

struct Portal
{
    Plane base;
};

struct PortalConnection
{
    Portal a;
    Portal b;
};

struct Box
{
    vec3 size;
    vec3 rotation;
    vec3 position;
    Material material;
};

//** Global **
struct Ray
{
    vec3 origin;
    vec3 direction;
};

struct Hit
{
    Material material;
    float distanceTo;
    vec3 position;
    vec3 normal;
    bool hitObject;
    bool portal;
};

Material nullMaterial = Material(vec3(0.), 0., 0., 0., 0.);
Hit nullHit = Hit(nullMaterial, 10000, vec3(0.), vec3(0.), false, false);

//** MATH **
struct RotationMatrixComponents
{
    mat4 x;
    mat4 y;
    mat4 z;
};

RotationMatrixComponents rotationMatrix(vec3 rotation)
{
    mat4 rotationX = mat4(
        1, 0,               0,                0,
        0, cos(rotation.x), -sin(rotation.x), 0,
        0, sin(rotation.x), cos(rotation.x),  0,
        0, 0,               0,                1
    );

    mat4 rotationY = mat4(
        cos(rotation.y),  0, sin(rotation.y), 0,
        0,                1, 0,               0,
        -sin(rotation.y), 0, cos(rotation.y), 0,
        0,                0, 0,               1
    );

    mat4 rotationZ = mat4(
        cos(rotation.z), -sin(rotation.z), 0, 0,
        sin(rotation.z), cos(rotation.z),  0, 0,
        0,               0,                1, 0,
        0,               0,                0, 1
    );

    return RotationMatrixComponents(rotationX, rotationY, rotationZ);
}

//** INTERSECTIONS **
Hit boxIntersection(Ray ray, Box box, out vec2 uvPosition, out int faceNumber) 
{
    mat4 position = mat4(
        1,              0,              0,              0,
        0,              1,              0,              0,
        0,              0,              1,              0,
        box.position.x, box.position.y, box.position.z, 1
    );

    RotationMatrixComponents rotationComponents = rotationMatrix(box.rotation);

    mat4 transformWorldToBox = inverse(rotationComponents.x * rotationComponents.y * rotationComponents.z * position);

    // Convert World To Box dimension
    vec3 rayDirection = (transformWorldToBox * vec4(ray.direction, 0.)).xyz;
    vec3 rayOrigin = (transformWorldToBox * vec4(ray.origin, 1.)).xyz;

    vec3 halfSize = box.size / 2;

    // Intersect box in Box dimension space
    // magic goes here:
    vec3 m = 1.0 / rayDirection;
    vec3 side = vec3((rayDirection.x < 0.) ? 1.: -1., (rayDirection.y < 0.) ? 1.: -1., (rayDirection.z < 0.) ? 1.: -1.);

    vec3 sizedSide = side * halfSize;

    vec3 point1 = m * (-rayOrigin + sizedSide);
    vec3 point2 = m * (-rayOrigin - sizedSide);

    float distanceToNear = max(max(point1.x, point1.y), point1.z);
    float distanceToFar = min(min(point2.x, point2.y), point2.z);
	
    if(distanceToNear > distanceToFar || distanceToFar < 0.0)
    {
        return nullHit;
    }

    vec3 normal;

    mat4 transformBoxToWorld = inverse(transformWorldToBox);

    // Compute normal (in world space), UV position and face number
    if(point1.x > point1.y && point1.x > point1.z)
    { 
        normal = transformBoxToWorld[0].xyz * side.x; 
        uvPosition = rayOrigin.yz + rayDirection.yz * point1.x; 
        faceNumber = (1 + int(side.x)) / 2;
    }
    else if(point1.y > point1.z)
    { 
        normal = transformBoxToWorld[1].xyz * side.y; 
        uvPosition = rayOrigin.zx + rayDirection.zx * point1.y; 
        faceNumber = (5 + int(side.y)) / 2;
    }
    else
    { 
        normal = transformBoxToWorld[2].xyz * side.z; 
        uvPosition = rayOrigin.xy + rayDirection.xy * point1.z; 
        faceNumber = (9 + int(side.z)) / 2;
    }

    return Hit(box.material, distanceToNear, ray.origin + ray.direction * distanceToNear, normal, true, false);
}

Hit planeIntersection(Ray ray, Plane plane)
{
    vec3 distanceVector = plane.position - ray.origin;

    vec3 planeDirection = vec3(0., 1., 0.);

    RotationMatrixComponents rotationComponents = rotationMatrix(plane.rotation);
    mat4 rotationMatrix = rotationComponents.x * rotationComponents.y * rotationComponents.z;

    planeDirection = (rotationMatrix * vec4(planeDirection, 0.)).xyz;

    if (dot(planeDirection, distanceVector) > 0.)
    {
        planeDirection *= -1;
    }

    float dotNormal = dot(planeDirection, ray.direction);
    
    if (dotNormal > 0.)
    {
        return nullHit;
    }

    float distanceTo = dot(distanceVector, planeDirection) / dotNormal;

    vec3 hitPosition = ray.origin + distanceTo * ray.direction;

    vec3 hitDistanceVector = hitPosition - plane.position;

    hitDistanceVector = (inverse(rotationMatrix) * vec4(hitDistanceVector, 0.)).xyz;

    if (abs(hitDistanceVector.z) > plane.size.y || abs(hitDistanceVector.x) > plane.size.x)
    {
        return nullHit;
    }

    return Hit(plane.material, distanceTo, hitPosition, planeDirection, true, false);
}

Hit sphereIntersection(Ray ray, Sphere sphere)
{
    vec3 distanceVector = ray.origin - sphere.position;

    float b = dot(distanceVector, ray.direction);

    // if (b >= 0)
    // {
    //     return nullHit;
    // }

    // Quadratic equation (very and very simplified by strange logic)
    float disc = b * b - dot(distanceVector, distanceVector) + sphere.radius * sphere.radius;

    if (disc < 0.)
    {
        return nullHit;
    }

    disc = sqrt(disc);
    float distanceTo = -b - disc;

    if (distanceTo < 0.)
    {
        distanceTo = -b + disc;
    }

    if (distanceTo < 0.)
    {
        return nullHit;
    }

    vec3 hitPosition = ray.origin + distanceTo * ray.direction;

    return Hit(sphere.material, distanceTo, hitPosition, (hitPosition - sphere.position) / sphere.radius, true, false);
}

//** RAYTRACING **

vec3 skySimple(vec3 direction)
{
    return vec3(0.001) * direction.y;
}

vec3 sky(Ray ray, vec3 sunDirection, bool fast)
{
    vec3 color = vec3(0.0);

    vec3 p = ray.origin;      
    float T = 1.;    
    float mu = dot(sunDirection, ray.direction);

	vec3 background = 6.0 * mix(vec3(0.2, 0.52, 1.0), vec3(0.8, 0.95, 1.0), pow(0.5 + 0.5 * mu, 15.0)) + mix(vec3(3.5), vec3(0.0), min(1.0, 2.3 * ray.direction.y));

    if(!fast)
    {
        background += T * vec3(1e4 * smoothstep(0.9998, 1.0, mu));
    }

    color += background * T;
    
    return color * 0.022;
}

Hit sceneIntersection(Ray ray)
{
    Hit hit = nullHit;
    Hit tempHit = nullHit;

    // SPHERE
    const int spheresNumber = 2;
    Sphere spheres[spheresNumber];
    spheres[0] = Sphere(0.25, vec3(0., 1., -1.), Material(vec3(1.), 1., 5, 0.02, 0.));
    spheres[1] = Sphere(0.1, vec3(-1., 1., -0.5), Material(vec3(1., 0., 0.), 1., 3, 0.01, 0.));

    for (int i = 0; i < spheresNumber; i++)
    {
        tempHit = sphereIntersection(ray, spheres[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // PLANE
    Material roomMaterial = Material(vec3(1.), 0.1, 5, 0.02, 0.);

    const int planesNumber = 8;
    Plane planes[planesNumber];

    planes[0] = Plane(vec2(2., 4.), vec3(0.), vec3(0.), roomMaterial);
    planes[1] = Plane(vec2(2., 2.), vec3(0.), vec3(6., 0., -2.), roomMaterial);
    planes[2] = Plane(vec2(1., 0.5), vec3(0., 0., PI / 2), vec3(2., 1., 3.5), roomMaterial);
    planes[3] = Plane(vec2(1., 0.5), vec3(0., 0., PI / 2), vec3(2., 1., -3.5), roomMaterial);
    planes[4] = Plane(vec2(1., 1.), vec3(0., 0., PI / 2), vec3(2., 1., 0), roomMaterial);
    planes[5] = Plane(vec2(1., 4.), vec3(0., 0., -PI / 2), vec3(-2., 1., 0), roomMaterial);
    planes[6] = Plane(vec2(1., 2.), vec3(0., PI / 2, PI / 2), vec3(0., 1., -4.), roomMaterial);
    planes[7] = Plane(vec2(1., 2.), vec3(0., -PI / 2., -PI / 2), vec3(0., 1., 4.), roomMaterial);

    for (int i = 0; i < planesNumber; i++)
    {
        tempHit = planeIntersection(ray, planes[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // BOX
    const int boxesNumber = 1;
    Box boxes[boxesNumber];

    /*
    vec3 size;
    vec3 rotation;
    vec3 position;
    Material material;
    */

    vec2 uvPosition;
    int faceNumber;

    boxes[0] = Box(vec3(0.2), vec3(0., 0., 0.), vec3(1., 0.1, 0.), Material(vec3(1.), 1., 5, 0.04, 0.));

    for (int i = 0; i < boxesNumber; i++)
    {
        tempHit = boxIntersection(ray, boxes[i], uvPosition, faceNumber);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    
    // PORTAL
    const int portalsNumber = 2;
    Portal portals[portalsNumber];

    portals[0] = Portal(
                    Plane(vec2(0.5), vec3(0., 0., PI / 4), vec3(2.5, -1.5, 0.), nullMaterial)
                );
    portals[1] = Portal(
                    Plane(vec2(0.5), vec3(0., 0., -PI / 4), vec3(-2.5, -1.5, 0.), nullMaterial)
                );

    const int portalsConnectionsNumber = 1;
    PortalConnection portalConnections[portalsConnectionsNumber];

    portalConnections[0] = PortalConnection(portals[0], portals[1]);

    for (int i = 0; i < portalsNumber; i++)
    {
        tempHit = planeIntersection(ray, portals[i].base);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            tempHit.portal = true;
            hit = tempHit;
        }
    }

    return hit;
}

// "Edges shadow"
// Schlick's approximation
float fresnel(vec3 normal, vec3 rayDirection, Material material)
{
    return (pow(1. - clamp(dot(normal, rayDirection), 0., 1.), material.specularPower) * (1. - material.specularIntensity) + material.specularIntensity) * material.reflectionIntensity;
}

// FOR SPHERE AND SUN LIGHT
vec3 lightCast(vec3 hitPosition, vec3 normal, SunLight light)
{
    Hit shadowHit = sceneIntersection(Ray(hitPosition + epsilon * -light.direction, -light.direction));

    if (!shadowHit.hitObject)
    {
        return clamp(dot(normal, -light.direction), 0., 1.) * light.color;
    }

    return vec3(0.);
}

vec3 radiance(Ray ray, SunLight sunLight)
{
    vec3 color = vec3(0.);
    float attenuation = 1.;

    for (int i = 0; i <= RAY_ITERATIONS; i++)
    {
        if (attenuation < 1e-3)
        {
            continue;
        }

        Hit hit = sceneIntersection(ray);

        if (hit.hitObject)
        {  
            if (!hit.portal)
            {
                float fresnel = fresnel(hit.normal, -ray.direction, hit.material);

                color += vec3(1. - fresnel) * attenuation * hit.material.color * lightCast(hit.position, hit.normal, sunLight);
                
                attenuation *= fresnel;

                vec3 newDirection = reflect(ray.direction, hit.normal);
                ray = Ray(hit.position + epsilon * newDirection, newDirection);
            }
            else
            {
                color = vec3(0.);
            }
        }
        else
        {
            color += attenuation * sky(ray, normalize(-sunLight.direction), false);
        }
    }

    return color;
}

//** MAIN **

void main()
{
    vec2 uv = 2. * gl_FragCoord.xy / screenSize.xy - 1.;

    Camera camera = Camera(1.5, camera_rotation, camera_position);
    SunLight sunLight = SunLight(1., normalize(vec3(2., -2., -1.)), vec3(1.));

    vec3 color = vec3(0.);
    vec2 msaa = vec2(1.) / MSAA;
    vec3 offset;
    vec3 direction;

    RotationMatrixComponents rotationComponents = rotationMatrix(camera.rotation);
    mat4 rotationTransform = rotationComponents.y * rotationComponents.x;

    for (int i = 0; i < MSAA * MSAA; i++)
    {
        offset = vec3(msaa.xy * i / screenSize.y, 0.);

        direction = vec3(screenSize.x / screenSize.y * uv.x, uv.y, -camera.focal) + offset;
        direction = (rotationTransform * vec4(direction, 0.)).xyz;

        Ray ray = Ray(camera.position, normalize(direction));
        color += radiance(ray, sunLight) / (MSAA * MSAA);
    }

    outputColor = vec4(pow(color, vec3(1. / gammaCorrection)), 1.0);
}