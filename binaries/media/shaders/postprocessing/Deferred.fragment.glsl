#version 430 core

layout(location = 0) out vec4 outColor;

uniform int DisablePostEffects;
uniform float VDAOGlobalMultiplier;

#include LogDepth.glsl

FragmentData currentFragment;

#include Lighting.glsl
vec2 UV = gl_FragCoord.xy / resolution;
#include UsefulIncludes.glsl
#include Shade.glsl
#include Direct.glsl
#include AmbientOcclusion.glsl
#include RSM.glsl

uniform vec3 LightColor;
uniform vec3 LightPosition;
uniform vec4 LightOrientation;
uniform float LightAngle;
uniform int LightUseShadowMap;
uniform int LightShadowMapType;
uniform mat4 LightVPMatrix;


layout(binding = 20) uniform sampler2DShadow shadowMapSingle;

layout(binding = 21) uniform samplerCubeShadow shadowMapCube;

#define KERNEL 6
#define PCFEDGE 1
float PCFDeferred(vec2 uvi, float comparison){

    float shadow = 0.0;
    float pixSize = 1.0 / textureSize(shadowMapSingle,0).x;
    float bound = KERNEL * 0.5 - 0.5;
    bound *= PCFEDGE;
    for (float y = -bound; y <= bound; y += PCFEDGE){
        for (float x = -bound; x <= bound; x += PCFEDGE){
			vec2 uv = vec2(uvi+ vec2(x,y)* pixSize);
            shadow += texture(shadowMapSingle, vec3(uv, comparison));
        }
    }
	return shadow / (KERNEL * KERNEL);
}

vec3 ApplyLighting(FragmentData data, int samp)
{
	vec3 result = vec3(0);
	if(LightUseShadowMap == 1){
		if(LightShadowMapType == 0){
			vec4 lightClipSpace = LightVPMatrix * vec4(data.worldPos, 1.0);
			if(lightClipSpace.z > 0.0){
				vec3 lightScreenSpace = (lightClipSpace.xyz / lightClipSpace.w) * 0.5 + 0.5;   

				float percent = 0;
				if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) {
					percent = PCFDeferred(lightScreenSpace.xy, toLogDepth2(distance(data.worldPos, LightPosition), 10000) - 0.00001);
				}
				vec3 radiance = shade(CameraPosition, data.specularColor, data.normal, data.worldPos, LightPosition, LightColor, data.roughness, false) * (data.roughness);
				vec3 difradiance = shade(CameraPosition, data.diffuseColor, data.normal, data.worldPos, LightPosition, LightColor, 1.0, false) * (data.roughness + 1.0);
				result += (radiance + difradiance) * 0.5 * percent;
			}
		} else if(LightShadowMapType == 1){
			
			vec3 checkdir = normalize(data.worldPos - LightPosition);
			float percent = texture(shadowMapCube, vec4(checkdir, toLogDepth2(distance(data.worldPos, LightPosition), 10000) - 0.001));
			
			vec3 radiance = shade(CameraPosition, data.specularColor, data.normal, data.worldPos, LightPosition, LightColor, data.roughness, false) * (data.roughness);
			vec3 difradiance = shade(CameraPosition, data.diffuseColor, data.normal, data.worldPos, LightPosition, LightColor, 1.0, false) * (data.roughness + 1.0);
			result += (radiance + difradiance) * 0.5 * percent;
		
		} 
	} else if(LightUseShadowMap == 0){
		vec3 radiance = shade(CameraPosition, data.specularColor, data.normal, data.worldPos, LightPosition, LightColor, data.roughness, false) * (data.roughness);
		vec3 difradiance = shade(CameraPosition, data.diffuseColor, data.normal, data.worldPos, LightPosition, LightColor, 1.0, false) * (data.roughness + 1.0);
		result += (radiance + difradiance) * 0.5;
	}
	return result;
}

void main()
{	
	
	float MSAASampleFrequency = MSAADifference(albedoRoughnessTex, UV);
    int samples = min(int(mix(1, 8, MSAASampleFrequency)), 8);
    vec3 color  = vec3(0);
    
    for(int i=0;i<samples;i++){
        vec4 albedoRoughnessData = textureMSAA(albedoRoughnessTex, UV, i);
        vec4 normalsDistanceData = textureMSAA(normalsDistancetex, UV, i);
        vec4 specularBumpData = textureMSAA(specularBumpTex, UV, i);
        vec3 camSpacePos = reconstructCameraSpaceDistance(UV, normalsDistanceData.a);
        vec3 worldPos = FromCameraSpace(camSpacePos);
        
        currentFragment = FragmentData(
            albedoRoughnessData.rgb,
            specularBumpData.rgb,
            normalsDistanceData.rgb,
            vec3(1,0,0),
            worldPos,
            camSpacePos,
            normalsDistanceData.a,
            1.0,
            albedoRoughnessData.a,
            specularBumpData.a
        );	
        
        float stepsky = step(0.001, currentFragment.cameraDistance);
        color += stepsky * ApplyLighting(currentFragment, i);
    }
    color /= samples;
    outColor = clamp(vec4(color, 1.0), 0.0, 10000.0);
}