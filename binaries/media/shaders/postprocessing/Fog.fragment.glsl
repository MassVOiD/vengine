#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;

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

#include noise3D.glsl

//#define ENABLE_FOG_NOISE


float raymarchReflection(vec2 uvstart, vec2 uvend, int i){
	float fogDensity = 0;
	/*for(float m = 0.0; m< 1.0;m+= 0.05){
		vec2 pos = mix(uvstart, uvend, m);
		float att = 1.0 / pow(((distance(uvstart, pos)/1.0) + 1.0), 2.0) * 16.0;
		#ifdef ENABLE_FOG_NOISE
		//float fogNoise = (snoise(pos / 4.0 + vec3(0, -Time*0.2, 0)) + 1.0) / 2.0;
		float fogNoise = (snoise(vec4(pos * 4, Time)) + 1.0) / 2.0;
		#else
		float fogNoise = 1.0;
		#endif
		fogDensity += (1.0 / 100.0 * fogNoise * att) / (distance(UV, pos)*100 + 1.0);
	}*/
	return fogDensity;
}

mat4 PVMatrix = ProjectionMatrix * ViewMatrix;

float reflectPoint(vec3 point, vec3 dir, float dist, int i){
	vec4 pclip = PVMatrix * vec4(point, 1.0);
	vec2 pcspace = ((pclip.xyz / pclip.w).xy + 1.0) / 2.0;
	vec3 norm = texture(normalsTex, pcspace).rgb;
	vec3 reflected = reflect(dir, norm);
	vec3 newpoint = point + reflected * dist;
	vec4 p2clip = PVMatrix * vec4(newpoint, 1.0);
	vec2 p2cspace = ((p2clip.xyz / p2clip.w).xy + 1.0) / 2.0;
	return raymarchReflection(pcspace, p2cspace, i);
}

vec3 raymarchFog(vec3 start, vec3 end, float sampling){
	vec3 color1 = vec3(0);

	//vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;	
	
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
		
		
		float fogDensity = 0.0;
		float fogMultiplier = 2.4;
		
		for(float m = 0.0; m< 1.0;m+= sampling){
			vec3 pos = mix(start, end, m);
			//float att = 1.0 / pow(((distance(pos, LightsPos[i])/1.0) + 1.0), 2.0) * LightsColors[i].a;
			float att = 1;
			vec4 lightClipSpace = lightPV * vec4(pos, 1.0);
			#ifdef ENABLE_FOG_NOISE
			//float fogNoise = (snoise(pos / 4.0 + vec3(0, -Time*0.2, 0)) + 1.0) / 2.0;
			
			// rain
			float fogNoise = (snoise(vec3(pos.x*15, pos.y / 2 + Time*7, pos.z*15)) + 1.0) / 2.0;
			
			// snow
			//float fogNoise = (snoise(vec3(pos.x*5, pos.y * 5 + Time, pos.z*5)) + 1.0) / 2.0;
			//fogNoise = clamp((fogNoise - 0.8) * 20, 0, 1);
			
			#else
			float fogNoise = 1.0;
			#endif
			//float idle = 1.0 / 1000.0 * fogNoise * fogMultiplier;
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
		color1 += LightsColors[i].xyz * fogDensity;
		
	}
	return color1;
}

void main()
{
	
	vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;
    outColor = clamp(vec4(raymarchFog(CameraPosition, fragmentPosWorld3d, FogSamples), 1), 0, 1);
}