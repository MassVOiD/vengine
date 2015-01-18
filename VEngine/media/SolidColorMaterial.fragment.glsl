#version 430 core

uniform vec4 input_Color;

in vec4 positionWorldSpace;
in vec3 positionModelSpace;
in vec3 normal;

out vec4 outColor;

float hash3(vec3 uv) {
	return fract(sin(uv.x * 15.78 + uv.y * 35.14 + uv.z * 26.1134) * 43758.23);
}

void main()
{
	float diffuse = clamp(dot(vec3(1.0, 1.0, 1.0), normalize(normal)), 0.0, 1.0);
    outColor = (input_Color) * (diffuse);
}