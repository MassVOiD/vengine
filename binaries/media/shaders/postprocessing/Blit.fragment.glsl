#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

out vec4 outColor;

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;


void main()
{
	vec4 color1 = texture(texColor, UV).rgba;
	gl_FragDepth = texture(texDepth, UV).r;
    outColor = color1;
}