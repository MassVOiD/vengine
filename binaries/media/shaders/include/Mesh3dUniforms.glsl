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


uniform int Instances;
uniform mat4 ModelMatrix;
uniform mat4 RotationMatrix;
uniform mat4 InitialTransformation;
uniform mat4 InitialRotation;
uniform mat4 CameraTransformation;

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

uniform uint MeshID;

uniform float DiffuseComponent;
uniform float SpecularComponent;
uniform float Roughness;
uniform float Metalness;
uniform float ReflectionStrength;
uniform float RefractionStrength;
uniform vec2 resolution;
float ratio = resolution.y/resolution.x;


uniform int IgnoreLighting;

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