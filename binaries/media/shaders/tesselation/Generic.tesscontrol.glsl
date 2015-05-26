#version 430 core
#include Mesh3dUniforms.glsl

// define the number of CPs in the output patch
layout (vertices = 3) out;

uniform vec3 gEyeWorldPos;

// attributes of the input CPs
in int instanceId_CS_in[];
in vec3 ModelPos_CS_in[];
in vec3 WorldPos_CS_in[];
in vec2 TexCoord_CS_in[];
in vec3 Normal_CS_in[];
in vec3 Barycentric_CS_in[];
in vec3 Tangent_CS_in[];

// attributes of the output CPs
out int instanceId_ES_in[];
out vec3 ModelPos_ES_in[];
out vec3 WorldPos_ES_in[];
out vec2 TexCoord_ES_in[];
out vec3 Normal_ES_in[];
out vec3 Barycentric_ES_in[];
out vec3 Tangent_ES_in[];

float GetTessLevel(float Distance0, float Distance1)
{
	float rd = ((Distance0 +Distance1)/2);
    if(rd < 150.0) 
        return 33.0;
	else if(rd < 280.0) 
        return 8.0;
    return 4.0;
}

uniform float TesselationMultiplier;

void main()
{
    // Set the control points of the output patch
    TexCoord_ES_in[gl_InvocationID] = TexCoord_CS_in[gl_InvocationID];
    Normal_ES_in[gl_InvocationID] = Normal_CS_in[gl_InvocationID];
    WorldPos_ES_in[gl_InvocationID] = WorldPos_CS_in[gl_InvocationID];
    ModelPos_ES_in[gl_InvocationID] = ModelPos_CS_in[gl_InvocationID];
    Barycentric_ES_in[gl_InvocationID] = Barycentric_CS_in[gl_InvocationID];
    Tangent_ES_in[gl_InvocationID] = Tangent_CS_in[gl_InvocationID];
    instanceId_ES_in[gl_InvocationID] = instanceId_CS_in[gl_InvocationID];
   	//Barycentric_ES_in = Barycentric_ES_in[0];
	
	// Calculate the distance from the camera to the three control points
    float EyeToVertexDistance0 = distance(CameraPosition, WorldPos_CS_in[0]);
    float EyeToVertexDistance1 = distance(CameraPosition, WorldPos_CS_in[1]);
    float EyeToVertexDistance2 = distance(CameraPosition, WorldPos_CS_in[2]);

    // Calculate the tessellation levels
    gl_TessLevelOuter[0] = GetTessLevel(EyeToVertexDistance1, EyeToVertexDistance2) * TesselationMultiplier;
    gl_TessLevelOuter[1] = GetTessLevel(EyeToVertexDistance2, EyeToVertexDistance0) * TesselationMultiplier;
    gl_TessLevelOuter[2] = GetTessLevel(EyeToVertexDistance0, EyeToVertexDistance1) * TesselationMultiplier;
    gl_TessLevelInner[0] = gl_TessLevelOuter[2];
    gl_TessLevelInner[1] = gl_TessLevelOuter[2];
}