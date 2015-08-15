#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

out vec4 outColor;

layout(binding = 0) uniform sampler2D textureIn;

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

vec3 bilinearLightest(){
    ivec2 texSize = textureSize(textureIn,0);
    float lookupLengthX = 1.0 / texSize.x;
    float lookupLengthY = 1.0 / texSize.y;
    vec3 p1 = texture(textureIn, UV).rgb;
    vec3 p2 = texture(textureIn, UV + vec2(lookupLengthX, lookupLengthY)).rgb;
    vec3 p3 = texture(textureIn, UV + vec2(0, lookupLengthY)).rgb;
    vec3 p4 = texture(textureIn, UV + vec2(lookupLengthX, 0)).rgb;
    
    vec3 p5 = texture(textureIn, UV - vec2(lookupLengthX, lookupLengthY)).rgb;
    vec3 p6 = texture(textureIn, UV - vec2(0, lookupLengthY)).rgb;
    vec3 p7 = texture(textureIn, UV - vec2(lookupLengthX, 0)).rgb;
    return max(max(max(max(max(max(p1, p2), p3), p4), p5), p6), p7);
}

void main()
{	
    vec2 fragCoord = UV * resolution;
	texcoords(fragCoord);
	vec4 color1 = fxaa(textureIn, fragCoord);
    //vec4 color1 = vec4(bilinearLightest(), 1);
    outColor = color1*2;
}