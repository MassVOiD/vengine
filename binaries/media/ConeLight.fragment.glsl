#version 430 core

//in vec3 normal;
in vec4 vertexPosition;

uniform vec3 LightPosition;

out vec4 outColor;

void main()
{
	vec3 d1 = vertexPosition.xyz;
	vec3 d2 = LightPosition;
	//gl_FragDepth = length(vertexPosition.xyz - LightPosition);
    outColor = vec4(d2.x, d2.y, d2.z, length(vertexPosition.xyz - LightPosition) * 10.0f);
}