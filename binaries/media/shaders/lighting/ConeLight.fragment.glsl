#version 430 core
//in vec3 normal;
smooth in vec3 vertexWorldSpace;
uniform vec3 LightPosition;
#include LogDepth.glsl

//uniform int Instances;

layout(binding = 2) uniform sampler2D AlphaMask;
uniform int UseAlphaMask;
layout(binding = 31) uniform sampler2D bumpMap;
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
    vec3 wpos = vertexWorldSpace;	
	vec3 outNormals = vec3(0);
    if(Instances == 1){
        outNormals = (RotationMatrix * vec4(normal, 0)).xyz;
    } else {
        outNormals = (RotationMatrixes[instanceId] * vec4(normal, 0)).xyz;
    }
    if(UseBumpMap == 1){
        float factor = (texture(bumpMap, UV).r - 0.5);
        wpos += normalize(outNormals) * factor;

    }    
	float depth = distance(wpos, LightPosition);
	gl_FragDepth = toLogDepth(depth);
    //outColor = 0;
}