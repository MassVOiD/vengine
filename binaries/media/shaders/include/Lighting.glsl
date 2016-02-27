#include_once LightingSamplers.glsl

float lookupDepthFromLight(uint i, vec2 uvi, float comparison){
    float distance1 = 0.0;
    vec3 uv = vec3(uvi, float(i));
    distance1 = texture(shadowMapsArray, uv).r;
    return step(comparison, distance1);
}
#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)

float rand2s(vec2 co){
        return fract(sin(dot(co.xy,vec2(12.9898,78.233))) * 43758.5453);
}
float ArandsPointer = 0;
float LightingGetRand(){
    float r = rand2s(vec2(ArandsPointer, ArandsPointer*2.42354));
    ArandsPointer+=0.5432;
    return r;
}/*
float getBlurAmount(vec2 uv, uint i, float ainvd, float distance2){
    float AInv = 1.0 / ((ainvd) + 1.0);
    float average = 0.0;
    float counter = 0;
    float minval = 999;
    float maxval = 0;
    for(float x = 0; x < mPI2; x+=0.5){ 
        for(float y=0.01;y<1.0;y+= 0.3){  
            vec2 crd = vec2(sin(x + y), cos(x + y)) * rand2s(uv - vec2(x,y)) * AInv * 0.4;
            vec2 fakeUV = uv + crd;
            float bval = (lookupDepthFromLight(i, fakeUV, distance2));
            average += bval;
            counter+=1;
        }
    }
    if(counter == 0) return 0.0;
    float bbb = average/counter;
    return clamp(bbb, 0, 12);
}
*/
float LastProbeDistance = 0.0;
float rand2d(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
float getShadowPercent(vec2 uv, vec3 pos, uint i){
    float accum = 0.0;
    
    ArandsPointer = float(ArandsPointer + rand2s(uv+Time) * 113.86786 );
    float distance2 = distance(pos, LightsPos[i]) + 0.1;
    
    mat4 lightPV = (LightsPs[i] * LightsVs[i]);
	
    vec4 lightClipSpace = (lightPV) * vec4(pos, 1.0);
    vec3 lightScreenSpace = lightClipSpace.xyz / lightClipSpace.w;
    float distance3 = (lightScreenSpace.z);

    float distance1 = 0.0;
    vec2 fakeUV = vec2(0.0);
    
    float counter = 0;
    //return lookupDepthFromLight(i, uv) - distance3 > 0.000015 ? 0.0 : 1.0;
    float pssblur = 0;//max(0, (getBlurAmount(uv, i, distance2, distance3)) - 0.1) * 1.1;
    //return lookupDepthFromLight(i, uv, distance3 - 0.000004);
    for(float x = 0; x < 1.0; x+=0.3){ 
		fakeUV = uv + (vec2(rand2s(uv + vec2(x,0)), rand2s(uv + vec2(0,x))) * 2.0 - 1.0) * distance2 * 0.00005 * LightsBlurFactors[i];
		accum += lookupDepthFromLight(i, fakeUV, distance3 + 0.00003);

		//if(distance3 -  distance1 > 0.000015) accum += 1.0 ;
		counter+=1;
	
    }

    
    //LastProbeDistance = LastProbeDistance / counter;
    float rs = 1.0 - (accum / counter);
    return rs;//return smoothstep(0.0, 0.9, rs);
}