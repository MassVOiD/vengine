
#include Mesh3dUniforms.glsl
smooth in vec3 positionModelSpace;
smooth in vec3 positionWorldSpace;
smooth in vec3 normal;
smooth in vec3 barycentric;
flat in int instanceId;
uniform int UseNormalMap;
uniform int UseBumpMap;

//log2(x+1) / log2(100+1)
//y=2^( (X)* log2(100+1))

float toLogDepthEx(float depth, float far){
	//float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*far + 1.0f);
    float badass_depth = log2(max(1e-6, 1.0 + depth)) / (log2(far+1.0));
	return badass_depth;
}
float toLogDepth(float depth){
	return toLogDepthEx(depth, FarPlane);
}
void updateDepth(){
	float depth = distance(positionWorldSpace, CameraPosition);
	float badass_depth = toLogDepth(depth);
	gl_FragDepth = badass_depth;
}
float getDepth(){
	float depth = distance(positionWorldSpace, CameraPosition);
	float badass_depth = toLogDepth(depth);
	return badass_depth;
}
#define MATH_E 2.7182818284
float reverseLogEx(float dd, float far){
	return pow(2, dd * log2(far+1.0)) - 1;
}
float reverseLog(float dd){
	return reverseLogEx(dd, FarPlane);
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