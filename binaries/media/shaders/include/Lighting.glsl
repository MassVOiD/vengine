#include LightingSamplers.glsl
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
	else if(i==6)distance1 = texture(lightDepth6, uv).r;
	else if(i==7)distance1 = texture(lightDepth7, uv).r;
	else if(i==8)distance1 = texture(lightDepth8, uv).r;
	else if(i==9)distance1 = texture(lightDepth9, uv).r;
	else if(i==10)distance1 = texture(lightDepth10, uv).r;
	else if(i==11)distance1 = texture(lightDepth11, uv).r;
	else if(i==12)distance1 = texture(lightDepth12, uv).r;
	else if(i==13)distance1 = texture(lightDepth13, uv).r;
	else if(i==14)distance1 = texture(lightDepth14, uv).r;
	else if(i==15)distance1 = texture(lightDepth15, uv).r;
	else if(i==16)distance1 = texture(lightDepth16, uv).r;
	else if(i==17)distance1 = texture(lightDepth17, uv).r;
	else if(i==18)distance1 = texture(lightDepth18, uv).r;
	else if(i==19)distance1 = texture(lightDepth19, uv).r;
	else if(i==20)distance1 = texture(lightDepth20, uv).r;
	else if(i==21)distance1 = texture(lightDepth21, uv).r;
	else if(i==22)distance1 = texture(lightDepth22, uv).r;
	else if(i==23)distance1 = texture(lightDepth23, uv).r;
	else if(i==24)distance1 = texture(lightDepth24, uv).r;
	else if(i==25)distance1 = texture(lightDepth25, uv).r;
	else if(i==26)distance1 = texture(lightDepth26, uv).r;
	//else if(i==27)distance1 = texture(lightDepth27, uv).r;
	return distance1;
}
#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)

float getBlurAmount(vec2 uv, uint i, float distance2){
	float distanceCenter = distance2;
	float AInv = 1.0 / ((distanceCenter) + 1.0);
	float average = 0.0;
	float maxv = 0.0;
	float minv = 1111.0;
	vec2 fakeUV;
	int counter = 0;
    for(float x = 0; x < mPI2 * 1.5; x+=GOLDEN_RATIO){ 
        for(float y=0.1;y<1.0;y+= 0.2){  
			vec2 crd = vec2(sin(x), cos(x)) * (y * 0.05);
			fakeUV = uv + crd;
			float bval = (lookupDepthFromLight(i, fakeUV));
			if(bval == 0  || bval == 1) continue;
			minv = min(minv, bval);
			maxv = max(maxv, bval);
		}
	}
	return clamp(clamp(maxv - minv, 0.0, 0.02) - 0.002, 0.0, 1.01 * 33.0);
}


float getShadowPercent(vec2 uv, vec3 pos, uint i){
	float accum = 1.0;
	
	
	
	float distance2 = distance(pos, LightsPos[i]);
	//float badass_depth = toLogDepth(distance2);
	mat4 lightPV = (LightsPs[i] * LightsVs[i]);
	vec4 lightClipSpace = lightPV * vec4(pos, 1.0);
	if(lightClipSpace.z <= 0.0) return 0;
	float badass_depth = (lightClipSpace.z / lightClipSpace.w) * 0.5 + 0.5;	
	
	
	float AInv = 1.0 / ((distance2) + 1.0);
	float bval = distance2 - reverseLog(lookupDepthFromLight(i, uv));
	//float distanceCam = distance(positionWorldSpace.xyz, CameraPosition);
	float distance1 = 0.0;
	vec2 fakeUV = vec2(0.0);
	
	//float centerDiff = abs(badass_depth - lookupDepthFromLight(i, uv)) * 10000.0;
		
	int counter = 0;
	//distance1 = lookupDepthFromLight(i, uv);
	//float pssblur = (getBlurAmount(uv, i, distance2)) * ShadowsBlur;
	float pssblur = 0.09;
    for(float x = 0; x < mPI2; x+=0.7){ 
        for(float y=0.0;y<4.0;y+= ShadowsSamples ){  
			vec2 crd = vec2(sin(x), cos(x)) * y * pssblur * 0.04;
			fakeUV = uv + crd;
			distance1 = lookupDepthFromLight(i, fakeUV);
			float diff = abs(distance1 -  badass_depth);
			if(diff > 0.0001) accum += 1.0;
			counter++;
		}
	}
	return 1.0 - (accum / counter);
}