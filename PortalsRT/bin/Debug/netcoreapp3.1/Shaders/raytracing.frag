#version 330

#define RAY_ITERATIONS 4

uniform vec3 screenSize;
out vec4 outputColor;

float gammaCorrection = 2.2;

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

struct Light
{
    float intensity;
    vec3 position;
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
    return Hit(sphere.material, 10, vec3(0.), vec3(0.), true);
}

//** RAYTRACING **

vec3 sky(vec3 direction)
{
    return vec3(.001) * direction.y;
}

Hit sceneIntersection(Ray ray)
{
    Hit hit = nullHit;
    Hit tempHit = nullHit;

    // SPHERE
    const int spheresNumber = 2;
    Sphere spheres[spheresNumber];
    spheres[0] = Sphere(1., vec3(0, 0.5, 10), Material(vec3(1.), 0.2, 3, 0.5, 0.));
    spheres[1] = Sphere(0.75, vec3(0.5, 0.5, 10), Material(vec3(1.), 0.2, 3, 0.5, 0.));

    for (int i = 0; i <= spheresNumber; i++)
    {
        tempHit = sphereIntersection(ray, spheres[i]);

        if (!hit.hitObject || tempHit.distanceTo < hit.distanceTo)
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

vec3 radiance(Ray ray)
{
    vec3 color = vec3(0.0);
    vec3 attenuation = vec3(1.);

    for (int i = 0; i <= RAY_ITERATIONS; i++)
    {
        Hit hit = sceneIntersection(ray);

        if (hit.hitObject)
        {

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

    Camera camera = Camera(1, vec3(0, 0, 0), vec3(0, 0, 0));
    Ray ray = Ray(camera.position, normalize(vec3(uv.x, uv.y, -camera.focal)));

    vec3 color = radiance(ray);

    outputColor = vec4(pow(color, vec3(1./gammaCorrection)), 1.0);
}