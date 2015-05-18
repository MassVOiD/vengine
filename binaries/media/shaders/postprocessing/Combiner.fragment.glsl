#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#define mPI (3.14159265)
#define mPI2 (2*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;

layout(binding = 0) uniform sampler2D color;
layout(binding = 1) uniform sampler2D depth;
layout(binding = 2) uniform sampler2D fog;
layout(binding = 3) uniform sampler2D fogDepth;
layout(binding = 4) uniform sampler2D lightpoints;
layout(binding = 5) uniform sampler2D bloom;
layout(binding = 6) uniform sampler2D globalIllumination;
layout(binding = 7) uniform sampler2D diffuseColor;
layout(binding = 8) uniform sampler2D normals;
layout(binding = 9) uniform sampler2D worldPos;
layout(binding = 10) uniform sampler2D lastworldPos;

float centerDepth;


vec3 lookupFog(vec2 fuv){
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 0; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.004);
			vec3 color = texture(fog, fuv + gauss).rgb;
			outc += color;
			counter++;
		}
	}
	return outc / counter;
}
vec3 lookupFogSimple(vec2 fuv){
	return texture(fog, fuv).rgb;
}
/*
vec3 lookupFog(vec2 fuv){
	vec3 outc = vec3(0);
	float near = 99;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 0; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.005);
			vec3 color = texture(fog, fuv + gauss).rgb;
			float fdepth = texture(fogDepth, fuv + gauss).r;
			if(abs(fdepth - centerDepth) < near){
				near = abs(fdepth - centerDepth);
				outc = color;
			}
		}
	}
	return outc;
}*/

vec3 lookupGIBlurred(vec2 giuv, float radius){
	vec3 outc = vec3(0);
	float last = 0;
	int counter = 0;
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 1; g2 < 6.0; g2+=1.0)
		{ 
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * radius);
			vec3 color = texture(globalIllumination, giuv + gauss).rgb;
			if(length(color) >= last){
				outc += color;
				counter++;
				last = length(color);
			}
		}
	}
	return outc / counter / 3 * texture(diffuseColor, giuv).rgb ;
}

vec3 lookupBloomBlurred(vec2 buv, float radius){
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = 0; g < mPI2 * 2; g+=0.6)
	{ 
		for(float g2 = 0; g2 < 1.0; g2+=0.15)
		{ 
			vec2 gauss = vec2(sin(g)*ratio, cos(g)) * (g2 * radius);
			vec4 color = texture(bloom, buv + gauss).rgba;
			outc += (color.rgb * color.a) * (1.0 - g2);
			counter++;
		}
	}
	return outc / counter;
}


vec3 lookupGIBilinearDepthNearest(vec2 giuv){
    //ivec2 texSize = textureSize(globalIllumination,0);
	//float lookupLengthX = 1.7 / texSize.x;
	//float lookupLengthY = 1.7 / texSize.y;
	//lookupLengthX = clamp(lookupLengthX, 0, 1);
	//lookupLengthY = clamp(lookupLengthY, 0, 1);
	vec3 gi =  (texture(globalIllumination, giuv ).rgb);
	return (texture(diffuseColor, giuv).rgb) * gi	* 1.1 + (texture(color, giuv).rgb) * gi	* 1.1;
}

vec3 lookupGI(vec2 guv){
	return lookupGIBilinearDepthNearest(guv);
}
vec3 lookupGISimple(vec2 giuv){
	return texture(globalIllumination, giuv ).rgb;
}
/*
vec3 subsurfaceScatteringExperiment(){
	float frontDistance = reverseLog(texture(depth, UV).r);
	float backDistance = reverseLog(texture(backDepth, UV).r);
	float deepness =  backDistance - frontDistance;
	return vec3(
		1.0 - deepness * 15
	);
}*/

vec2 proj(vec3 dir){
	vec3 positionCenter = texture(worldPos, UV).rgb; 
	vec3 dirPosition = positionCenter + dir * 0.05;
	
	vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(normalize(positionCenter), 1.0);
	vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	clipspace = (ProjectionMatrix * ViewMatrix) * vec4(normalize(dirPosition), 1.0);
	vec2 sspace2 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	return normalize(sspace2 - sspace1);
}

vec2 refractUV(){
	vec3 rdir = normalize(texture(worldPos, UV).rgb - CameraPosition);
	vec3 crs1 = normalize(cross(CameraPosition, texture(worldPos, UV).rgb));
	vec3 crs2 = normalize(cross(crs1, rdir));
	vec3 rf = refract(rdir, texture(normals, UV).rgb, 0.02);
	return UV + proj(rf) * 0.3;
}


vec3 motionBlurExperiment(vec2 uv){
	vec3 outc = texture(color, uv).rgb;
	vec3 centerPos = texture(worldPos, uv).rgb;
	vec2 nearestUV = uv;
	float worldDistance = 999999;
	
	for(float g = 0; g < mPI2 * 2; g+=0.9)
	{ 
		for(float g2 = 0.0; g2 < 4.0; g2+=0.3)
		{ 
			vec2 dsplc = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.002);
			vec3 pos = texture(lastworldPos, uv + dsplc).rgb;
			float ds = distance(pos, centerPos);
			if(worldDistance > ds){
				worldDistance = ds;
				nearestUV = uv + dsplc;
			}
		}
	}	
	//if(distance(nearestUV, uv) < 0.001) return outc;
	int counter = 0;
	outc = vec3(0);
	vec2 direction = (nearestUV - uv);
	for(float g = 0; g < 1; g+=0.1)
	{ 
		outc += texture(color, mix(uv - direction, uv + direction, g)).rgb;
		counter++;
	}
	return outc / counter;
}

uniform int UseSimpleGI;
uniform int UseFog;
uniform int UseLightPoints;
uniform int UseDepth;
uniform int UseBloom;
uniform int UseDeferred;
uniform int UseBilinearGI;

layout (std430, binding = 2) buffer SSBOTest
{
  vec3 BufValues[]; 
}; 

void main()
{
	vec2 nUV = UV;
	vec3 color1 = vec3(0);
	if(texture(diffuseColor, UV).a < 0.99){
		nUV = refractUV();
	   //color1 += texture(color, UV).rgb * texture(diffuseColor, UV).a;
	}
	//if(UseDeferred == 1) color1 += texture(color, nUV).rgb;
	if(UseDeferred == 1) color1 += motionBlurExperiment(nUV);
	//if(UseFog == 1) color1 += lookupFog(nUV) * FogContribution;
	if(UseFog == 1) color1 += lookupFogSimple(nUV) * FogContribution;
	if(UseLightPoints == 1) color1 += texture(lightpoints, nUV).rgb;
	if(UseBloom == 1) color1 += lookupBloomBlurred(nUV, 0.1).rgb * BloomContribution;
	if(UseDepth == 1) color1 += texture(depth, nUV).rrr;
	if(UseBilinearGI == 1) color1 += lookupGIBilinearDepthNearest(nUV);
	if(UseSimpleGI == 1) color1 += lookupGIBlurred(nUV, 0.002) * GIContribution;
	//if(UseSimpleGI == 1) color1 += lookupGISimple(nUV) * GIContribution;
	centerDepth = texture(depth, UV).r;
	
	gl_FragDepth = centerDepth;
	
	/*if(UV.x > 0 && UV.x < 0.05) color1 = (BufValues[0]);
	if(UV.x > 0.05 && UV.x < 0.1) color1 = (BufValues[1]);
	if(UV.x > 0.1 && UV.x < 0.15) color1 = (BufValues[2]);
	if(UV.x > 0.15 && UV.x < 0.2) color1 = (BufValues[3]);
	*/
    outColor = vec4(color1, 1);
}