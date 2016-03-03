#version 430 core

layout(location = 0) out vec4 outColor;

in vec2 UV;
uniform int DisablePostEffects;
uniform float VDAOGlobalMultiplier;

#include LogDepth.glsl

FragmentData currentFragment;

#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include Direct.glsl
#include AmbientOcclusion.glsl
#include RSM.glsl

float AOValue = 1.0;

vec3 ApplyLighting(FragmentData data){
	if(UseHBAO == 1) AOValue = AmbientOcclusion(data);
	vec3 result = vec3(0);
	if(UseDeferred == 1) result += DirectLight(data);
	if(UseVDAO == 1) result += AOValue * texture(envLightTex, UV).rgb;
	if(UseRSM == 1) result += AOValue * RSM(data);
	if(UseDepth == 1) result = mix(result, vec3(1), 1.0 - CalculateFallof(data.cameraDistance*0.1));
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
        
        color += ApplyLighting(currentFragment);
    }
    color /= samples;
    if(textureMSAAFull(normalsDistancetex, UV).a < 0.001)color.rgb = vec3(0.0);
    outColor = clamp(vec4(color, 1.0), 0.0, 10000.0);
}