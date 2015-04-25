#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#define mPI (3.14159265)
#define mPI2 (2*3.14159265)
#define GOLDEN_RATIO (1.6180339)

out vec4 outColor;

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

vec3 blurWhitening(){
	vec3 outc = vec3(0);
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 0.03; g2 < 1.0; g2+=BloomSamples)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * (0.02 * BloomSize));
			vec3 color = texture(texColor, UV + gauss).rgb;
			float luminance = length(color); // luminance from 1.4 to 1.7320
			if(luminance > 0.5)
			{
				//luminance = (luminance - 1.0) / 0.320;
				outc += 0.0012857142 * color / g2;
			}
		}
	}
	return outc;
}


void main()
{
    outColor = vec4(blurWhitening(), 1);
}