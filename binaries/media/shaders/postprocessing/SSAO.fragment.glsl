#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl

out vec4 outColor;

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;

	
float getSSAOAmount(){
	vec3 normalCenter = texture(normalsTex, UV).rgb;	
	float ssao = 0.0;
	int counter = 0;
    for(float x = 0; x < mPI2 * 2; x+=GOLDEN_RATIO){ 
        for(float y=0;y<6;y+= 1.0){  
			vec2 crd = vec2(sin(x), cos(x)) * (y * 0.002);
			vec3 normalThere = texture(normalsTex, UV + crd).rgb;
			float dotProduct = dot(normalCenter, normalThere);
		}
	}
	return ssao;
}

void main()
{
	vec3 color1 = texture(texColor, UV).rgb;
	float depth = texture(texDepth, UV).r;
	gl_FragDepth = depth;
    outColor = vec4(color1 - getSSAOAmount(), 1);
}