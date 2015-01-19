#version 430 core
layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;
out vec2 cubetexcoord;
uniform float Time;
uniform mat4 ModelMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjectionMatrix;
uniform vec3 CameraPosition;
out float time;
void main(void){

	float ratio = 16.0 / 9.0;
	cubetexcoord = (vec2(in_position.x*ratio, in_position.y) + 1.0) / 2.0;
    gl_Position =  vec4(in_position.xyz,1);
	time = Time;
	
}