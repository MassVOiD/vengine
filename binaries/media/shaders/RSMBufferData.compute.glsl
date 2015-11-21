#version 430 core
#define NO_FS
vec3 CameraPosition;

#include UsefulIncludes.glsl
#define MATH_E 2.7182818284
float reverseLogEx(float dd, float far){
	return pow(2, dd * log2(far+1.0)) - 1;
}
layout(binding = 22) uniform usampler2D LightData;
layout(binding = 23) uniform sampler2D LightDepth;

layout( local_size_x = 1, local_size_y = 1, local_size_z = 1 ) in;
uniform mat4 MatP;
uniform mat4 MatV;
uniform mat4 MatI;
uniform float Far;
uniform vec3 LightPos;
void main(){

    float ax = float(gl_GlobalInvocationID.x) / 64.0;
    float ay = float(gl_GlobalInvocationID.y) / 64.0;
    vec2 UV = vec2(ax, ay);
    ivec2 iUV = ivec2(vec2(ax, ay) * vec2(textureSize(LightData, 0)));
    uint gIndex = 
        //gl_GlobalInvocationID.z * gl_GlobalInvocationID.x * gl_GlobalInvocationID.y +
        gl_GlobalInvocationID.y * gl_GlobalInvocationID.x + 
        gl_GlobalInvocationID.x;
    float ldep = texelFetch(LightDepth, iUV, 0).r;            
    uvec2 lcolord =  texelFetch(LightData, iUV, 0).rg;
    vec4 upackA = unpackUnorm4x8(lcolord.r);
    vec4 upackB = unpackSnorm4x8(lcolord.g);
    vec3 lcolor = upackA.rgb;
    float lrough = upackA.a;
    vec3 lnormal = upackB.rgb;
    float lmetal = upackB.a;
    
    UV = (UV * 2 - 1);
    vec4 reconstructDir = inverse(MatP * MatV * MatI) * vec4(UV, 1.0, 1.0);
    reconstructDir.xyz /= reconstructDir.w;
    
    float revlog = reverseLogEx(ldep, Far);
    // not optimizable
    vec3 newpos = normalize(reconstructDir.xyz - LightPos) * revlog + LightPos;
    RSMLight light = RSMLight(vec4(newpos, lrough), vec4(lnormal, lmetal), vec4(lcolor, 1));
    rsmLights[gIndex] = light;
}