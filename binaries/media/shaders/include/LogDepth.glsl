
#include Mesh3dUniforms.glsl
smooth in vec3 positionModelSpace;
smooth in vec3 positionWorldSpace;
smooth in vec3 normal;
smooth in vec3 barycentric;
flat in int instanceId;
uniform int UseNormalMap;
uniform int UseBumpMap;
void updateDepth(){
	float depth = distance(positionWorldSpace, CameraPosition);
	float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*FarPlane + 1.0f);
	gl_FragDepth = badass_depth;
}
float getDepth(){
	float depth = distance(positionWorldSpace, CameraPosition);
	float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*FarPlane + 1.0f);
	return badass_depth;
}
float toLogDepth(float depth){
	float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*FarPlane + 1.0f);
	return badass_depth;
}
#define MATH_E 2.7182818284
float reverseLog(float dd){
	return pow(MATH_E, dd - 1.0) / LogEnchacer;
}
/*
void updateDepth(){
	float depth = distance(positionWorldSpace, CameraPosition);
	gl_FragDepth = depth;
}
float getDepth(){
	float depth = distance(positionWorldSpace, CameraPosition);
	return depth;
}*/