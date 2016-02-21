#version 430 core

in vec2 UV;
out vec4 outColor;
uniform int UseVDAO;
uniform int UseHBAO;
uniform int UseFog;
uniform int DisablePostEffects;
uniform float VDAOGlobalMultiplier;

#include LogDepth.glsl

FragmentData currentFragment;

#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include EnvironmentLight.glsl
#include Direct.glsl
#include AmbientOcclusion.glsl

float AOValue = 1.0;

vec3 ApplyLighting(FragmentData data){
	if(UseHBAO == 1) AOValue = AmbientOcclusion(data) * 0.01;
	vec3 directlight = DirectLight(data);
	//vec3 envlight = UseVDAO == 1 ? (VDAOGlobalMultiplier * EnvironmentLight(data)) : vec3(0);

	directlight += AOValue * (UseVDAO == 1 ? (data.diffuseColor) : vec3(1.0));
	
	//if(data.diffuseColor.x > 1.0 && data.diffuseColor.y > 1.0 && data.diffuseColor.z > 1.0) directlight = (data.diffuseColor - 1.0) * AOValue;
	return directlight;
}

void main()
{	
	vec4 albedoRoughnessData = textureMSAAFull(albedoRoughnessTex, UV);
	vec4 normalsDistanceData = textureMSAAFull(normalsDistancetex, UV);
	vec4 specularBumpData = textureMSAAFull(specularBumpTex, UV);
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
	
	vec3 color = ApplyLighting(currentFragment);
	vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2);
	color.rgb = vec3(pow(color.r, gamma.r),
		pow(color.g, gamma.g),
		pow(color.b, gamma.b));
    outColor = clamp(vec4(color, 1.0), 0.0, 10000.0);
}