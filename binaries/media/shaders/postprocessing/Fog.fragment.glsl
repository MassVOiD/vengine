#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;

out vec4 outColor;


const int MAX_FOG_SPACES = 256;
uniform int FogSpheresCount;
uniform vec3 FogSettings[MAX_FOG_SPACES]; //x: FogDensity, y: FogNoise, z: FogVelocity
uniform vec4 FogPositionsAndSizes[MAX_FOG_SPACES]; //w: Size
uniform vec4 FogVelocitiesAndBlur[MAX_FOG_SPACES]; //w: Blur
uniform vec4 FogColors[MAX_FOG_SPACES];


#include noise3D.glsl

//#define ENABLE_FOG_NOISE

vec3 raymarchFog(vec3 start, vec3 end, float sampling){
	vec3 color1 = vec3(0);

	//vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;	
	
    bool foundSun = false;
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(end, 1.0);
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && lightClipSpace.z < 0.0){
            if(foundSun) continue;
            else foundSun = true;
            //att = 1;
        }
		
		
		float fogDensity = 0.0;
		float fogMultiplier = 2.4;
        vec2 fuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
		vec3 lastPos = start - mix(start, end, sampling);
		for(float m = 0.0; m< 1.0;m+= sampling){
			vec3 pos = mix(start, end, m);
            float distanceMult = clamp(distance(lastPos, pos) * 2, 0, 33);
            lastPos = pos;
			float att = 1.0 / pow(((distance(pos, LightsPos[i])/1.0) + 1.0), 2.0) * LightsColors[i].a;
			//float att = 1;
            if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) att = 0.1;
			lightClipSpace = lightPV * vec4(pos, 1.0);
			#ifdef ENABLE_FOG_NOISE
			//float fogNoise = (snoise(pos / 4.0 + vec3(0, -Time*0.2, 0)) + 1.0) / 2.0;
			
			// rain
			//float fogNoise = (snoise(vec3(pos.x*15, pos.y / 2 + Time*7, pos.z*15)) + 1.0) / 2.0;
			
			// snow
			float fogNoise = (snoise(vec3(pos.x*5, pos.y * 5 + Time, pos.z*5)) + 1.0) / 2.0;
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
            vec2 frfuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
            float badass_depth = toLogDepthEx(distance(pos, LightsPos[i]), LightsFarPlane[i]);
            float diff = (badass_depth - lookupDepthFromLight(i, frfuv));
			if(diff < 0) {
				float culler = clamp(1.0 - distance(frfuv, vec2(0.5)) * 2.0, 0.0, 1.0);
				//float fogNoise = 1.0;
				fogDensity += idle + 1.0 / 200.0 * culler * fogNoise * fogMultiplier * att * distanceMult;
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
	
	vec3 fragmentPosWorld3d = FromCameraSpace(texture(worldPosTex, UV).xyz);
    outColor = clamp(vec4(raymarchFog(CameraPosition, fragmentPosWorld3d, FogSamples), 1), 0.0, 1.0);
}