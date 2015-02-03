#version 430 core

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;

uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform vec3 LightPosition;

//out vec3 normal;
out vec4 vertexPosition;

void main(){

    vec4 v = vec4(in_position,1);
    //vec4 n = vec4(in_normal,0);
	mat4 mvp = ProjectionMatrix * ViewMatrix * ModelMatrix;
	//normal = (ProjectionMatrix * ModelMatrix * n).xyz;
	vertexPosition = ModelMatrix * v;
	gl_Position = mvp * v;
	//gl_Position.z = pow(gl_Position.z, 99);
}