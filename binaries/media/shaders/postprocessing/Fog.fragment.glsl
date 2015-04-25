#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
//layout(binding = 31) uniform sampler2D normalsTex;

out vec4 outColor;

float testVisibility(vec2 uv1, vec2 uv2, vec3 lpos) {
	vec3 d3d1Front = texture(worldPosTex, uv1).rgb;
	vec3 d3d2 = lpos;
	float ovis = 1;
	for(float i=0;i<1.0;i+= 0.01) { 
		vec2 ruv = mix(uv1, uv2, i);
		if(ruv.x < 0 || ruv.x > 1 || ruv.y < 0 || ruv.y > 1) continue;
		float rd3dFront = distance(CameraPosition, texture(worldPosTex, ruv).rgb);
		if(rd3dFront < distance(CameraPosition, mix(d3d1Front, d3d2, i))) {
			ovis -= 0.09;
			if(ovis <= 0) return 0;
		}
	}
	return ovis;
}

const int MAX_FOG_SPACES = 256;
uniform int FogSpheresCount;
uniform vec3 FogSettings[MAX_FOG_SPACES]; //x: FogDensity, y: FogNoise, z: FogVelocity
uniform vec4 FogPositionsAndSizes[MAX_FOG_SPACES]; //w: Size
uniform vec4 FogVelocitiesAndBlur[MAX_FOG_SPACES]; //w: Blur
uniform vec4 FogColors[MAX_FOG_SPACES];

const int MAX_SIMPLE_LIGHTS = 2000;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

#include noise4D.glsl

//#define ENABLE_FOG_NOISE

void main()
{
	vec3 color1 = vec3(0);

	vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;	
	
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
		
		
		float fogDensity = 0.0;
		float fogMultiplier = 0.4;
		
		for(float m = 0.0; m< 1.0;m+= FogSamples){
			vec3 pos = mix(CameraPosition, fragmentPosWorld3d, m);
			float att = 1.0 / pow(((distance(pos, LightsPos[i])/1.0) + 1.0), 2.0) * 390.0;
			vec4 lightClipSpace = lightPV * vec4(pos, 1.0);
			#ifdef ENABLE_FOG_NOISE
			//float fogNoise = (snoise(pos / 4.0 + vec3(0, -Time*0.2, 0)) + 1.0) / 2.0;
			float fogNoise = (snoise(vec4(pos * 4, Time)) + 1.0) / 2.0;
			#else
			float fogNoise = 1.0;
			#endif
			//float idle = 1.0 / 250.0 * fogNoise * fogMultiplier;
			float idle = 0.0;
			if(lightClipSpace.z < 0.0){ 
				fogDensity += idle;
				continue;
			}
			vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
			if(lightScreenSpace.x < 0.0 || lightScreenSpace.x > 1.0 || lightScreenSpace.y < 0.0 || lightScreenSpace.y > 1.0){ 
				fogDensity += idle;
				continue;
			}
			if(toLogDepth(distance(pos, LightsPos[i])) < lookupDepthFromLight(i,lightScreenSpace)) {
				float culler = clamp(1.0 - distance(lightScreenSpace, vec2(0.5)) * 2.0, 0.0, 1.0);
				//float fogNoise = 1.0;
				fogDensity += idle + 1.0 / 200.0 * culler * fogNoise * fogMultiplier * att;
			} else {
				fogDensity += idle;
			}
		}
		color1 += LightsColors[i].xyz * LightsColors[i].a * fogDensity;
		
	}
	
/*
	for(int i=0;i<SimpleLightsCount;i++){

		float fogDensity = 0.0;
		float fogMultiplier = 0.4;
		
		for(float m = 0.0; m< 1.0;m+= 0.030){
			vec3 pos = mix(CameraPosition, fragmentPosWorld3d, m);
			float att = 1.0 / pow(((distance(pos, SimpleLightsPos[i])/1.0) + 1.0), 2.0) * 40.0;
			
			#ifdef ENABLE_FOG_NOISE
			//float fogNoise = (snoise(pos / 4.0 + vec3(0, -Time*0.2, 0)) + 1.0) / 2.0;
			float fogNoise = (snoise(vec4(pos * 4, Time)) + 1.0) / 2.0;
			#else
			float fogNoise = 1.0;
			#endif
			//float idle = 1.0 / 250.0 * fogNoise * fogMultiplier;
			float idle = 0.0;
			
			vec4 clipspace1 = (ProjectionMatrix * ViewMatrix) * vec4(SimpleLightsPos[i], 1.0);
			vec2 sspace1 = ((clipspace1.xyz / clipspace1.w).xy + 1.0) / 2.0;
			
			vec4 clipspace2 = (ProjectionMatrix * ViewMatrix) * vec4(pos, 1.0);
			vec2 sspace2 = ((clipspace2.xyz / clipspace2.w).xy + 1.0) / 2.0;
			
			float vis = testVisibility(sspace2, sspace1, SimpleLightsPos[i]);			

			if(vis > 0.1) {
				fogDensity += idle + 1.0 / 200.0 * fogNoise * fogMultiplier * att * vis;
			} else {
				fogDensity += idle;
			}
		}
		color1 += SimpleLightsColors[i].xyz * fogDensity;
		
	}	
	*/
    outColor = vec4(color1, 1);
}