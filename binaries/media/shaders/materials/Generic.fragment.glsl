#version 430 core
#include Fragment.glsl
layout(binding = 0) uniform sampler2D Tex;
uniform int DrawMode;
#define MODE_TEXTURE_ONLY 0
#define MODE_COLOR_ONLY 1
#define MODE_TEXTURE_MULT_COLOR 2
#define MODE_ONE_MINUS_COLOR_OVER_TEXTURE 3

void main()
{
	discardIfAlphaMasked();
    if(DrawMode == MODE_TEXTURE_ONLY) finishFragment(texture(Tex, UV));
	else if(DrawMode == MODE_COLOR_ONLY) finishFragment(input_Color);
	else if(DrawMode == MODE_TEXTURE_MULT_COLOR) finishFragment(texture(Tex, UV) * input_Color);
	else if(DrawMode == MODE_ONE_MINUS_COLOR_OVER_TEXTURE) 
        finishFragment(vec4(1) - (input_Color / (texture(Tex, UV) + vec4(1, 1, 1, 0))));
}