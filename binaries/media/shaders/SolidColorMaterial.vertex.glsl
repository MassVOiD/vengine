#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

uniform vec3 input_Color;

out vec2 LightScreenSpace[MAX_LIGHTS];
out vec3 positionWorldSpace;
out vec3 positionModelSpace;
out vec3 normal;

void main(){

    vec4 v = vec4(in_position,1);
    vec4 n = vec4(in_normal,0);
	mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrix;
	
	for(uint i = 0; i < LightsCount; i++){
		vec4 clipspace = ((LightsPs[i] * LightsVs[i] * ModelMatrix) * v);
		LightScreenSpace[i] = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	}
	
	positionWorldSpace = (ModelMatrix * v).xyz;
	positionModelSpace = in_position;
	normal = (ModelMatrix * n).xyz;
	
    gl_Position = (ProjectionMatrix * ViewMatrix * ModelMatrix) * v;   
}