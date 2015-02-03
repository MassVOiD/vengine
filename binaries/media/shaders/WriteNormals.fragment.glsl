#version 430 core

in vec3 normal;
in vec4 vertexPosition;

out vec4 outColor;

void main()
{
    outColor = vec4(vertexPosition);
}