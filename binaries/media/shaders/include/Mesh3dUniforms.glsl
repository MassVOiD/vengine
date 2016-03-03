//uniform mat4 ViewMatrix;
//uniform mat4 ProjectionMatrix;
uniform mat4 VPMatrix;

const int MAX_LIGHTS = 6;
uniform int LightsCount;
uniform mat4 LightsPs[MAX_LIGHTS];
uniform mat4 LightsVs[MAX_LIGHTS];
uniform int LightsShadowMapsLayer[MAX_LIGHTS];
uniform vec3 LightsPos[MAX_LIGHTS];
uniform float LightsFarPlane[MAX_LIGHTS];
uniform vec4 LightsColors[MAX_LIGHTS];
uniform float LightsBlurFactors[MAX_LIGHTS];
uniform int LightsExclusionGroups[MAX_LIGHTS];

uniform vec4 LightsConeLB[MAX_LIGHTS];
uniform vec4 LightsConeLB2BR[MAX_LIGHTS];
uniform vec4 LightsConeLB2TL[MAX_LIGHTS];


uniform int Instances;

struct Material{
	vec4 diffuseColor;
	vec4 specularColor;
	vec4 roughnessAndParallaxHeight;
	
	uvec2 diffuseAddr;
	uvec2 specularAddr;
	uvec2 alphaAddr;
	uvec2 roughnessAddr;
	uvec2 bumpAddr;
	uvec2 normalAddr;
};

layout (std430, binding = 7) buffer MatBuffer
{
  Material Materials[]; 
};
uniform int MaterialIndex;

Material getCurrentMaterial(){
	//return Materials[MaterialIndex];
	Material mat = Material(
		Materials[MaterialIndex].diffuseColor,
		Materials[MaterialIndex].specularColor,
		Materials[MaterialIndex].roughnessAndParallaxHeight,
		
		Materials[MaterialIndex].diffuseAddr,
		Materials[MaterialIndex].specularAddr,
		Materials[MaterialIndex].alphaAddr,
		Materials[MaterialIndex].roughnessAddr,
		Materials[MaterialIndex].bumpAddr,
		Materials[MaterialIndex].normalAddr
	);
	return mat;
}

Material currentMaterial = getCurrentMaterial();

struct ModelInfo{
	vec4 Rotation;
	vec3 Translation;
	uint Id;
	vec4 Scale;
};

layout (std430, binding = 0) buffer MMBuffer
{
  ModelInfo ModelInfos[]; 
}; 

#include Quaternions.glsl

vec3 transform_vertex(int info, vec3 vertex){
	vec3 result = vertex;
	result *= ModelInfos[info].Scale.xyz;
	result = quat_mul_vec(ModelInfos[info].Rotation, result);
	result += ModelInfos[info].Translation.xyz;
	return result;
}

uniform vec3 CameraPosition;
//uniform vec3 CameraDirection;
uniform float Time;

#define SpecularColor currentMaterial.specularColor.xyz
#define DiffuseColor currentMaterial.diffuseColor.xyz

uniform float Brightness;

#define UseNormalsTex (currentMaterial.normalAddr.x > 0)
#define UseBumpTex (currentMaterial.bumpAddr.x > 0)
#define UseAlphaTex (currentMaterial.alphaAddr.x > 0)
#define UseRoughnessTex (currentMaterial.roughnessAddr.x > 0)
#define UseDiffuseTex (currentMaterial.diffuseAddr.x > 0)
#define UseSpecularTex (currentMaterial.specularAddr.x > 0)


#extension GL_ARB_bindless_texture : require
#define bumpTex sampler2D(currentMaterial.bumpAddr)
#define alphaTex sampler2D(currentMaterial.alphaAddr)
#define diffuseTex sampler2D(currentMaterial.diffuseAddr)
#define normalsTex sampler2D(currentMaterial.normalAddr)
#define specularTex sampler2D(currentMaterial.specularAddr)
#define roughnessTex sampler2D(currentMaterial.roughnessAddr)

#define Roughness currentMaterial.roughnessAndParallaxHeight.x
#define ParallaxHeightMultiplier currentMaterial.roughnessAndParallaxHeight.y
//uniform float Metalness;

uniform vec2 resolution;
float ratio = resolution.y/resolution.x;

uniform int UseVDAO;
uniform int UseHBAO;
uniform int UseFog;
uniform int UseBloom;
uniform int UseDeferred;
uniform int UseDepth;
uniform int UseCubeMapGI;
uniform int UseRSM;
uniform int UseSSReflections;
