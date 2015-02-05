#version 430 core
#include Lighting.glsl

uniform vec4 input_Color;
out vec4 outColor;
void main()
{
	outColor = vec4(processLighting(input_Color.xyz), input_Color.a);
}