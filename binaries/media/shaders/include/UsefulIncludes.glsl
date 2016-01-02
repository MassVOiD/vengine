vec3 ToCameraSpace(vec3 position){
    return position + -CameraPosition;
}

vec3 FromCameraSpace(vec3 position){
    return position - -CameraPosition;
}

vec3 ProjectPointOnLine(vec3 p, vec3 a, vec3 b){
    vec3 u = normalize(b - a);
    return dot(u, p - a) * u + a;
}

struct RSMLight
{
    vec4 Position;
    vec4 normal;
    vec4 Color;
};

layout (std430, binding = 9) buffer RSMBuffer
{
    RSMLight rsmLights[64*64]; 
}; 

//y = (cos(x*pi)+1)/2
float cosmix(float a, float b, float factor){
    return mix(a, b, 1.0 - (cos(factor*3.1415)*0.5+0.5));
}
float ncos(float a){
    return cosmix(0, 1, clamp(a, 0.0, 1.0));
}
uniform vec3 FrustumConeLeftBottom;
uniform vec3 FrustumConeBottomLeftToBottomRight;
uniform vec3 FrustumConeBottomLeftToTopLeft;
//mat4 imvp =inverse(ProjectionMatrix * ViewMatrix);
vec3 reconstructCameraSpace(vec2 uv){
	vec3 dir = normalize((FrustumConeLeftBottom + FrustumConeBottomLeftToBottomRight * uv.x + FrustumConeBottomLeftToTopLeft * uv.y));
	return dir * reverseLog(textureMSAA(depthTex, uv).r);
}
vec2 samplesoff[8] = vec2[8](
	vec2(0),
	vec2(0.25, 0.25),
	vec2(0.25, -0.25),
	vec2(-0.25, 0.25),
	vec2(-0.25, -0.25),
	vec2(0.55, 0.11),
	vec2(0.11, -0.55),
	vec2(0.55, -0.11)
);
vec3 reconstructCameraSpace(vec2 uv, int samplee){
	//uv += samplesoff[samplee] * (1.0/textureSize(depthTex));
	vec3 dir = normalize((FrustumConeLeftBottom + FrustumConeBottomLeftToBottomRight * uv.x + FrustumConeBottomLeftToTopLeft * uv.y));
	return dir * reverseLog(textureMSAA(depthTex, uv, samplee).r);
}
vec3 reconstructCameraSpaceLinear(vec2 uv){
	vec3 dir = normalize((FrustumConeLeftBottom + FrustumConeBottomLeftToBottomRight * uv.x + FrustumConeBottomLeftToTopLeft * uv.y));
	return dir * (textureMSAA(depthTex, uv).r);
}
mat4 imvp =inverse(VPMatrix);
vec3 reconstructCameraSpacexxx(vec2 uv){
	vec4 clip = imvp * vec4(uv * 2.0 - 1.0, 0.01, 1.0);
	vec3 dir = normalize((clip.xyz / clip.w).xyz - CameraPosition);
	return dir * reverseLog(textureMSAA(depthTex, uv).r);
}
vec3 getTangentPlane(vec3 inp){
	return normalize(cross(inp.xzy,inp));	
}