#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

layout(binding = 0) uniform sampler2D textureIn;
layout(binding = 2) uniform sampler2D texDepth;


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

vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
	vec3 finalColor = texture(textureIn, UV).rgb;  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	float centerDepthDistance = abs((centerDepth) - (depthfocus));
	//float centerDepth = texture(texDepth, UV).r;
    for(float x = 0; x < mPI2; x+=0.2){ 
        for(float y=0;y<samples;y+= 1.0){  
			
			//ngon
			float xt = x;
			while(xt > PIOverSides) xt -= PIOverSides;
			float ndist = abs(xt - PIOverSidesOver2);
			ndist /= PIOverSidesOver2;
			float rat = mix(1, triangleHeight, ndist);
		
			vec2 crd = vec2(sin(x) * ratio, cos(x)) * (y * 0.25 * rat);
			//if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.01 * amount;  
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
            if(distance(coord, UV.xy) < max_radius){  
                float depth = texture(texDepth, coord).r;
				if(centerDepth - depth < 0.05 || centerDepthDistance > 0.4 || depth > 0.99 || centerDepth > 0.99){
					vec3 texel = texture(textureIn, coord).rgb;
					float w = length(texel) + 0.1;
					weight+=1;
					finalColor += texel;
				}
            }
        }
    }
	return finalColor/weight;
}

void main()
{
	vec2 fragCoord = UV * resolution;
	texcoords(fragCoord);
	vec4 color1 = fxaa(textureIn, fragCoord);
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
		blur = clamp(blur,0.0,1.0) * 50;
		color1.xyz = lensblur(blur, focus, 0.03, 7.0);
	}
	vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2) / Brightness;
	color1.rgb = vec3(pow(color1.r, gamma.r),
                  pow(color1.g, gamma.g),
                  pow(color1.b, gamma.b));
				  
    outColor = clamp(color1, 0.0, 1.0);
	
}