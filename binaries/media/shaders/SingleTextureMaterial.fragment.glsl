#version 430 core
in vec2 UV;
#include Lighting.glsl

out vec4 outColor;
layout(binding = 8) uniform sampler2D tex;

void main()
{
	vec4 color = texture(tex, UV);
	outColor = vec4(processLighting(color.xyz), color.a);
}