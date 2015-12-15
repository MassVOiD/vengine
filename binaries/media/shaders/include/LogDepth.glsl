
#include_once Mesh3dUniforms.glsl


in Data {
#include InOutStageLayout.glsl
} Input;
uniform int UseNormalMap;
uniform int UseBumpMap;

//log2(x+1) / log2(100+1)
//y=2^( (X)* log2(100+1))

#define FarPlane (10000.0f)

float toLogDepthEx(float depth, float far){
	//float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*far + 1.0f);
    float badass_depth = log2(max(1e-6, 1.0 + depth)) / (log2(far));
    //float badass_depth = log2(1.0 + depth) / log2(far+1.0);
	return badass_depth;
}
float toLogDepth(float depth){
	return toLogDepthEx(depth, FarPlane);
}
#ifndef NO_FS
void updateDepth(){
	float depth = distance(Input.WorldPos, CameraPosition);
	float badass_depth = toLogDepth(depth);
	gl_FragDepth = badass_depth;
}

void updateDepthFromWorldPos(vec3 w){
	float depth = distance(w, CameraPosition);
	float badass_depth = toLogDepth(depth);
	gl_FragDepth = badass_depth;
}
#endif
float getDepth(){
	float depth = distance(Input.WorldPos, CameraPosition);
	float badass_depth = toLogDepth(depth);
	return badass_depth;
}
#define MATH_E 2.7182818284
// dd = log2(depth+1) / lg
float reverseLogEx(float dd, float far){
	//return pow(2, dd * log2(far+1.0) ) - 1;
	return pow(2, dd * log2(far)) - 1.0;
}
float reverseLog(float dd){
	return reverseLogEx(dd, FarPlane);
}

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
