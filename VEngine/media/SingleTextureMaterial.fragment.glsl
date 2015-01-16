#version 430 core

uniform sampler2D tex;

in vec3 normal;
in vec2 UV;

out vec4 outColor;

void main()
{
    outColor = vec4(texture(tex, UV).xyz, 1.0);
}