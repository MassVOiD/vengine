uniform mat4 ModelMatrix;
uniform mat4 RotationMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform float LogEnchacer;
uniform float FarPlane;

const int MAX_LIGHTS = 28;
uniform int LightsCount;
uniform mat4 LightsPs[MAX_LIGHTS];
uniform mat4 LightsVs[MAX_LIGHTS];
uniform vec3 LightsPos[MAX_LIGHTS];
uniform float LightsFarPlane[MAX_LIGHTS];
uniform vec4 LightsColors[MAX_LIGHTS];


const int MAX_INSTANCES = 1024;
uniform int Instances;
uniform mat4 ModelMatrixes[MAX_INSTANCES];
uniform mat4 RotationMatrixes[MAX_INSTANCES];

uniform float RandomSeed;
uniform vec3 CameraPosition;
uniform float Time;


uniform float DiffuseComponent;
uniform float SpecularComponent;
uniform float SpecularSize;