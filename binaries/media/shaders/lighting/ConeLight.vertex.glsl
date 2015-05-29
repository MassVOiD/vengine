#version 430 core

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec3 in_tangent;


uniform vec3 LightPosition;

uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
const int MAX_INSTANCES = 2000;
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
smooth out vec2 UV;
#include Bones.glsl

//out vec3 normal;
smooth out vec3 vertexWorldSpace;

void main(){
	vec4 v = vec4(in_position,1);

	if(Instances == 1){
        vec3 mspace = v.xyz;
        if(UseBoneSystem == 1){
            int bone = determineBone(mspace);
            mspace = applyBoneRotationChain(mspace, bone);
            //inorm = applyBoneRotationChainNormal(inorm, bone);
        }
        v = vec4(mspace, 1);    
		mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrix;
		vertexWorldSpace = (ModelMatrix * v).xyz;
		gl_Position = mvp * v;
	} else {
		mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrixes[gl_InstanceID];
		vertexWorldSpace = (ModelMatrixes[gl_InstanceID] * v).xyz;
		gl_Position = mvp * v;
	}
	UV = vec2(in_uv.x, -in_uv.y);

}