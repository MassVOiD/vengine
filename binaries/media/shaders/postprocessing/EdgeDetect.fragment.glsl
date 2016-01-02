#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

#include noise4D.glsl

out vec4 outColor;

float GetFrequency(sampler2DMS sampler){
	vec3 pix = vec3(1.0 / textureSize(sampler), 0);// trick
	vec2 pix2 = vec2(pix.x, -pix.y);
	float f = 0;
	vec4 center = textureMSAA(sampler, UV, 0);
	
	f += distance(center, textureMSAA(sampler, UV + pix.xz, 0));
	f += distance(center, textureMSAA(sampler, UV - pix.xz, 0));
	
	f += distance(center, textureMSAA(sampler, UV + pix.zy, 0));
	f += distance(center, textureMSAA(sampler, UV - pix.zy, 0));
	
	f += distance(center, textureMSAA(sampler, UV + pix.xy, 0));
	f += distance(center, textureMSAA(sampler, UV - pix.xy, 0));
	
	f += distance(center, textureMSAA(sampler, UV + pix.xy, 0));
	f += distance(center, textureMSAA(sampler, UV - pix.xy, 0));
	
	f += distance(center, textureMSAA(sampler, UV + pix2.xy, 0));
	f += distance(center, textureMSAA(sampler, UV - pix2.xy, 0));
	
	f += distance(center, textureMSAA(sampler, UV + pix2.xy, 0));
	f += distance(center, textureMSAA(sampler, UV - pix2.xy, 0));
	
	return f / 12.0;
}

float GetFrequencyRed(sampler2DMS sampler){
	vec3 pix = vec3(1.0 / textureSize(sampler), 0);// trick
	vec2 pix2 = vec2(pix.x, -pix.y);
	float f = 0;
	float center = textureMSAA(sampler, UV, 0).r;
	
	f += abs(center - textureMSAA(sampler, UV + pix.xz, 0).r);
	f += abs(center - textureMSAA(sampler, UV - pix.xz, 0).r);
	                
	f += abs(center - textureMSAA(sampler, UV + pix.zy, 0).r);
	f += abs(center - textureMSAA(sampler, UV - pix.zy, 0).r);
	                
	f += abs(center - textureMSAA(sampler, UV + pix.xy, 0).r);
	f += abs(center - textureMSAA(sampler, UV - pix.xy, 0).r);
	                
	f += abs(center - textureMSAA(sampler, UV + pix.xy, 0).r);
	f += abs(center - textureMSAA(sampler, UV - pix.xy, 0).r);
	                
	f += abs(center - textureMSAA(sampler, UV + pix2.xy, 0).r);
	f += abs(center - textureMSAA(sampler, UV - pix2.xy, 0).r);
	                
	f += abs(center - textureMSAA(sampler, UV + pix2.xy, 0).r);
	f += abs(center - textureMSAA(sampler, UV - pix2.xy, 0).r);
	
	return f*10.0;
}

void main()
{
	float h = GetFrequency(normalsTex)*0.1 + GetFrequencyRed(depthTex);
    outColor = vec4(h*0.1);
}