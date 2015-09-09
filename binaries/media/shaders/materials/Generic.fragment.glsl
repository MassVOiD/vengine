#version 430 core
#include Fragment.glsl
uniform int DrawMode;
#define MODE_TEXTURE_ONLY 0
#define MODE_COLOR_ONLY 1
#define MODE_TEXTURE_MULT_COLOR 2
#define MODE_ONE_MINUS_COLOR_OVER_TEXTURE 3

void main()
{
	discardIfAlphaMasked();
    if(DrawMode == MODE_TEXTURE_ONLY) finishFragment(texture(currentTex, UV));
	else if(DrawMode == MODE_COLOR_ONLY) finishFragment(input_Color);
	else if(DrawMode == MODE_TEXTURE_MULT_COLOR) finishFragment(texture(currentTex, UV) * input_Color);
	else if(DrawMode == MODE_ONE_MINUS_COLOR_OVER_TEXTURE) 
        finishFragment(vec4(1) - (input_Color / (texture(currentTex, UV) + vec4(1, 1, 1, 0))));
}