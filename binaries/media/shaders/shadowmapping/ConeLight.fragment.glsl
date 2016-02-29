#version 430 core

layout(location = 0) out vec4 outDiffuseColorDistance;
layout(location = 1) out vec4 outNormals;
in Data {
#include InOutStageLayout.glsl
} Input;
#include Mesh3dUniforms.glsl
#include LightingSamplers.glsl
#include Shade.glsl

uniform vec3 LightColor;
// change cubeMapTex to your cubemap name
uniform samplerCube cubeMapTex;
#define MMAL_LOD_REGULATOR 512
vec3 MMAL(vec3 normal, vec3 reflected, float roughness){
    float levels = float(textureQueryLevels(cubeMapTex)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
    vec3 result = textureLod(cubeMapTex, mix(reflected, normal, roughness), mx * levels).rgb;
    return result;
}


vec3 EnvironmentLight(vec3 cameraSpace, vec3 normal, vec3 specularColor, vec3 diffuseColor, float roughness)
{       
    vec3 dir = normalize(reflect(cameraSpace, normal));
    vec3 vdir = normalize(cameraSpace);
	
    vec3 reflected = MMAL(normal, dir, roughness) * specularColor;
    vec3 diffused = MMAL(normal, dir, 1.0) * diffuseColor;
	    
    return makeFresnel(1.0 - max(0, dot(normal, vdir)), reflected + diffused);
}
vec3 getSimpleLighting(){
	vec3 diffuse = DiffuseColor;
	if(UseDiffuseTex) diffuse = texture(diffuseTex, Input.TexCoord).rgb;
	
	vec3 specular = SpecularColor;
	if(UseSpecularTex) specular = texture(specularTex, Input.TexCoord).rgb; 
	
	float roughness = Roughness;
	if(UseRoughnessTex) roughness = texture(roughnessTex, Input.TexCoord).r; 
	
	vec3 radiance = shade(CameraPosition, specular, Input.Normal, Input.WorldPos, CameraPosition, LightColor, roughness, false) * (roughness);
	
	vec3 difradiance = shade(CameraPosition, diffuse, Input.Normal, Input.WorldPos, CameraPosition, LightColor, 1.0, false) * (roughness + 1.0);
	
	return (radiance + difradiance) * 0.5;
}

void main()
{
	float dist = distance(CameraPosition, Input.WorldPos);
	outDiffuseColorDistance = vec4(getSimpleLighting(), dist);
	outNormals = vec4((RotationMatrixes[Input.instanceId] * vec4(normalize(Input.Normal), 0)).rgb, dist);
	
}