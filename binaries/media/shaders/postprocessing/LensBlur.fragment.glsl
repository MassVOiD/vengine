#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

uniform float LensBlurAmount;
uniform float CameraCurrentDepth;

out vec4 outColor;
	
float centerDepth;

#define mPI (3.14159265)
#define mPI2 (2*3.14159265)
vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
	vec3 finalColor = vec3(0.0,0.0,0.0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	float centerDepthDistance = abs((centerDepth) - (depthfocus));
	//float centerDepth = texture(texDepth, UV).r;
    for(float x = 0; x < mPI2; x+=0.5){ 
        for(float y=0;y<samples;y+= 1.0){  
			vec2 crd = vec2(sin(x) * ratio, cos(x)) * (rand(UV + vec2(x, y)) * 0.25);
			//if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.01 * amount;  
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
            if(distance(coord, UV.xy) < max_radius){  
                float depth = texture(texDepth, coord).r;
				if(abs(centerDepth - depth) < 0.0005 || centerDepthDistance > 0.4 || depth > 0.99 || centerDepth > 0.99){
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

void main()
{

	vec3 color1 = texture(texColor, UV).rgb;
	float depth = texture(texDepth, UV).r;
	centerDepth = depth;
	if(LensBlurAmount > 0.001){
		float focus = CameraCurrentDepth;
		float adepth = reverseLog(depth);
		float fDepth = reverseLog(CameraCurrentDepth);
		//float avdepth = clamp(pow(abs(depth - focus), 0.9) * 53.0 * LensBlurAmount, 0.0, 4.5 * LensBlurAmount);		
		float f = 16.0 / LensBlurAmount; //focal length in mm
		float d = fDepth*1000.0; //focal plane in mm
		float o = adepth*1000.0; //depth in mm
		
		float fstop = 4.0;
		float CoC = 0.03;
		float a = (o*f)/(o-f); 
		float b = (d*f)/(d-f); 
		float c = (d-f)/(d*fstop*CoC); 
		
		float blur = abs(a-b)*c;
		blur = clamp(blur,0.0,1.0) * 50.0;
		color1 = lensblur(blur, focus, 0.03, 5.0);
	
	}
	gl_FragDepth = depth;
    outColor = vec4(color1, 1.0);
	
}