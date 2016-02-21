
#include_once Mesh3dUniforms.glsl

struct FragmentData
{
	vec3 diffuseColor;
	vec3 specularColor;
	vec3 normal;
	vec3 tangent;
	vec3 worldPos;
	vec3 cameraPos;
	float cameraDistance;
	float alpha;
	float roughness;
	float bump;
};


in Data {
#include InOutStageLayout.glsl
} Input;
uniform int UseNormalMap;
uniform int UseBumpMap;
#define FarPlane (10000.0f)

// it returns the cosine
float calculateAngleOccupied(float dist, float radius){
    float pp = sqrt(dist*dist+radius*radius);
    return 2*(dist/pp);
}

bool intersectPoint(vec3 origin, vec3 direction, vec3 position, float radius){
    float angle = dot(position - origin, direction);
    float cosine = calculateAngleOccupied(distance(position, origin), radius);
    return angle <= cosine;
}
