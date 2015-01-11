#version 330 core
in vec2 UV;
in float time;
out vec4 outColor;
uniform sampler2D tex;
void main()
{
    outColor = vec4(1.0, 0.0, 1.0, 1.0);
}