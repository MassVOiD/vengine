#version 430 core
#include Lighting.glsl

out vec4 outColor;
layout(binding = 8) uniform sampler2D tex;
in vec2 UV;

void main()
{
	vec4 color = texture(tex, UV);
	outColor = vec4(processLighting(color.xyz), color.a);
}