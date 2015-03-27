#version 430 core

in vec2 UV;

layout(binding = 0) uniform sampler2DMS texColor;
layout(binding = 1) uniform sampler2DMS texDepth;

const int samples = 8;
float samplesInverted = 1.0 / samples;

out vec4 outColor;

ivec2 ctexSize = textureSize(texColor);
ivec2 dtexSize = textureSize(texDepth);

vec4 fetchColor()
{
	vec4 c = vec4(0.0);
	ivec2 tx = ivec2(ctexSize * UV); 
	for (int i = 0; i < samples; i++) c += texelFetch(texColor, tx, i);  
	return c * samplesInverted;
}

float fetchDepth()
{
	float d = 0.0;
	ivec2 tx = ivec2(dtexSize * UV); 
	for (int i = 0; i < samples; i++) d += texelFetch(texDepth, tx, i).r;  
	return d * samplesInverted;
}

void main()
{
	outColor = fetchColor();
	gl_FragDepth = fetchDepth();
}