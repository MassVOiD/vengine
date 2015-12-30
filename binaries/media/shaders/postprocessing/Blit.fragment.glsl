#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

out vec4 outColor;

#define MODE_COLOR 0
#define MODE_DEPTH 1
#define MODE_COLOR_DEPTH 2
uniform int BlitMode;

void main()
{
	vec4 color1 = vec4(0);
	if(BlitMode == MODE_COLOR_DEPTH){
		color1 = texture(currentTex, UV).rgba;
		gl_FragDepth = texture(depthTex, UV).r;
	} else if(BlitMode == MODE_COLOR){
		color1 = texture(currentTex, UV).rgba;
	} else if(BlitMode == MODE_DEPTH){
		color1 = vec4(reverseLog(texture(depthTex, UV).r));
	}
    outColor = color1;
}