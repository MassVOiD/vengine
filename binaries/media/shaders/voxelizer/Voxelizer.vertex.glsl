#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl


uniform vec3 CenterToZero;
uniform float NormalizationDivisor;
uniform int Pass;

out vec4 Position;
out vec4 innormal;

void main(){
    vec3 v = in_position*0.999;
	//if(Pass == 1) v = v.zxy;
	//if(Pass == 2) v = v.yzx;
   // v.xyz *= NormalizationDivisor;
   // v.xyz -= CenterToZero * NormalizationDivisor;
	Position = vec4(v, 1);
	innormal = vec4(in_normal, 1);

	int vid = gl_VertexID;
	vec3 vx = vec3(0, 0, 0);
	if(vid % 3 == 1)vx = vec3(1, 0, 0);
	if(vid % 3 == 2)vx = vec3(0, 1, 0);
    gl_Position = vec4(vx, 1);
}