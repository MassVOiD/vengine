#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl
#define mPI (3.14159265)
#define mPI2 (2*3.14159265)

out vec4 outColor;

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

vec3 blurWhitening(){
	vec3 outc = vec3(0);
	for(float g = 0; g < mPI2; g+=0.6)
	{ 
		for(float g2 = 0; g2 < 14.0; g2+=2.0)
		{ 
			vec2 gauss = vec2(sin(g)*ratio, cos(g)) * (g2 * 0.001);
			vec3 color = texture(texColor, UV + gauss).rgb;
			float luminance = length(color); // luminance from 1.4 to 1.7320
			if(luminance > 1.0)
			{
				luminance = (luminance - 1.0) / 0.320;
				outc += 0.042857142 * luminance;
			}
		}
	}
	return outc;
}

void main()
{
	vec3 color1 = texture(texColor, UV).rgb;
	
	color1 += blurWhitening();
		
    outColor = vec4(color1, 1);
}