#include_once LightingSamplers.glsl

float lookupDepthFromLight(uint i, vec2 uvi, float comparison){
    float distance1 = 0.0;
    vec3 uv = vec3(uvi, float(i));
	vec4 compres = textureGather(shadowMapsArray, uv, comparison);
    //return (compres.r+compres.g+compres.b+compres.a) * 0.25;
    //return step(comparison, distance1);
	return texture(shadowMapsArray, vec4(uv, comparison));
	//vec2 f = fract( uvi.xy * vec2(textureSize(shadowMapsArray, 0)) );
	//vec2 mx = mix( compres.xz, compres.yw, f.x );
	//return mix( mx.x, mx.y, f.y );
}

#define KERNEL 6
#define PCFEDGE 1
float PCF(uint i, vec2 uvi, float comparison){

    float shadow = 0.0;
    float pixSize = 1.0 / textureSize(shadowMapsArray,0).x;
    float bound = KERNEL * 0.5 - 0.5;
    bound *= PCFEDGE;
    for (float y = -bound; y <= bound; y += PCFEDGE){
        for (float x = -bound; x <= bound; x += PCFEDGE){
			vec3 uv = vec3(uvi+ vec2(x,y)* pixSize, float(i));
            shadow += texture(shadowMapsArray, vec4(uv, comparison));
        }
    }
	return shadow / (KERNEL * KERNEL);
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
vec2 shadowmapSamples[] = vec2[](
vec2(0.5, 0.5),
vec2(0.25, 0.5),
vec2(0.75, 0.5),
vec2(0.5, 0.25),
vec2(0.5, 0.75),
vec2(0.25, 0.25),
vec2(0.75, 0.75),
vec2(0.75, 0.25),
vec2(0.75, 0.25),
vec2(0.33, 0.5),
vec2(0.66, 0.5),
vec2(0.5, 0.33),
vec2(0.5, 0.66),
vec2(0.33, 0.33),
vec2(0.66, 0.66),
vec2(0.66, 0.33),
vec2(0.66, 0.33)
);
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
    return 1.0 - PCF(i, uv, distance3 + 0.0001);
	float iter = 0;
	for(int ix=0;ix<1;ix++){
		float rot = rand2d(uv + iter) * 3.1415 * 2;
		iter += 1.0;
		mat2 RM = mat2(cos(rot), -sin(rot), sin(rot), cos(rot));
		
		for(int id = 0; id < shadowmapSamples.length(); id++){ 
			fakeUV = uv + (RM * shadowmapSamples[id]) * distance2 * 0.00005 * LightsBlurFactors[i];
			accum += lookupDepthFromLight(i, clamp(fakeUV, 0.001, 0.999), distance3 + 0.00006);
		}	
	}
    
    //LastProbeDistance = LastProbeDistance / counter;
    float rs = 1.0 - (accum / (shadowmapSamples.length() * iter));
    return rs;//return smoothstep(0.0, 0.9, rs);
}