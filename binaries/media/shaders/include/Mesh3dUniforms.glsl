uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform float FarPlane;

const int MAX_LIGHTS = 6;
uniform int LightsCount;
uniform mat4 LightsPs[MAX_LIGHTS];
uniform mat4 LightsVs[MAX_LIGHTS];
uniform vec3 LightsPos[MAX_LIGHTS];
uniform float LightsFarPlane[MAX_LIGHTS];
uniform vec4 LightsColors[MAX_LIGHTS];


uniform int Instances;
uniform mat4 InitialTransformation;
uniform mat4 InitialRotation;

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
uniform vec3 CameraTangentUp;
uniform vec3 CameraTangentLeft;
uniform float Time;
uniform int FrameINT;

uniform float DiffuseComponent;
uniform float SpecularComponent;
uniform float Roughness;
uniform float Metalness;
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