#version 430 core
in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl
smooth in vec3 barycentric;
out vec4 outColor;
layout(binding = 0) uniform sampler2D tex;

void main()
{
	vec4 color = texture(tex, UV);
	outColor = vec4(processLighting(color.xyz), color.a);
	updateDepth();
	//float dist = barycentric.x < 0.02 || barycentric.y < 0.02 || barycentric.z < 0.02 ? 1.0 : 0.0;
	//outColor = vec4(barycentric, color.a);
}