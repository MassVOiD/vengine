#version 430 core
//in vec3 normal;
in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D Tex;
smooth in vec3 vertexWorldSpace;
uniform vec3 LightPosition;
uniform vec4 input_Color;
uniform vec4 LightColor;
uniform int DrawMode;
#define MODE_TEXTURE_ONLY 0
#define MODE_COLOR_ONLY 1
#define MODE_TEXTURE_MULT_COLOR 2
#define MODE_ONE_MINUS_COLOR_OVER_TEXTURE 3

//uniform int Instances;

layout(binding = 2) uniform sampler2D AlphaMask;
uniform int UseAlphaMask;
layout(binding = 31) uniform sampler2D bumpMap;
void discardIfAlphaMasked(){
	if(UseAlphaMask == 1){
		if(texture(AlphaMask, UV).r < 0.5) discard;
	}
}

out vec4 outColor;	

void finishFragment(vec4 c){
    vec3 cc = mix(LightColor.rgb*c.rgb, LightColor.rgb, Metalness);
    vec3 difcolor = cc;
    vec3 difcolor2 = LightColor.rgb*c.rgb;
    
    vec3 radiance = mix(difcolor2, difcolor*Roughness, Metalness);
    outColor = vec4(radiance, dot(normalize(normal.xyz), normalize(CameraPosition - vertexWorldSpace)));
}

void main()
{
	discardIfAlphaMasked();
    vec3 wpos = vertexWorldSpace;	
	vec3 outNormals = vec3(0);
    if(Instances == 0){
        outNormals = (RotationMatrix * vec4(normal, 0)).xyz;
    } else {
        outNormals = (RotationMatrixes[instanceId] * vec4(normal, 0)).xyz;
    }
    if(UseBumpMap == 1){
       // float factor = (texture(bumpMap, UV).r - 0.5);
      //  wpos += normalize(outNormals) * factor;
//
    }    
	float depth = distance(wpos, LightPosition);
    if(DrawMode == MODE_TEXTURE_ONLY) finishFragment(texture(Tex, UV));
	else if(DrawMode == MODE_COLOR_ONLY) finishFragment(input_Color);
	else if(DrawMode == MODE_TEXTURE_MULT_COLOR) finishFragment(texture(Tex, UV) * input_Color);
	else if(DrawMode == MODE_ONE_MINUS_COLOR_OVER_TEXTURE) 
        finishFragment(vec4(1) - (input_Color / (texture(Tex, UV) + vec4(1, 1, 1, 0))));
	gl_FragDepth = toLogDepth(depth);
}