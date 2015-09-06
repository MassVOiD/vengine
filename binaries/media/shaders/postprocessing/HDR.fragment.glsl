#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

layout(binding = 0) uniform sampler2D textureIn;
layout(binding = 3) uniform sampler2D texDepth;
layout(binding = 4) uniform sampler2D bloom;
layout(binding = 5) uniform sampler2D worldPosTex;
layout(binding = 9) uniform sampler2D numbersTex;
layout(binding = 10) uniform sampler2D lastworldPos;

uniform int Numbers[12];
uniform int NumbersCount;


uniform float Brightness;

out vec4 outColor;


#ifndef FXAA_REDUCE_MIN
    #define FXAA_REDUCE_MIN   (1.0/ 128.0)
#endif
#ifndef FXAA_REDUCE_MUL
    #define FXAA_REDUCE_MUL   (1.0 / 8.0)
#endif
#ifndef FXAA_SPAN_MAX
    #define FXAA_SPAN_MAX     8.0
#endif

//optimized version for mobile, where dependent 
//texture reads can be a bottleneck
vec2 v_rgbNW;
vec2 v_rgbNE;
vec2 v_rgbSW;
vec2 v_rgbSE;
vec2 v_rgbM;

void texcoords(vec2 fragCoord) {
	vec2 inverseVP = 1.0 / resolution.xy;
	v_rgbNW = (fragCoord + vec2(-1.0, -1.0)) * inverseVP;
	v_rgbNE = (fragCoord + vec2(1.0, -1.0)) * inverseVP;
	v_rgbSW = (fragCoord + vec2(-1.0, 1.0)) * inverseVP;
	v_rgbSE = (fragCoord + vec2(1.0, 1.0)) * inverseVP;
	v_rgbM = vec2(fragCoord * inverseVP);
}

vec4 fxaa(sampler2D tex, vec2 fragCoord) {
    vec4 color;
    mediump vec2 inverseVP = vec2(1.0 / resolution.x, 1.0 / resolution.y);
    vec3 rgbNW = texture(tex, v_rgbNW).xyz;
    vec3 rgbNE = texture(tex, v_rgbNE).xyz;
    vec3 rgbSW = texture(tex, v_rgbSW).xyz;
    vec3 rgbSE = texture(tex, v_rgbSE).xyz;
    vec4 texColor = texture(tex, v_rgbM);
    vec3 rgbM  = texColor.xyz;
    vec3 luma = vec3(0.299, 0.587, 0.114);
    float lumaNW = dot(rgbNW, luma);
    float lumaNE = dot(rgbNE, luma);
    float lumaSW = dot(rgbSW, luma);
    float lumaSE = dot(rgbSE, luma);
    float lumaM  = dot(rgbM,  luma);
    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
    
    mediump vec2 dir;
    dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
    dir.y =  ((lumaNW + lumaSW) - (lumaNE + lumaSE));
    
    float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) *
                          (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
    
    float rcpDirMin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
    dir = min(vec2(FXAA_SPAN_MAX, FXAA_SPAN_MAX),
              max(vec2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX),
              dir * rcpDirMin)) * inverseVP;
    
    vec3 rgbA = 0.5 * (
        texture(tex, fragCoord * inverseVP + dir * (1.0 / 3.0 - 0.5)).xyz +
        texture(tex, fragCoord * inverseVP + dir * (2.0 / 3.0 - 0.5)).xyz);
    vec3 rgbB = rgbA * 0.5 + 0.25 * (
        texture(tex, fragCoord * inverseVP + dir * -0.5).xyz +
        texture(tex, fragCoord * inverseVP + dir * 0.5).xyz);

    float lumaB = dot(rgbB, luma);
    if ((lumaB < lumaMin) || (lumaB > lumaMax))
        color = vec4(rgbA, texColor.a);
    else
        color = vec4(rgbB, texColor.a);
    return color;
}


uniform float LensBlurAmount;
uniform float CameraCurrentDepth;

	
float centerDepth;
#define mPI (3.14159265)

float ngonsides = 5;
float sideLength = sqrt(1+1-2*cos(mPI2 / ngonsides));

float PIOverSides = mPI2/ngonsides;
float PIOverSidesOver2 = PIOverSides/2;
float triangleHeight = 0.85;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
	vec3 finalColor = vec3(0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	float centerDepthDistance = abs((centerDepth) - (depthfocus));
	//float centerDepth = texture(texDepth, UV).r;
    float focus = texture(texDepth, vec2(0.5)).r;
    for(float x = 0; x < mPI2; x+=0.5){ 
        for(float y=0.1;y<1.0;y+= 0.1){  
			
			//ngon
		
			vec2 crd = vec2(sin(x + y) * ratio, cos(x + y)) * (rand(UV + vec2(x, y)) * 0.125);
			//if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.01 * amount;  
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
            float depth = length(texture(worldPosTex, coord).xyz);
            if(distance(coord, UV.xy) < max_radius){  
                //if((depth - focus) > 0.005) continue;     
                vec3 texel = texture(textureIn, coord).rgb;
                float w = length(texel) + 0.2;
                float dpf = abs(focus - toLogDepth(depth))*0.2+0.8;
                w*=dpf;
                weight+=w;
                finalColor += texel * w;
            
            }
        }
    }
	return weight == 0.0 ? vec3(0.0) : finalColor/weight;
}

uniform int UseBloom;
vec3 lookupBloomBlurred(vec2 buv, float radius){
	vec3 outc = vec3(0);
	float counter = 0;
	for(float g = 0; g < mPI2; g+=0.3)
	{ 
		for(float g2 = 0; g2 < 1.0; g2+=0.1131)
		{ 
			vec2 gauss = vec2(sin(g)*ratio, cos(g)) * (rand(UV+vec2(g, g2)) * radius);
			vec4 color = texture(bloom, buv + gauss).rgba;
            float w = max(0, (length(color) - 1.3) ) * 1.1;
            counter += max(w, 0.1);
			outc += (color.rgb * color.a) * w * (1.0 - g2) * 1.4;
			//counter++;
		}
	}
	return outc / counter;
}

float avgdepth(vec2 buv){
	float outc = float(0);
	float counter = 0;
    float fDepth = length(texture(worldPosTex, vec2(0.5, 0.5)).rgb);
	for(float g = 0; g < mPI2; g+=0.5)
	{ 
		for(float g2 = 0; g2 < 1.0; g2+=0.33)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.2 * LensBlurAmount);
			vec3 color = texture(worldPosTex, buv + gauss).xyz;
            float adepth = length(color);
            //float avdepth = clamp(pow(abs(depth - focus), 0.9) * 53.0 * LensBlurAmount, 0.0, 4.5 * LensBlurAmount);		
            float f = log((LensBlurAmount+1))*11; //focal length in mm
            float d = fDepth*1000.0; //focal plane in mm
            float o = adepth*1000.0; //depth in mm
            
            float fstop = 8.0;
            float CoC = 0.07;
            float a = (o*f)/(o-f); 
            float b = (d*f)/(d-f); 
            float c = (d-f)/(d*fstop*CoC); 
            
            float blur = abs(a-b)*c;
            outc += clamp(blur * 50,0.0,4.0) * 10;
			counter++;
		}
	}
	return (outc / counter)*2;
}
void main()
{
	vec2 fragCoord = UV * resolution;
	texcoords(fragCoord);
	vec4 color1 = fxaa(textureIn, fragCoord);
    //vec4 color1 = vec4(0,0,0,1);
	float depth = texture(texDepth, UV).r;
	centerDepth = depth;
	if(LensBlurAmount > 0.001){
		float focus = CameraCurrentDepth;
		float adepth = length(texture(worldPosTex, UV).xyz);
		//float fDepth = reverseLog(CameraCurrentDepth);

		color1.xyz = lensblur(avgdepth(UV), 1, 0.09, 7.0);
	}
    float letterpixels = 10;
    float maxx = NumbersCount * (0.5 / letterpixels);
    if(UV.x < maxx && UV.y < 0.05){
        vec2 nuv = vec2(UV.x / maxx, UV.y / 0.05);
        float letterx = 1.0 / letterpixels;
        vec2 nuv2 = vec2(mod(UV.x / maxx, letterx), 1.0 - UV.y / 0.05);
        for(int i=0;i<NumbersCount;i++){
            vec2 numbUVOffset = vec2(i*letterx, 0);
            if(nuv.x > numbUVOffset.x && nuv.x < numbUVOffset.x + letterx){
                vec2 numbUV = vec2(Numbers[i]*letterx, 0) + nuv2;
                float data = texture(numbersTex, numbUV).a;
                color1 += data;
            }
        }
    }
				
	if(UseBloom == 1) color1.xyz += lookupBloomBlurred(UV, 0.1).rgb * BloomContribution;  
    outColor = clamp(color1, 0.0, 1.0);
}