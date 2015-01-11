#version 430 core
layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_uv;
layout(location = 2) in vec3 in_normal;
out vec2 cubetexcoord;
uniform float Time;
out float time;
void main(void){

	cubetexcoord = (in_position.xy + 1.0) / 2.0;
    gl_Position =  vec4(in_position.xyz,1);
	time = Time;
	
}