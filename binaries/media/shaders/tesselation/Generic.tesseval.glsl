#version 430 core

layout(triangles, equal_spacing, ccw) in;

#include Mesh3dUniforms.glsl

in vec3 ModelPos_ES_in[];
in vec3 WorldPos_ES_in[];
in vec2 TexCoord_ES_in[];
in vec3 Normal_ES_in[];
in vec3 Barycentric_ES_in[];
in int instanceId_ES_in[];

uniform int UseBumpMap;
layout(binding = 16) uniform sampler2D bumpMap;

smooth out vec3 normal;
smooth out vec3 positionWorldSpace;
smooth out vec3 positionModelSpace;
smooth out vec2 UV;
smooth out vec3 barycentric;
out int instanceId;

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y )* v1 + vec3(gl_TessCoord.z) * v2;
}

void main()
{
   	// Interpolate the attributes of the output vertex using the barycentric coordinates
   	UV = interpolate2D(TexCoord_ES_in[0], TexCoord_ES_in[1], TexCoord_ES_in[2]);
   	barycentric = interpolate3D(Barycentric_ES_in[0], Barycentric_ES_in[1], Barycentric_ES_in[2]);
   	normal = interpolate3D(Normal_ES_in[0], Normal_ES_in[1], Normal_ES_in[2]);
   	positionWorldSpace = interpolate3D(WorldPos_ES_in[0], WorldPos_ES_in[1], WorldPos_ES_in[2]);
   	positionModelSpace = interpolate3D(ModelPos_ES_in[0], ModelPos_ES_in[1], ModelPos_ES_in[2]);
	   	// Displace the vertex along the normal
	instanceId = instanceId_ES_in[0];
	normal = normalize(normal);
    
    //if(UseBumpMap == 1){
    //    positionWorldSpace += normal * texture(bumpMap, UV).r;
    //}
	
   	gl_Position = ProjectionMatrix * ViewMatrix * vec4(positionWorldSpace, 1.0);
}
/**/