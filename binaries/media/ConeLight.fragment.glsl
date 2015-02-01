#version 430 core

//in vec3 normal;
in vec4 vertexPosition;

uniform vec3 LightPosition;

out vec4 outColor;

void main()
{
	vec3 d1 = vertexPosition.xyz;
	vec3 d2 = LightPosition;
	gl_FragDepth = length(vertexPosition.xyz - LightPosition) / 4120.0f;
    outColor = vec4(0,0,0,0);
}