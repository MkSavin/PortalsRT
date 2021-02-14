#version 330

#define RAY_ITERATIONS 2

const float gammaCorrection = 2.2;
const float epsilon = 4e-4;

uniform vec3 screenSize;
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

//** INTERSECTIONS **

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
    return vec3(0.001);// * direction.y;
}

Hit sceneIntersection(Ray ray)
{
    Hit hit = nullHit;
    Hit tempHit = nullHit;

    // SPHERE
    const int spheresNumber = 1;
    Sphere spheres[spheresNumber];
    spheres[0] = Sphere(1., vec3(0., 1., 0.), Material(vec3(1.), 0.2, 5, 0.04, 0.));
    // spheres[1] = Sphere(0.75, vec3(0.5, 1., 0.), Material(vec3(1.), 0.2, 3, 0.04, 0.));

    for (int i = 0; i < spheresNumber; i++)
    {
        tempHit = sphereIntersection(ray, spheres[i]);

        if (tempHit.hitObject && (!hit.hitObject || tempHit.distanceTo < hit.distanceTo))
        {
            hit = tempHit;
        }
    }

    // BOX
    // ...
    // PLANE
    // ...
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

    Camera camera = Camera(1, vec3(0, 0, 0), vec3(0., 1.1, 4.));

    SunLight sunLight = SunLight(1., normalize(vec3(2., -1., -1.)), vec3(1.));

    Ray ray = Ray(camera.position, normalize(vec3(screenSize.x/screenSize.y * uv.x, uv.y, -camera.focal)));
    // Ray ray = Ray(camera.position, normalize(vec3(uv.x, uv.y, -camera.focal)));

    vec3 color = radiance(ray, sunLight);

    outputColor = vec4(pow(color, vec3(1./gammaCorrection)), 1.0);
}