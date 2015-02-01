#version 430 core

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;

uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform vec3 CameraPosition;
uniform float Time;
uniform vec3 input_Color;


uniform mat4 LightsPs_0;
uniform mat4 LightsVs_0;

out vec2 LightScreenSpace;
out vec3 positionWorldSpace;
out vec3 normal;

void main(){

    vec4 v = vec4(in_position,1);
    vec4 n = vec4(in_normal,0);
	mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrix;
	
	vec4 clipspace = ((LightsPs_0 * LightsVs_0 * ModelMatrix) * v);
	LightScreenSpace = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	
	positionWorldSpace = (ModelMatrix * v).xyz;
	normal = (ProjectionMatrix * ModelMatrix * n).xyz;
	
    gl_Position = (ProjectionMatrix * ViewMatrix * ModelMatrix) * v;   
}