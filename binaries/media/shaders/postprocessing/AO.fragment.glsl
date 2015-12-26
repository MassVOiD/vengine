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
{
    if(texture(diffuseColorTex, UV).r >= 999){ 
		outColor = vec4(vec3(1), 1);
		return;
    }
    vec3 position = FromCameraSpace(reconstructCameraSpace(UV));
    vec3 normal = normalize(texture(normalsTex, UV).rgb);
    float roughness = texture(diffuseColorTex, UV).a;
    float metalness =  texture(normalsTex, UV).a;

	float ao = AmbientOcclusion(position, normal, roughness, fract(metalness));

    outColor = vec4(normal, ao);
}
