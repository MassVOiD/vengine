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
	int counter = 0;
	vec3 color = texture(texColor, UV).rgb;
	float luminance = length(color); // luminance from 1.4 to 1.7320
	if(luminance > 1.4)
	{
		//luminance = (luminance - 1.0) / 0.320;
		outc += color;
		//outc.a = (luminance - 1.0) / 0.320;
	}	
	for(float g2 = -1.0; g2 < 1.0; g2+=0.008)
	{ 
		vec2 gauss = vec2(g2 * 0.5, 0);
		vec4 color = texture(texColor, UV + gauss).rgba;
		float luminance = length(color); // luminance from 1.4 to 1.7320
		if(luminance > 1.4)
		{
			outc += (color.rgb * color.a * vec3(0.13, 0.13, 0.54)) * (1.0 - sin(abs(g2)));
		}			
		counter++;
		
		gauss = vec2(0, g2 * 0.5);
		color = texture(texColor, UV + gauss).rgba;
		luminance = length(color); // luminance from 1.4 to 1.7320
		if(luminance > 1.4)
		{
			outc += (color.rgb * color.a * vec3(0.13, 0.13, 0.54)) * (1.0 - sin(abs(g2)));
		}			
		counter++;		
	}	
	return outc / counter;
}


void main()
{
    outColor = vec4(blurWhitening(), 1);
}