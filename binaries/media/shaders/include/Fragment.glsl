in vec2 UV;
uniform vec4 input_Color;
out vec4 outColor;
#include Lighting.glsl
#include LogDepth.glsl

void finishFragment(vec4 color){
	outColor = vec4(processLighting(color.xyz), color.a);
	updateDepth();
}