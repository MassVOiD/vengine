#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include FXAA.glsl
#include Shade.glsl
#include AmbientOcclusion.glsl

#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;

void main()
{/*
    if(textureMSAA(diffuseColorTex, UV, 0).r >= 999){ 
		outColor = vec4(vec3(1), 1);
		return;
    }*/
    vec3 position = FromCameraSpace(reconstructCameraSpace(UV, 0));
    vec3 normal = textureMSAA(normalsTex, UV, 0).rgb;
	normal += step(0, -length(normal));
    float roughness = textureMSAA(diffuseColorTex, UV, 0).a;
    float metalness =  textureMSAA(normalsTex, UV, 0).a;

	float ao = AmbientOcclusion(position, normalize(normal), roughness, metalness);

    outColor = vec4(normal, ao);
}
