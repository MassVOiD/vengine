#version 430 core
//in vec3 normal;
smooth in vec3 vertexWorldSpace;
uniform vec3 LightPosition;
#include LogDepth.glsl

layout(binding = 2) uniform sampler2D AlphaMask;
uniform int UseAlphaMask;
in vec2 UV;
void discardIfAlphaMasked(){
	if(UseAlphaMask == 1){
		if(texture(AlphaMask, UV).r < 0.5) discard;
	}
}

out float outColor;	

void main()
{
	discardIfAlphaMasked();
	float depth = distance(vertexWorldSpace, LightPosition);
	gl_FragDepth = toLogDepth(depth);
		
    //outColor = 0;
}