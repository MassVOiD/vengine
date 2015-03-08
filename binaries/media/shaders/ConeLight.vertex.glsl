#version 430 core

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;

#include Mesh3dUniforms.glsl
uniform vec3 LightPosition;

//out vec3 normal;
smooth out vec3 vertexWorldSpace;

void main(){
	vec4 v = vec4(in_position,1);

	mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrixes[gl_InstanceID];
	vertexWorldSpace = (ModelMatrixes[gl_InstanceID] * v).xyz;
	gl_Position = mvp * v;

}