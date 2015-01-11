#version 430 core

uniform vec4 input_Color;

in vec3 normal;

out vec4 outColor;

void main()
{
	float diffuse = clamp(dot(vec3(1.0, 1.0, 1.0), normalize(normal)), 0.0, 1.0);
    outColor = input_Color * (diffuse);
}