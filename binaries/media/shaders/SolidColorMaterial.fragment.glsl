#version 430 core

smooth in vec3 positionWorldSpace;
smooth in vec3 normal;
uniform vec4 input_Color;
out vec4 outColor;

#include Lighting.glsl

void main()
{
	vec3 color = input_Color.xyz;
	outColor = vec4(processLighting(color), input_Color.a);
}