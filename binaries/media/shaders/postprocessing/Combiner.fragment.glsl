#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl
#define mPI (3.14159265)
#define mPI2 (2*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;

layout(binding = 0) uniform sampler2D color;
layout(binding = 1) uniform sampler2D depth;
layout(binding = 2) uniform sampler2D fog;
layout(binding = 3) uniform sampler2D lightpoints;
layout(binding = 4) uniform sampler2D bloom;
layout(binding = 5) uniform sampler2D globalIllumination;
layout(binding = 6) uniform sampler2D backDepth;

vec3 lookupFog(){
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 0; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.005);
			vec3 color = texture(fog, UV + gauss).rgb;
			outc += color;
			counter++;
		}
	}
	return outc / counter;
}

vec3 lookupFogSimple(){
	return texture(fog, UV).rgb;
}

float centerDepth;

vec3 lookupGIBilinearDepthNearest(vec2 giuv){
    ivec2 texSize = textureSize(globalIllumination,0);
	float lookupLengthX = 1.0 / texSize.x;
	float lookupLengthY = 1.0 / texSize.y;
	lookupLengthX = clamp(lookupLengthX, 0, 1);
	lookupLengthY = clamp(lookupLengthY, 0, 1);
	return (texture(globalIllumination, giuv + vec2(-lookupLengthX, -lookupLengthY)).rgb
	+ texture(globalIllumination, giuv + vec2(lookupLengthX, -lookupLengthY)).rgb
	+ texture(globalIllumination, giuv + vec2(-lookupLengthX, lookupLengthY)).rgb
	+ texture(globalIllumination, giuv + vec2(lookupLengthX, lookupLengthY)).rgb) / 4;
}

vec3 lookupGI(){
	return lookupGIBilinearDepthNearest(UV);
}
vec3 lookupGISimple(vec2 giuv){
	return texture(globalIllumination, giuv ).rgb;
}

vec3 subsurfaceScatteringExperiment(){
	float frontDistance = reverseLog(texture(depth, UV).r);
	float backDistance = reverseLog(texture(backDepth, UV).r);
	float deepness =  backDistance - frontDistance;
	return vec3(
		1.0 - deepness * 15
	);
}

void main()
{
	vec3 color1 = texture(color, UV).rgb;
	color1 += lookupFog();
	color1 += texture(lightpoints, UV).rgb;
	color1 += texture(bloom, UV).rgb;
	centerDepth = texture(depth, UV).r;
	//color1 += lookupGI();
	vec3 gi = color1 + lookupGISimple(UV);
	color1 = gi;
	
	
	gl_FragDepth = centerDepth;
	
	
    outColor = vec4(color1, 1);
}