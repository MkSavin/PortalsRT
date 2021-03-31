#version 420

// Rendering
#define RAY_ITERATIONS 4
#define MSAA 1
#define SAMPLES_PER_MSAA_LEVEL 1
#define DENOISE .9//.75//.96

// Ray Marching
#define RAY_MARCHING_STEPS 128

// Math
#define PI 3.14159265359

// Sparks
#define SPEED_FACTOR 1.5
#define LENGTH_FACTOR 1.5
#define GROUP_FACTOR 1.0
#define SPREAD_FACTOR 0.1
#define MIN_ANGLE 0.1
#define RAND_FACTOR 0.0

#define MAX_SPARKS 12

// Julia
#define JULIA_ITERATIONS 8//10

uniform vec3 sparksPositions[MAX_SPARKS];

const float gammaCorrection = 2.2;
const float epsilon = 4e-4;

uniform int frameNumber;
uniform float frameRate;
uniform vec3 screenSize;
uniform bool camera_moved;
uniform vec3 camera_rotation;
uniform vec3 camera_position;
layout(binding=0) uniform sampler2D accumTexture;
layout(binding=1) uniform sampler2D bluenoiseMask;

layout(location = 0) out vec4 outputColor;

//** Math **
float seed = 0;

float bluenoise(vec2 uv)
{
    uv += 12.2130 * vec2(frameNumber);
    return texture(bluenoiseMask, (uv + 0.5) / vec2(192.).xy, 0.0).x;
}

lowp float hash1() {
    return fract(sin(seed += 0.1) * 43758.5453123);
}

lowp vec2 hash2() {
    return fract(sin(vec2(seed += 0.1, seed += 0.1)) * vec2(43758.5453123, 22578.1459123));
}

lowp vec3 hash3() {
    return fract(sin(vec3(seed += 0.1, seed += 0.1, seed += 0.1)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
}

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
    bool fullBsdf;
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

struct Spark
{
    vec3 color;
    vec3 position;
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

struct Julia
{
    Box bounds;
    vec4 constants;
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
    bool raymarching;
};

Material nullMaterial = Material(vec3(0.), 0., 0., 0., 0., false);
Hit nullHit = Hit(nullMaterial, 10000, vec3(0.), vec3(0.), false, false, false);

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
	
    if (distanceToNear > distanceToFar || distanceToFar < 0.0)
    {
        return nullHit;
    }

    vec3 normal;

    mat4 transformBoxToWorld = inverse(transformWorldToBox);

    // Compute normal (in world space), UV position and face number
    if (point1.x > point1.y && point1.x > point1.z)
    { 
        normal = transformBoxToWorld[0].xyz * side.x; 
        uvPosition = rayOrigin.yz + rayDirection.yz * point1.x; 
        faceNumber = (1 + int(side.x)) / 2;
    }
    else if (point1.y > point1.z)
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

    return Hit(box.material, distanceToNear, ray.origin + ray.direction * distanceToNear, normal, true, false, false);
}

Hit planeIntersection(Ray ray, Plane plane)
{
    vec3 planeDirection = vec3(0., 1., 0.);

    RotationMatrixComponents rotationComponents = rotationMatrix(plane.rotation);
    mat4 rotationMatrix = rotationComponents.x * rotationComponents.y * rotationComponents.z;

    planeDirection = (rotationMatrix * vec4(planeDirection, 0.)).xyz;

    vec3 distanceVector = plane.position - ray.origin;

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

    return Hit(plane.material, distanceTo, hitPosition, planeDirection, true, false, false);
}

Hit sparkIntersection(Ray ray, Spark spark)
{
    if (dot((spark.position - ray.origin), ray.direction) / (length(ray.origin - spark.position) * length(ray.direction)) < 1 - 1e-5)
    {
        return nullHit;
    }

    return Hit(Material(vec3(1.), 0., 0., 0., 1., false), length(spark.position - ray.origin), spark.position, normalize(spark.position - ray.origin), true, false, false);
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

    return Hit(sphere.material, distanceTo, hitPosition, (hitPosition - sphere.position) / sphere.radius, true, false, false);
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
    
    return clamp(color * 0.085, 0., 1.);
}

const int portalsNumber = 4;
Portal portals[portalsNumber];

Portal nullPortal = Portal(Plane(vec2(0.), vec3(0.), vec3(0.), nullMaterial));

const int portalsConnectionsNumber = 2;
PortalConnection portalConnections[portalsConnectionsNumber];

PortalConnection nullPortalConnection = PortalConnection(nullPortal, nullPortal);

Julia julia = Julia(Box(vec3(32.), vec3(0., 0., 0.), vec3(6.2, 0.5, 0), Material(vec3(1.), 0., 5, 0.2, 0., true)), vec4(-0.137, -0.630, -0.475, -0.046));

Hit sceneIntersection(Ray ray, out Portal lastHittedPortal, out Hit withoutRayMarching)
{
    lastHittedPortal = nullPortal;
    Hit hit = nullHit;
    Hit tempHit = nullHit;

    // SPHERE
    const int spheresNumber = 2;
    Sphere spheres[spheresNumber];
    spheres[0] = Sphere(0.25, vec3(0., 1., -1.), Material(vec3(1.), 1., 5, 0.2, 0., true));
    spheres[1] = Sphere(0.1, vec3(-1., 1., -0.5), Material(vec3(1., 0., 0.), 1., 3, 0.01, 0., true));

    for (int i = 0; i < spheresNumber; i++)
    {
        tempHit = sphereIntersection(ray, spheres[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // PLANE
    Material roomMaterial = Material(vec3(1.), 0., 5, 0.2, 0., true);

    const int planesNumber = 14;
    Plane planes[planesNumber];

    float roomOffset = 0.;

    planes[0] = Plane(vec2(2., 4.), vec3(0.), vec3(roomOffset + 0., 0., 0.), roomMaterial);
    planes[1] = Plane(vec2(1., 0.5), vec3(0., 0., PI / 2), vec3(roomOffset + 2., 1., 3.5), roomMaterial);
    planes[2] = Plane(vec2(1., 0.5), vec3(0., 0., PI / 2), vec3(roomOffset + 2., 1., -3.5), roomMaterial);
    planes[3] = Plane(vec2(1., 1.), vec3(0., 0., PI / 2), vec3(roomOffset + 2., 1., 0), roomMaterial);
    planes[4] = Plane(vec2(1., 4.), vec3(0., 0., -PI / 2), vec3(roomOffset + -2., 1., 0), roomMaterial);
    planes[5] = Plane(vec2(1., 2.), vec3(0., PI / 2, PI / 2), vec3(roomOffset + 0., 1., -4.), roomMaterial);
    planes[6] = Plane(vec2(1., 2.), vec3(0., -PI / 2., -PI / 2), vec3(roomOffset + 0., 1., 4.), roomMaterial);

    roomOffset = 6;

    planes[7] = Plane(vec2(2., 4.), vec3(0.), vec3(roomOffset + 0., 0., 0.), roomMaterial);
    planes[8] = Plane(vec2(1., 0.5), vec3(0., 0., PI / 2), vec3(roomOffset - 2., 1., 3.5), roomMaterial);
    planes[9] = Plane(vec2(1., 0.5), vec3(0., 0., PI / 2), vec3(roomOffset - 2., 1., -3.5), roomMaterial);
    planes[10] = Plane(vec2(1., 1.), vec3(0., 0., PI / 2), vec3(roomOffset - 2., 1., 0), roomMaterial);
    planes[11] = Plane(vec2(1., 4.), vec3(0., 0., -PI / 2), vec3(roomOffset + 2., 1., 0), roomMaterial);
    planes[12] = Plane(vec2(1., 2.), vec3(0., PI / 2, PI / 2), vec3(roomOffset + 0., 1., -4.), roomMaterial);
    planes[13] = Plane(vec2(1., 2.), vec3(0., -PI / 2., -PI / 2), vec3(roomOffset + 0., 1., 4.), roomMaterial);

    for (int i = 0; i < planesNumber; i++)
    {
        tempHit = planeIntersection(ray, planes[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // Sparks
    Spark sparks[MAX_SPARKS];

    for (int i = 0; i < MAX_SPARKS; i++)
    {
        sparks[i] = Spark(vec3(1.), sparksPositions[i - planesNumber]);
    }

    for (int i = 0; i < MAX_SPARKS; i++)
    {
        tempHit = sparkIntersection(ray, sparks[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // BOX
    const int boxesNumber = 2;
    Box boxes[boxesNumber];

    /*
    vec3 size;
    vec3 rotation;
    vec3 position;
    Material material;
    */

    vec2 uvPosition;
    int faceNumber;

    boxes[0] = Box(vec3(1.), vec3(0., 0., 0.), vec3(0.4, 0.5, 0.), Material(vec3(1., 1., 0.), 1., 5, 0.04, 0., true));
    boxes[1] = Box(vec3(1.), vec3(0., 0., 0.), vec3(0.2, 0.5, 1.2), Material(vec3(1., 0., 1.), 1., 5, 0.04, 0., true));

    for (int i = 0; i < boxesNumber; i++)
    {
        tempHit = boxIntersection(ray, boxes[i], uvPosition, faceNumber);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // PORTAL
    roomOffset = 0.;

    portals[0] = Portal(
                    Plane(vec2(1.), vec3(0., 0., PI / 2), vec3(roomOffset + 2., 1., 2.), nullMaterial)
                );
    portals[1] = Portal(
                    Plane(vec2(1.), vec3(PI, 0., PI / 2), vec3(roomOffset + 2., 1., -2.), nullMaterial)
                );

    roomOffset = 6;

    portals[2] = Portal(
                    Plane(vec2(1.), vec3(0., 0., PI / 2), vec3(roomOffset - 2., 1., 2.), nullMaterial)
                );
    portals[3] = Portal(
                    Plane(vec2(1.), vec3(PI, 0., PI / 2), vec3(roomOffset - 2., 1., -2.), nullMaterial)
                );

    portalConnections[0] = PortalConnection(portals[0], portals[2]);
    portalConnections[1] = PortalConnection(portals[1], portals[3]);

    for (int i = 0; i < portalsNumber; i++)
    {
        tempHit = planeIntersection(ray, portals[i].base);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            lastHittedPortal = portals[i];
            tempHit.portal = true;
            hit = tempHit;
        }
    }

    withoutRayMarching = hit;

    // JULIA
    tempHit = boxIntersection(ray, julia.bounds, uvPosition, faceNumber);

    if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
    {
        hit = tempHit;
        hit.raymarching = true;
    }

    return hit;
}

// Raymarching

vec4 qSquare(vec4 a)
{
    return vec4(a.x * a.x - dot(a.yzw, a.yzw), 2. * a.x * a.yzw);
}

float juliaDistance(Julia julia, vec3 p) {
    vec4 f = vec4(p * (1. / julia.bounds.size + 1.) - julia.bounds.position, 0.0);
    
    float fp2 = 1.0;

    for (int i = 0; i < JULIA_ITERATIONS; i++)
    {
        fp2 *= 4.0 * dot(f,f);
		f = qSquare(f) + julia.constants; // Julia formula constants
        
        if (dot(f,f) > 4.0)
        {
            break;
        }
    }
    
    float r = length(f);
    
    return 0.5 * log(r) * (r / sqrt(fp2)) / length(1. / julia.bounds.size + 1.);
}

// Method 1
vec3 juliaNormal(vec3 position, float distanceToObject)
{
    vec3 normal;

    normal.x = juliaDistance(julia, vec3(position.x + epsilon, position.y, position.z));
    normal.y = juliaDistance(julia, vec3(position.x, position.y + epsilon, position.z));
    normal.z = juliaDistance(julia, vec3(position.x, position.y, position.z + epsilon));

    return normalize(normal - distanceToObject);
}

Hit sceneIntersectionRayMarching(Ray ray)
{
    vec3 e = vec3(0.0005, 0., 0.);

    vec3 position;
    float distanceToRayOrigin = 0.;
    float distanceToObject;
    vec3 normal;

    for (int i = 0; i < RAY_MARCHING_STEPS; i++)
    {
        position = ray.origin + ray.direction * distanceToRayOrigin;
        
        distanceToObject = juliaDistance(julia, position);
        
        if (distanceToRayOrigin > 15.)
        {
            break;
        }

        if (distanceToObject <= 0.)
        {
            return Hit(julia.bounds.material, distanceToRayOrigin, position, juliaNormal(position, distanceToObject), true, false, true);
        }

        distanceToRayOrigin += max(distanceToObject, epsilon);
    }

    return nullHit;
}

// Colors computation

// "Edges shadow"
// Schlick's approximation
float fresnel(vec3 normal, vec3 rayDirection, Material material)
{
    return pow(1. - clamp(dot(normal, rayDirection), 0., 1.), material.specularPower) * (1. - material.specularIntensity) + material.specularIntensity;
}

// FOR SPHERE AND SUN LIGHT
vec3 lightCast(vec3 hitPosition, vec3 normal, SunLight light)
{
    Ray ray = Ray(hitPosition + epsilon * -light.direction, -light.direction);
    Portal portal;
    Hit nonRaymarched;
    Hit hit = sceneIntersection(ray, portal, nonRaymarched);

    if (hit.hitObject && hit.raymarching)
    {
        hit = sceneIntersectionRayMarching(ray);
    }

    if (hit == nullHit)
    {
        hit = nonRaymarched;
    }

    if (!hit.hitObject)
    {
        return clamp(dot(normal, -light.direction), 0., 1.) * light.color;
    }

    return vec3(0.);
}

vec2 sampleDisk(vec2 uv)
{
	float theta = 2.0 * PI * uv.x;
	float r = sqrt(uv.y);
	return vec2(cos(theta), sin(theta)) * r;
}

vec3 cosHemisphere(vec2 uv)
{
	vec2 disk = sampleDisk(uv);
	return vec3(disk.x, sqrt(max(0.0, 1.0 - dot(disk, disk))), disk.y);
}

mat3 onb(vec3 normal)
{
	mat3 ret;
	ret[1] = normal;

	if(normal.z < -0.999805696) 
    {
		ret[0] = vec3(0.0, -1.0, 0.0);
		ret[2] = vec3(-1.0, 0.0, 0.0);
	} 
    else 
    {
		float a = 1.0 / (1.0 + normal.z);
		float b = -normal.x * normal.y * a;
		ret[0] = vec3(1.0 - normal.x * normal.x * a, b, -normal.x);
		ret[2] = vec3(b, 1.0 - normal.y * normal.y * a, -normal.y);
	}
    
	return ret;
}

Portal findTargetPortal(Portal basePortal)
{
    Portal targetPortal = nullPortal;
    PortalConnection basePortalConnection = nullPortalConnection;

    if (basePortal != nullPortal)
    {
        for (int p = 0; p < portalsConnectionsNumber; p++)
        {
            if (portalConnections[p].a == basePortal || portalConnections[p].b == basePortal)
            {
                basePortalConnection = portalConnections[p];
                continue;
            }
        }

        if (basePortalConnection != nullPortalConnection)
        {
            if (basePortalConnection.a == basePortal)
            {
                targetPortal = basePortalConnection.b;
            }
            else
            {
                targetPortal = basePortalConnection.a;
            }
        }
    }

    return targetPortal;
}

Ray portalizeHittedRay(Ray ray, Hit hit, Portal basePortal, Portal targetPortal)
{
    // Transform hit position and ray direction to local of first (base) portal
    RotationMatrixComponents rotationComponents = rotationMatrix(basePortal.base.rotation);
    mat4 transformRotationMatrix = rotationComponents.x * rotationComponents.y * rotationComponents.z;

    vec3 newDirection = (transformRotationMatrix * vec4(ray.direction, 0.)).xyz;

    hit.position -= basePortal.base.position;

    vec3 relativeHitPosition = (transformRotationMatrix * vec4(hit.position, 0.)).xyz;
    relativeHitPosition /= vec3(basePortal.base.size, 1.);

    // Transform hit position and ray direction to absolute by using second (target) portal transform
    rotationComponents = rotationMatrix(targetPortal.base.rotation);
    transformRotationMatrix = inverse(rotationComponents.x * rotationComponents.y * rotationComponents.z);

    vec3 relativeRayOrigin = relativeHitPosition * vec3(targetPortal.base.size, 1.);
    relativeRayOrigin = (transformRotationMatrix * vec4(relativeRayOrigin, 0.)).xyz;

    vec3 absoluteRayOrigin = relativeRayOrigin + targetPortal.base.position;

    newDirection = (transformRotationMatrix * vec4(newDirection, 0.)).xyz;

    // Return ray
    return Ray(absoluteRayOrigin + epsilon * newDirection, newDirection);
}

vec3 radiance(Ray ray, SunLight sunLight)
{
    vec3 backBrdf = vec3(1.);
    vec3 color = vec3(0.);
    vec3 tempColor = vec3(0.);
    float attenuation = 1.;

    bool stopIterations = false;

    Portal lastPortal;
    Portal targetPortal;
    PortalConnection lastPortalConnection = nullPortalConnection;

    Hit nonRaymarched;
    Hit hit;

    int bsdfDepth = 0;

    for (int i = 0; i <= RAY_ITERATIONS; i++)
    {
        if (attenuation < 1e-2)
        {
            continue;
        }

        if (stopIterations)
        {
            break;
        }

        hit = sceneIntersection(ray, lastPortal, nonRaymarched);

        if (hit.hitObject && hit.raymarching && !(bsdfDepth > 1))
        {
            hit = sceneIntersectionRayMarching(ray);
        }

        if (hit == nullHit || hit.distanceTo > nonRaymarched.distanceTo || (bsdfDepth > 1))
        {
            hit = nonRaymarched;
        }

        if (hit.hitObject)
        {  
            if (hit.portal)
            {
                // Error color
                tempColor = vec3(1., 0., 0.);
                stopIterations = true;

                if ((targetPortal = findTargetPortal(lastPortal)) != nullPortal)
                {
                    ray = portalizeHittedRay(ray, hit, lastPortal, targetPortal);

                    tempColor = vec3(0.);
                    stopIterations = false;
                }

                color += tempColor;//vec3(0.);
            }
            /* else if (hit.raymarching)
            {
                color = vec3(1., 1., 0.5);
            } */
            else
            {
                tempColor = hit.material.color;

                float currentFresnel = fresnel(hit.normal, -ray.direction, hit.material);

                vec3 brdf = tempColor;

                tempColor = vec3(1. - currentFresnel) * brdf * backBrdf * attenuation * lightCast(hit.position, hit.normal, sunLight);
                
                tempColor = mix(tempColor, brdf, hit.material.glowness);

                attenuation *= currentFresnel;

                if (hit.material.fullBsdf)
                {
                    vec3 bsdfDirection = reflect(ray.direction, hit.normal);
                    ray = Ray(hit.position + epsilon * bsdfDirection, bsdfDirection);

                    mat3 currentOnb = onb(hit.normal);
                    bsdfDirection = mix(normalize(currentOnb * cosHemisphere(hash2())), bsdfDirection, hit.material.reflectionIntensity);

                    Ray nextRay = Ray(hit.position + epsilon * bsdfDirection, bsdfDirection);

                    bsdfDepth++;

                    backBrdf *= brdf;
                    ray = nextRay;
                }

                color += tempColor;
            }
        }
        else
        {
            color += attenuation * sky(ray, normalize(-sunLight.direction), false);
            stopIterations = true;
        }
    }

    return clamp(color, 0., 1.);
}

//** MAIN **

void main()
{
    vec2 uv = 2. * gl_FragCoord.xy / screenSize.xy - 1.;

    Camera camera = Camera(1.5, camera_rotation, camera_position);
    SunLight sunLight = SunLight(1., normalize(vec3(2., -2., -1.)), vec3(1.));

    seed = bluenoise(gl_FragCoord.xy);

    vec3 color = vec3(0.);
    vec2 msaa = vec2(1.) / MSAA;
    vec2 rnd;
    vec3 offset;
    vec3 direction;
    vec2 screenspaceDirection;

    RotationMatrixComponents rotationComponents = rotationMatrix(camera.rotation);
    mat4 rotationTransform = rotationComponents.y * rotationComponents.x;

    // Getting color of pixel by every sample
    for (int i = 0; i < MSAA * MSAA; i++)
    {
        for (int j = 0; j < SAMPLES_PER_MSAA_LEVEL; j++)
        {
            offset = vec3(msaa.xy * i / screenSize.y, 0.);

            rnd = hash2();

            screenspaceDirection = vec2(screenSize.x / screenSize.y * uv.x, uv.y);
            direction = vec3(screenspaceDirection + rnd.x * dFdx(uv) + rnd.y * dFdy(uv), -camera.focal) + offset;// + vec3(rnd / 1000, 0);
            direction = (rotationTransform * vec4(direction, 0.)).xyz;

            Ray ray = Ray(camera.position, normalize(direction));
            color += radiance(ray, sunLight);

            seed = mod(seed * 1.1234567893490423, 13.);
        }
    }
    
    // Color normalization
    color = pow(color / (MSAA * MSAA) / float(SAMPLES_PER_MSAA_LEVEL), vec3(1. / gammaCorrection));

    // Denoising
    vec3 accumColor = texture(accumTexture, gl_FragCoord.xy / screenSize.xy).xyz;

    #ifdef DENOISE
    if (frameNumber > 0 && !camera_moved)
    {
        color = color * (1. - DENOISE) + accumColor * DENOISE;
    }
    #endif

    // Vignette
    // vec2 vignetteUv = gl_FragCoord.xy / screenSize.xy;
    // color *= 0.9 + 0.1 * pow(16.0 * vignetteUv.x * vignetteUv.y * (1.0 - vignetteUv.x) * (1.0 - vignetteUv.y), 0.1);  

    outputColor = vec4(color, 1.);
}
