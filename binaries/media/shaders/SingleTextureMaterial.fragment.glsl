#version 430 core


smooth in vec3 positionWorldSpace;
smooth in vec3 normal;
out vec4 outColor;

#include Lighting.glsl

layout(binding = 8) uniform sampler2D tex;
in vec2 UV;

void main()
{
	vec4 color = texture(tex, UV);
	outColor = vec4(processLighting(color.xyz), color.a);
}