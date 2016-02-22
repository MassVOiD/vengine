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

uniform float ParallaxHeightMultiplier;

uniform int Instances;

layout (std430, binding = 0) buffer MMBuffer
{
  mat4 ModelMatrixes[]; 
}; 
layout (std430, binding = 1) buffer RMBuffer
{
  mat4 RotationMatrixes[]; 
}; 
layout (std430, binding = 2) buffer IDMBuffer
{
  uint InstancedIds[]; 
}; 
uniform vec3 CameraPosition;
uniform vec3 CameraDirection;
uniform float Time;
uniform int FrameINT;

uniform vec3 SpecularColor;
uniform vec3 DiffuseColor;
uniform float Alpha;
uniform float Brightness;

uniform int UseNormalsTex;
uniform int UseBumpTex;
uniform int UseAlphaTex;
uniform int UseRoughnessTex;
uniform int UseDiffuseTex;
uniform int UseSpecularTex;

uniform float Roughness;
//uniform float Metalness;
uniform float ReflectionStrength;
uniform float RefractionStrength;
uniform float LodDistanceStart;
uniform float LodDistanceEnd;
uniform vec2 resolution;
float ratio = resolution.y/resolution.x;


uniform float AORange;
uniform float AOStrength;
uniform float AOAngleCutoff;
uniform float VDAOMultiplier;
uniform float VDAOSamplingMultiplier;
uniform float VDAORefreactionMultiplier;
uniform float SubsurfaceScatteringMultiplier;
// settings
uniform float ShadowsBlur;
uniform float ShadowsSamples;
uniform float LightPointSize;
uniform float SimpleLightPointSize;