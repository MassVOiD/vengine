#version 430 core

in vec2 UV;
#include LightingSamplers.glsl

out vec4 outColor;



void main()
{
	vec4 color1 = texture(currentTex, UV).rgba;
	gl_FragDepth = texture(depthTex, UV).r;
    outColor = color1;
}