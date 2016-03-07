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

float lookupAO(vec2 fuv, float radius, int samp){
    float outc = 0;
    float counter = 0;
    float depthCenter = textureMSAA(normalsDistancetex, fuv, samp).a;
	vec3 normalcenter = textureMSAA(normalsDistancetex, fuv, samp).rgb;
    for(float g = 0; g < mPI2; g+=0.8)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.33)
        {
            vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.008 * radius);
            float color = textureLod(aoTex, fuv + gauss, 0).r;
            float depthThere = textureMSAA(normalsDistancetex, fuv + gauss, samp).a;
			vec3 normalthere = textureMSAA(normalsDistancetex, fuv + gauss, samp).rgb;
			float weight = pow(max(0, dot(normalthere, normalcenter)), 32);
			outc += color * weight;
			counter+=weight;
            
        }
    }
    return counter == 0 ? textureLod(aoTex, fuv, 0).r : outc / counter;
}

vec3 ApplyLighting(FragmentData data, int samp){
	vec3 result = vec3(0);
	if(UseHBAO == 1 && samp == 0) AOValue = lookupAO(UV, 1.0, samp);
	if(UseDeferred == 1) result += DirectLight(data);
	if(UseVDAO == 1) result += AOValue * texture(envLightTex, UV).rgb;
	if(UseRSM == 1) result += AOValue * RSM(data);
	if(UseFog == 1) result += texture(fogTex, UV).rgb;
	if(UseDepth == 1) result = mix(result, vec3(1), 1.0 - CalculateFallof(data.cameraDistance*0.1));
	if(UseVDAO == 0 && UseRSM == 0 && UseHBAO == 1) result = vec3(AOValue * 0.5);
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
        color += stepsky * ApplyLighting(currentFragment, i) + (1.0 - stepsky) * vec3(1.0);
    }
    color /= samples;
	//color = textureMSAA(albedoRoughnessTex, UV, 0).rgb;
    outColor = clamp(vec4(color, 1.0), 0.0, 10000.0);
}