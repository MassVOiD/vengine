#include_once LightingSamplers.glsl
/*
Insane lighting
Part of: https://github.com/achlubek/vengine
@author Adrian Chlubek
*/

float lookupDepthFromLight(uint i, vec2 uv){
	float distance1 = 0.0;
	if(i==0)distance1 = texture(lightDepth0, uv).r;
	else if(i==1)distance1 = texture(lightDepth1, uv).r;
	else if(i==2)distance1 = texture(lightDepth2, uv).r;
	else if(i==3)distance1 = texture(lightDepth3, uv).r;
	else if(i==4)distance1 = texture(lightDepth4, uv).r;
	else if(i==5)distance1 = texture(lightDepth5, uv).r;
	return distance1;
}
uvec2 lookupColorFromLight(uint i, vec2 uv){
	uvec2 distance1 = uvec2(0.0);
	if(i==0)distance1 = texture(lightDepth0Color, uv).rg;
	else if(i==1)distance1 = texture(lightDepth1Color, uv).rg;
	else if(i==2)distance1 = texture(lightDepth2Color, uv).rg;
	else if(i==3)distance1 = texture(lightDepth3Color, uv).rg;
	else if(i==4)distance1 = texture(lightDepth4Color, uv).rg;
	else if(i==5)distance1 = texture(lightDepth5Color, uv).rg;
	return distance1;
}
#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)

float rand2s(vec2 co){
        return fract(sin(dot(co.xy,vec2(12.9898,78.233))) * 43758.5453);
}
float getBlurAmount(vec2 uv, uint i, float ainvd, float distance2){
	float distanceCenter = distance2;
	float AInv = 1.0 / ((ainvd) + 1.0);
	float average = 0.0;
	float counter = 0;
    float abcd = lookupDepthFromLight(i, uv);
    float minval = 999;
    float maxval = 0;
    for(float x = 0; x < mPI2; x+=0.2){ 
        for(float y=0.01;y<1.0;y+= 0.3){  
			vec2 crd = vec2(sin(x + y), cos(x + y)) * (rand2s(uv/2-vec2(x,y)) * AInv * 0.4);
			vec2 fakeUV = uv + crd;
			float bval = (lookupDepthFromLight(i, fakeUV));
            average += bval * sign(distance2 - bval);
            counter+=1;
		}
	}
    if(counter == 0) return 0.0;
    float bbb = average/counter;
	return clamp((distance2 - bbb) *7-0.05, 0, 12);
}

float LastProbeDistance = 0.0;
float rand2d(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
float getShadowPercent(vec2 uv, vec3 pos, uint i){
	float accum = 0.0;
    
	float distance2 = distance(pos, LightsPos[i]);
    
	mat4 lightPV = (LightsPs[i] * LightsVs[i]);
	vec4 lightClipSpace = lightPV * vec4(pos, 1.0);
    vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

	float distance1 = 0.0;
	vec2 fakeUV = vec2(0.0);
	
	float counter = 0;
  
    float distance3 = toLogDepthEx(distance2, LightsFarPlane[i]);
    float pssblur = (getBlurAmount(uv, i, distance2, distance3)) * ShadowsBlur;
    for(float x = 0; x < mPI2; x+=0.5){ 
        for(float y=0.05;y<1.0;y+= 0.2 ){  
            fakeUV = uv + vec2(sin(x+y), cos(x+y)) * rand2d(uv+vec2(x,y)) * pssblur * 0.009;
            distance1 = lookupDepthFromLight(i, fakeUV);
            if(distance3 -  distance1 > 0.00003) accum += 1.0 ;
            counter+=1;
        }
    }

	
    //LastProbeDistance = LastProbeDistance / counter;
    float rs = 1.0 - (accum / counter);
    return rs;//return smoothstep(0.0, 0.9, rs);
}