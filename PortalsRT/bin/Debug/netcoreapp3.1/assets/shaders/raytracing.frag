#version 330

#define RAY_ITERATIONS 4
#define MSAA 2

const float gammaCorrection = 2.2;
const float epsilon = 4e-4;

uniform vec3 screenSize;
uniform vec3 cameraRotation;
uniform vec3 cameraPosition;
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
    vec3 direction;
    vec3 position;
    Material material;
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
};

Hit nullHit = Hit(Material(vec3(0.), 0., 0., 0., 0.), 10000, vec3(0.), vec3(0.), false);

//** MATH **
mat4 rotationMatrix(vec3 rotation)
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

    return rotationX * rotationY * rotationZ;
}

//** INTERSECTIONS **
Hit boxIntersection(Ray ray, Box box, /*in mat4 txi -- inverse, in vec3 rad -- size/2 -- from box, out vec2 oT, out vec3 oN, */ out vec2 uvPosition, out int faceNumber) 
{
    mat4 position = mat4(
        1,              0,              0,              0,
        0,              1,              0,              0,
        0,              0,              1,              0,
        box.position.x, box.position.y, box.position.z, 1
    );

    mat4 transformWorldToBox = inverse(rotationMatrix(box.rotation) * position);

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
	
    if( distanceToNear > distanceToFar || distanceToFar < 0.0)
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

    // oT = vec2(distanceToNear, distanceToFar);
    
    return Hit(box.material, distanceToNear, ray.origin + ray.direction * distanceToNear, normal, true);
}

Hit planeIntersection(Ray ray, Plane plane)
{
    float dotNormal = dot(plane.direction, ray.direction);

    if (dotNormal > 0.)
    {
        return nullHit;
    }

    float distanceTo = -(dot(ray.origin, plane.direction)) / dotNormal;

    vec3 hitPosition = ray.origin + distanceTo * ray.direction;

    vec3 distanceVector = hitPosition - plane.position;

    if (abs(distanceVector.z) > plane.size.y || abs(distanceVector.x) > plane.size.x)
    {
        return nullHit;
    }

    return Hit(plane.material, distanceTo, hitPosition, plane.direction, true);
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

    return Hit(sphere.material, distanceTo, hitPosition, (hitPosition - sphere.position) / sphere.radius, true);
}

//** RAYTRACING **

vec3 sky(vec3 direction)
{
    return vec3(0.001) * direction.y;
}

Hit sceneIntersection(Ray ray)
{
    Hit hit = nullHit;
    Hit tempHit = nullHit;

    // SPHERE
    const int spheresNumber = 2;
    Sphere spheres[spheresNumber];
    spheres[0] = Sphere(1., vec3(0., 1., -1.), Material(vec3(1.), 0.2, 5, 0.04, 0.));
    spheres[1] = Sphere(0.75, vec3(-1., 1., -0.5), Material(vec3(1.), 0.2, 3, 0.04, 0.));

    for (int i = 0; i < spheresNumber; i++)
    {
        tempHit = sphereIntersection(ray, spheres[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // PLANE
    const int planesNumber = 1;
    Plane planes[planesNumber];

    planes[0] = Plane(vec2(2.5), vec3(0., 1., 0.), vec3(0.), Material(vec3(1.), 0.2, 5, 0.04, 0.));

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

    boxes[0] = Box(vec3(1.), vec3(0., 0., 0.), vec3(2., 0.5, 0.), Material(vec3(1.), 0.2, 5, 0.04, 0.));

    for (int i = 0; i < boxesNumber; i++)
    {
        tempHit = boxIntersection(ray, boxes[i], uvPosition, faceNumber);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }
    // PORTAL
    // ...

    return hit;
}

// "Edges shadow"
// Schlick's approximation
float fresnel(vec3 normal, vec3 rayDirection, Material material)
{
    return pow(1. - clamp(dot(normal, rayDirection), 0., 1.), material.specularPower) * (1. - material.specularIntensity) + material.specularIntensity;
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
    vec3 attenuation = vec3(1.);

    for (int i = 0; i <= RAY_ITERATIONS; i++)
    {
        Hit hit = sceneIntersection(ray);

        if (hit.hitObject)
        {  
            float fresnel = fresnel(hit.normal, -ray.direction, hit.material);

            color += vec3(1. - fresnel) * attenuation * hit.material.color * lightCast(hit.position, hit.normal, sunLight);
            
            attenuation *= fresnel;

            vec3 newDirection = reflect(ray.direction, hit.normal);
            ray = Ray(hit.position + epsilon * newDirection, newDirection);
        }
        else
        {
            color += attenuation * sky(ray.direction);
        }
    }

    return color;
}

//** MAIN **

void main()
{
    vec2 uv = 2. * gl_FragCoord.xy / screenSize.xy - 1.;

    Camera camera = Camera(1., cameraRotation, cameraPosition);
    SunLight sunLight = SunLight(1., normalize(vec3(2., -1., -1.)), vec3(1.));

    vec3 color = vec3(0.);
    vec2 msaa = vec2(1.) / MSAA;
    vec3 offset;
    vec3 direction;

    for (int i = 0; i < MSAA * MSAA; i++)
    {
        offset = vec3(msaa.xy * i / screenSize.y, 0.);

        direction = vec3(screenSize.x/screenSize.y * uv.x, uv.y, -camera.focal) + offset;

        direction = (rotationMatrix(camera.rotation) * vec4(direction, 0.)).xyz;

        Ray ray = Ray(camera.position, normalize(direction));
        color += radiance(ray, sunLight) / (MSAA * MSAA);
    }

    outputColor = vec4(pow(color, vec3(1./gammaCorrection)), 1.0);
}