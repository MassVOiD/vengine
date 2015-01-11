#version 330 core

layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 normal;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform float realTime;

out vec2 UV;
out float time;
out vec4 colorint;

void main(){

    vec4 v = vec4(vertexPosition_modelspace,1);
	mat4 mvp = projectionMatrix * viewMatrix * modelMatrix;
    gl_Position = mvp * v;
	time = realTime;
    UV = -texcoord;
}