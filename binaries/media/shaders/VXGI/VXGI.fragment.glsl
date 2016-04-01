#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

out vec4 outColor;

FragmentData currentFragment;

#include VXGITracing.glsl

void main()
{
    vec3 color = vec3(0);
    
    vec4 albedoRoughnessData = textureMSAA(albedoRoughnessTex, UV, 0);
	vec4 normalsDistanceData = textureMSAA(normalsDistancetex, UV, 0);
	vec4 specularBumpData = textureMSAA(specularBumpTex, UV, 0);
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
    
    color +=  traceConeDiffuse(currentFragment);
    color +=  traceConeSpecular(currentFragment) * specularBumpData.rgb ;
    //color = traceConeDiffuse();
    /*
    color = traceVisDir(vec3(-2, 1, 0)) 
    + traceVisDir(vec3(-1, 1, 0)) 
    + traceVisDir(vec3(0, 1, 0)) 
    + traceVisDir(vec3(1, 1, 0)) 
    + traceVisDir(vec3(2, 1, 0));
    color *= 0.2;*/
    color += max(vec3(0.0), albedoRoughnessData.rgb - 1.0);
    outColor = clamp(vec4(color, 0), 0.0, 10000.0);
}