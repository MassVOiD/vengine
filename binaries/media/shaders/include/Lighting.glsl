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
    for(float x = 0; x < mPI2; x+=1.2){ 
        for(float y=0.01;y<1.0;y+= 0.3){  
			vec2 crd = vec2(sin(x + y), cos(x + y)) * (rand2s(UV/2-vec2(x,y)) * AInv * 0.4);
			vec2 fakeUV = uv + crd;
			float bval = (lookupDepthFromLight(i, fakeUV));
            if(bval < distance2) average += bval;
            counter+=1;
		}
	}
    if(counter == 0) return 0.0;
    float bbb = average/counter;
	return clamp((distance2 - bbb) *7-0.1, 0, 11);
}

float LastProbeDistance = 0.0;
float rand2d(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
float getShadowPercent(vec2 uv, vec3 pos, uint i){
	float accum = 0.0;
	
	
	
	float distance2 = distance(pos, LightsPos[i]);
    
	//float badass_depth = toLogDepth(distance2);
	mat4 lightPV = (LightsPs[i] * LightsVs[i]);
	vec4 lightClipSpace = lightPV * vec4(pos, 1.0);
    vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   
	//if(lightClipSpace.z <= 0.0) return 1;
    //if(lightScreenSpace.x < 0.0 || lightScreenSpace.x > 1.0 || lightScreenSpace.y < 0.0 || lightScreenSpace.y > 1.0) return 1;
	//float badass_depth = toLogDepthEx(distance2, LightsFarPlane[i]);
	
	
	//float AInv = 1.0 / ((distance2) + 1.0);
	//float bval = badass_depth - lookupDepthFromLight(i, uv);
	//float distanceCam = distance(positionWorldSpace.xyz, CameraPosition);
	float distance1 = 0.0;
	vec2 fakeUV = vec2(0.0);
	
	//float centerDiff = abs(badass_depth - lookupDepthFromLight(i, uv)) * 10000.0;
		
	float counter = 0;
	//distance1 = lookupDepthFromLight(i, uv);
    if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE){
        float distance3 = toLogDepth(distance2);
        fakeUV = uv;
        distance1 = lookupDepthFromLight(i, uv);
        float diff = (distance3 -  distance1);
        if(diff > 0.00003) {
          //  LastProbeDistance = bval;
           // return -abs(bval);
        }
        
        return 1.0;
    } else {
        float distance3 = toLogDepthEx(distance2, LightsFarPlane[i]);
       // float pssblur =0.9;
      // LastProbeDistance = 1.0;
        float pssblur = (getBlurAmount(uv, i, distance2, distance3)) * ShadowsBlur;
        //float pssblur = 0;
        for(float x = 0; x < mPI2; x+=0.5){ 
            for(float y=0.05;y<1.0;y+= 0.4 ){  
                fakeUV = uv + vec2(sin(x+y), cos(x+y)) * rand2d(UV+vec2(x,y)) * pssblur * 0.009;
                distance1 = lookupDepthFromLight(i, fakeUV);
                if(distance3 -  distance1 > 0.00003) accum += 1.0 ;
               // LastProbeDistance = min(LastProbeDistance, abs(badass_depth - distance1));
                counter+=1;
            }
        }
    }
	
    //LastProbeDistance = LastProbeDistance / counter;
    float rs = 1.0 - (accum / counter);
	 return rs;
}