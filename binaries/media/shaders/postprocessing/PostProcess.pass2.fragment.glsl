#version 430 core

in vec2 UV;
#include Lighting.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

uniform float LensBlurAmount;
uniform float CameraCurrentDepth;
uniform float Brightness;

out vec4 outColor;

const int MAX_LINES_2D = 256;

uniform int Lines2dCount;
uniform vec3 Lines2dStarts[MAX_LINES_2D];
uniform vec3 Lines2dEnds[MAX_LINES_2D];
uniform vec3 Lines2dColors[MAX_LINES_2D];

uniform vec2 resolution;

float ratio = resolution.y/resolution.x;
	
#define MATH_E 2.7182818284
float reverseLog(float depth){
	return pow(MATH_E, depth - 1.0) / LogEnchacer;
}


float centerDepth;

#define mPI (3.14159265)
#define mPI2 (2*3.14159265)
vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
	vec3 finalColor = vec3(0.0,0.0,0.0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	float centerDepthDistance = abs(reverseLog(centerDepth) - reverseLog(depthfocus));
	//float centerDepth = texture(texDepth, UV).r;
    for(float x = 0; x < mPI2; x+=0.1){ 
        for(float y=0;y<samples;y+= 1.0){  
			vec2 crd = vec2(sin(x) * ratio, cos(x)) * (y * 0.25);
			if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.01 * amount;  
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
            if(distance(coord, UV.xy) < max_radius){  
                float depth = texture(texDepth, coord).r;
				if(centerDepth - depth < 0.05 || centerDepthDistance > 0.4 || depth > 0.99 || centerDepth > 0.99){
					vec3 texel = texture(texColor, coord).rgb;
					float w = length(texel)+0.1;
					weight+=w;
					finalColor += texel*w;
				}
            }
        }
    }
	return finalColor/weight;
}

/*
vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
	vec3 finalColor = vec3(0.0,0.0,0.0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	float centerDepthDistance = abs(reverseLog(centerDepth) - reverseLog(depthfocus));
	//float centerDepth = texture(texDepth, UV).r;
    for(float x=samples*-1.0;x<samples;x+= 1.0) {  
        for(float y=samples*-1.0;y<samples;y+= 1.0){  
			float xc = (x / samples) * ratio;
			float yc = y / samples;
			if(length(vec2(xc, yc)) > 1.0) continue;
            vec2 coord = UV+(vec2(xc, yc) * 0.01 * amount);  
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
            if(distance(coord, UV.xy) < max_radius){  
                float depth = texture(texDepth, coord).r;
				if(centerDepth - depth < 0.05 || centerDepthDistance > 0.4 || depth > 0.99 || centerDepth > 0.99){
					vec3 texel = texture(texColor, coord).rgb;
					float w = length(texel)+0.1;
					weight+=w;
					finalColor += texel*w;
				}
            }
        }
    }
	return finalColor/weight;
}*/
void main()
{

	vec3 color1 = texture(texColor, UV).rgb;
	float depth = texture(texDepth, UV).r;
	centerDepth = depth;
	if(LensBlurAmount > 0.001){
		float focus = CameraCurrentDepth;
		float avdepth = clamp(pow(abs(depth - focus), 0.9) * 53.0 * LensBlurAmount, 0.0, 4.5 * LensBlurAmount);
		color1 = lensblur(avdepth, focus, 0.03, 7.0);
	
	}
	
	vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2) / Brightness;
	color1 = vec3(pow(color1.r, gamma.r),
                  pow(color1.g, gamma.g),
                  pow(color1.b, gamma.b));
				  
    outColor = vec4(color1, 1.0);
	
}