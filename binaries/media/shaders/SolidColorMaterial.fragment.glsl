#version 430 core
in vec2 UV;

uniform vec4 input_Color;
out vec4 outColor;
#include Lighting.glsl
#include LogDepth.glsl
void main()
{
	//processLighting(input_Color.xyz);
	outColor = vec4(processLighting(input_Color.xyz), input_Color.a);
	updateDepth();
	//outColor = vec4(input_Color.xyz, input_Color.a);
}