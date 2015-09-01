uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform float LogEnchacer;
uniform float FarPlane;

const int MAX_LIGHTS = 6;
uniform int LightsCount;
uniform mat4 LightsPs[MAX_LIGHTS];
uniform mat4 LightsVs[MAX_LIGHTS];
uniform vec3 LightsPos[MAX_LIGHTS];
uniform float LightsFarPlane[MAX_LIGHTS];
uniform vec4 LightsColors[MAX_LIGHTS];
uniform vec2 LightsRanges[MAX_LIGHTS];
uniform int LightsMixModes[MAX_LIGHTS];
#define LIGHT_MIX_MODE_ADDITIVE 0
#define LIGHT_MIX_MODE_EXCLUSIVE 1
#define LIGHT_MIX_MODE_SUN_CASCADE 2


uniform int Instances;
uniform mat4 ModelMatrix;
uniform mat4 RotationMatrix;

layout (std430, binding = 0) buffer MMBuffer
{
  mat4 ModelMatrixes[]; 
}; 
layout (std430, binding = 1) buffer RMBuffer
{
  mat4 RotationMatrixes[]; 
}; 
layout (std430, binding = 1) buffer IDMBuffer
{
  vec4 InstancedIds[]; 
}; 


uniform float RandomSeed1;
uniform float RandomSeed2;
uniform float RandomSeed3;
uniform float RandomSeed4;
uniform float RandomSeed5;
uniform float RandomSeed6;
uniform float RandomSeed7;
uniform float RandomSeed8;
uniform float RandomSeed9;
uniform float RandomSeed10;
uniform vec3 CameraPosition;
uniform vec3 CameraDirection;
uniform vec3 CameraTangentUp;
uniform vec3 CameraTangentLeft;
uniform float Time;
uniform int FrameINT;
uniform int Selected;

uniform vec3 ColoredID;

uniform float DiffuseComponent;
uniform float SpecularComponent;
uniform float Roughness;
uniform float Metalness;
uniform float ReflectionStrength;
uniform float RefractionStrength;
uniform vec2 resolution;
float ratio = resolution.y/resolution.x;


uniform int IgnoreLighting;

// settings
uniform float HBAOContribution;
uniform float GIContribution;
uniform float MainLightAttentuation;
uniform float SimpleLightAttentuation;
uniform float FogContribution;
uniform float FogSamples;
uniform float GISamples;
uniform float HBAOSamples;
uniform float ShadowsBlur;
uniform float ShadowsSamples;
uniform float LightPointSize;
uniform float SimpleLightPointSize;
uniform float BloomSamples;
uniform float BloomSize;
uniform float BloomContribution;
uniform float GIDiffuseComponent;
uniform float HBAOStrength;