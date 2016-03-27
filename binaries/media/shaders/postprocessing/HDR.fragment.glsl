#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

uniform int Numbers[12];
uniform int NumbersCount;


//uniform float Brightness;

out vec4 outColor;




uniform float LensBlurAmount;
uniform float CameraCurrentDepth;

uniform int DisablePostEffects;
float centerDepth;
#define mPI (3.14159265)

float ngonsides = 5;
float sideLength = sqrt(1+1-2*cos(mPI2 / ngonsides));

float PIOverSides = mPI2/ngonsides;
float PIOverSidesOver2 = PIOverSides/2;
float triangleHeight = 0.85;
uniform int ShowSelected;
uniform int UnbiasedIntegrateRenderMode;


float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}


vec2 dofsamplesSpeed[] = vec2[](
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

uniform float InputFocalLength;
float getAmountForDistance(float focus, float dist){

	float f = InputFocalLength;
	float d = focus*1000.0; //focal plane in mm
	float o = dist*1000.0; //depth in mm
	
	float fstop = 64.0 / LensBlurAmount;
	float CoC = 1.0;
	float a = (o*f)/(o-f); 
	float b = (d*f)/(d-f); 
	float c = (d-f)/(d*fstop*CoC); 
	
	float blur = abs(a-b)*c;
	return blur;
}

vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
    vec3 finalColor = vec3(0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    if(amount < 0.05) amount = 0.05;
    amount -= 0.05;
	amount = min(amount, 1.1);
    //amount = max(0, amount - 0.1);
    //return textureLod(currentTex, UV, amount*2).rgb;
    float radius = max_radius;  
    float centerDepthDistance = abs((centerDepth) - (depthfocus));
    //float centerDepth = texture(texDepth, UV).r;
    float focus = length(reconstructCameraSpace(vec2(0.5)));
    float cc = textureMSAA(normalsDistancetex, UV, 0).a;
	
	float iter = 1.0;
	for(int ix=0;ix<4;ix++){
		float rot = rand2d(UV + iter) * 3.1415 * 2;
		iter += 1.0;
		mat2 RM = mat2(cos(rot), -sin(rot), sin(rot), cos(rot));
		for(int i=0;i<dofsamplesSpeed.length();i++){ 
				
			
			vec2 crd = RM * (dofsamplesSpeed[i] * 2 - 1) * vec2(ratio, 1.0);
			//float alpha = texture(alphaMaskTex, crd*1.41421).r;
			//if(length(crd) > 1.0) continue;
			vec2 coord = UV+crd * 0.02 * amount;  
			coord = clamp(coord, 0.0, 1.0);
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
			float depth = textureMSAA(normalsDistancetex, coord, 0).a;
			
			float amountForIt = min(2, getAmountForDistance(focus, depth));
			
			vec3 texel = texture(deferredTex, coord).rgb;
			//texel += texture(ssRefTex, coord).rgb;
			if(depth < 0.001) texel = vec3(1);
			float w = length(texel) + 0.1;
			
			// if 
			//w *= 
			float blurdif = abs(amount - amountForIt);
			
			float fact2 = 1.0 - blurdif * 0.1;
			
			w *= clamp(fact2, 0.001, 1.0);
			
			finalColor += texel * w;
			weight += w;
		}
    }
    return weight == 0.0 ? vec3(0.0) : finalColor/weight;
}


vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}

#include noise3D.glsl

float avgdepth(vec2 buv){
    float outc = float(0);
    float counter = 0;
    float fDepth = length(reconstructCameraSpace(vec2(0.5, 0.5)).rgb);
    //
            //vec2 gauss = buv + vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.05);
            //gauss = clamp(gauss, 0.0, 0.90);
            float adepth = textureMSAA(normalsDistancetex, buv, 0).a;
            //if(adepth < fDepth) adepth = fDepth + (fDepth - adepth);
            //float avdepth = clamp(pow(abs(depth - focus), 0.9) * 53.0 * LensBlurAmount, 0.0, 4.5 * LensBlurAmount);        
            float f = InputFocalLength;
            //float f = 715.0; //focal length in mm
            float d = fDepth*1000.0; //focal plane in mm
            float o = adepth*1000.0; //depth in mm
            
            float fstop = 64.0 / LensBlurAmount;
            float CoC = 1.0;
            float a = (o*f)/(o-f); 
            float b = (d*f)/(d-f); 
            float c = (d-f)/(d*fstop*CoC); 
            
            float blur = abs(a-b)*c;
            outc += blur;
            counter++;
     //   }
   // }
    return min(abs(outc / counter), 2.0);
}



vec3 ExecutePostProcessing(vec3 color, vec2 uv){
	float vignette = distance(vec2(0), vec2(0.5)) - distance(uv, vec2(0.5));
	vignette = 0.1 + 0.9*smoothstep(0.0, 0.3, vignette);
    return vec3pow(color.rgb, 1.0) * vignette * Brightness;
}

vec3 lookupSSR(vec2 fuv, float radius){
    vec3 outc = vec3(0);
    int counter = 0;
    float depthCenter = textureMSAA(normalsDistancetex, fuv, 0).a;
	vec3 normalcenter = textureMSAAFull(normalsDistancetex, fuv).rgb;
    for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO*0.5)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.1)
        {
            vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.0001 * radius);
            vec3 color = texture(ssRefTex, fuv + gauss).rgb;
            float depthThere = textureMSAA(normalsDistancetex, fuv + gauss, 0).a;
			vec3 normalthere = textureMSAAFull(normalsDistancetex, fuv).rgb;
            if(abs(depthThere - depthCenter) < 0.1 && dot(normalthere, normalcenter) > 0.9){
                outc += color;
                counter++;
            }
        }
    }
    return counter == 0 ? texture(ssRefTex, fuv).rgb : outc / counter;
}

vec3 lookupSSRLowPass(vec2 fuv, float radius){
	return texture(ssRefTex, fuv).rgb;
    vec3 outc = vec3(0);
    int counter = 0;
    float depthCenter = textureMSAA(normalsDistancetex, fuv, 0).a;
	vec3 normalcenter = textureMSAAFull(normalsDistancetex, fuv).rgb;
	float lastLum = 0;
    for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO*0.5)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.1)
        {
            vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.007 * radius);
            vec3 color = texture(ssRefTex, fuv + gauss).rgb;
            float depthThere = textureMSAA(normalsDistancetex, fuv + gauss, 0).a;
			vec3 normalthere = textureMSAAFull(normalsDistancetex, fuv).rgb;
            if(abs(depthThere - depthCenter) < 0.05){
				float difference = abs(lastLum - length(color))*12;
				lastLum = length(color);
                outc += (1.0 / (difference + 1)) * color;
                counter++;
            }
        }
    }
    return counter == 0 ? texture(ssRefTex, fuv).rgb : outc / counter;
}

// THATS FROM PANDA 3d! Thanks tobspr
const float SRGB_ALPHA = 0.055;
float linear_to_srgb(float channel) {
    if(channel <= 0.0031308)
        return 12.92 * channel;
    else
        return (1.0 + SRGB_ALPHA) * pow(channel, 1.0/2.4) - SRGB_ALPHA;
}
vec3 rgb_to_srgb(vec3 rgb) {
    return vec3(
        linear_to_srgb(rgb.r),
        linear_to_srgb(rgb.g),
        linear_to_srgb(rgb.b)
    );
}

uniform mat4 CurrentViewMatrix;
uniform mat4 LastViewMatrix;
uniform mat4 ProjectionMatrix;

vec2 projectMotion(vec3 pos){
    vec4 tmp = (ProjectionMatrix * vec4(pos, 1.0));
    return (tmp.xy / tmp.w) * 0.5 + 0.5;
}

vec3 makeMotion(vec2 uv){
	vec4 normalsDistanceData = textureMSAA(normalsDistancetex, uv, 0);
	normalsDistanceData.a += (1.0 - step(0.001, normalsDistanceData.a)) * 10000.0;
	vec3 camSpacePos = reconstructCameraSpaceDistance(uv, normalsDistanceData.a);
	vec3 worldPos = FromCameraSpace(camSpacePos);
	
	vec3 pos1 = (CurrentViewMatrix * vec4(worldPos, 1.0)).xyz;
	vec3 pos2 = (LastViewMatrix * vec4(worldPos, 1.0)).xyz;
	vec2 direction = (projectMotion(pos2) - projectMotion(pos1));
	if(length(direction) < (1.0/resolution.x)) return texture(deferredTex, uv).rgb;
	
	vec2 lookup = uv + direction * 0.05;
	
	vec3 color = vec3(0);
	for(int i=0;i<20;i++){
		color += texture(deferredTex, lookup).rgb;
		lookup += direction * 0.05;
	}
	
	return color / 20.0;
}

float lookupAO(vec2 fuv, float radius, int samp){
     float outc = 0;
     float counter = 0;
     float depthCenter = textureMSAA(originalNormalsTex, fuv, samp).a;
 	vec3 normalcenter = textureMSAA(originalNormalsTex, fuv, samp).rgb;
     for(float g = 0; g < mPI2; g+=0.8)
     {
         for(float g2 = 0; g2 < 1.0; g2+=0.33)
         {
             vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.012 * radius);
             float color = textureLod(aoTex, fuv + gauss, 0).r;
             float depthThere = textureMSAA(originalNormalsTex, fuv + gauss, samp).a;
 			vec3 normalthere = textureMSAA(originalNormalsTex, fuv + gauss, samp).rgb;
 			float weight = pow(max(0, dot(normalthere, normalcenter)), 32);
 			outc += color * weight;
 			counter+=weight;
             
         }
     }
     return counter == 0 ? textureLod(aoTex, fuv, 0).r : outc / counter;
 }
 vec3 lookupFog(vec2 fuv, float radius, int samp){
     vec3 outc =  textureLod(fogTex, fuv, 0).rgb;
     float counter = 1;
     for(float g = 0; g < mPI2; g+=0.8)
     {
         for(float g2 = 0.05; g2 < 1.0; g2+=0.14)
         {
             vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.012 * radius);
             vec3 color = textureLod(fogTex, fuv + gauss, 0).rgb;
 			float w = 1.0 - smoothstep(0.0, 1.0, g2);
 			outc += color * w;
 			counter+=w;
             
 
         }
     }
     return outc / counter;
}
layout(binding = 12) uniform samplerCube cube;

vec3 subsurfEnv(){
    float AOValue = 1.0;
    vec3 color = vec3(0);
    if(UseHBAO == 1) AOValue = lookupAO(UV, 1.0, 0);
    if(UseVDAO == 1) color = AOValue * texture(envLightTex, UV).rgb * 1;
    return (1.0 - AOValue) * color;
}

void main()
{
    float AOValue = 1.0;
    int samp = 0;
    vec3 color = makeMotion(UV);
    if(UseHBAO == 1 && samp == 0) AOValue = lookupAO(UV, 1.0, samp);
    if(UseVDAO == 1) color += AOValue * texture(envLightTex, UV).rgb * 1;
    if(UseVDAO == 0 && UseRSM == 0 && UseHBAO == 1) color = vec3(AOValue * 0.5);
	if(UseSSReflections){
		vec3 srdata = lookupSSRLowPass(UV, 1.0);
		color += srdata.rgb;
	}

    if(LensBlurAmount > 0.001 && DisablePostEffects == 0){
        float focus = CameraCurrentDepth;
        float adepth = textureMSAA(normalsDistancetex, vec2(0.5), 0).a;

        color = lensblur(avgdepth(UV), adepth, 0.99, 7.0);
    }

	//color = texture(dofFarTex, UV).aaa;
	//color = texture(dofNearTex, UV).rgb;	
	
	
	if(textureMSAAFull(normalsDistancetex, UV).a == 0.0){
        color = texture(cube, reconstructCameraSpaceDistance(UV, 1.0)).rgb;
    }
	if(DisablePostEffects == 0){
		if(UseBloom == 1) color += texture(bloomPassSource, UV).rgb * 0.2;
        color = ExecutePostProcessing(color, UV);
		//color = color / (1 + color);
		float gamma = 1.0/2.2;
		color = rgb_to_srgb(color);
	}
	
	float forwardDepth = texture(forwardPassBufferDepth, UV).r;
	float targetDepth = toLogDepth2(textureMSAAFull(normalsDistancetex, UV).a, 10000);
	/*if(forward.a < 0) {
		vec3 normalized = forward.rgb / (-forward.a);
		color = mix(color, normalized, -forward.a);
	}*/
	//if(forwardDepth < 1) color = vec3(forward.rgb / (-forward.a));
	
    outColor = clamp(vec4(color, toLogDepth(textureMSAAFull(normalsDistancetex, UV).a, 1000)), 0.0, 10000.0);
}