#version 430 core
layout(binding = 0) uniform sampler2D tex;
#include Fragment.glsl
void main()
{
	finishFragment(texture(tex, UV));
}