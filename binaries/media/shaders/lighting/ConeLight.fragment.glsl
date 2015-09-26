#version 430 core
//in vec3 normal;
in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
uniform vec3 LightPosition;
uniform vec4 input_Color;
uniform vec4 LightColor;
uniform int DrawMode;
#define MODE_TEXTURE_ONLY 0
#define MODE_COLOR_ONLY 1
#define MODE_TEXTURE_MULT_COLOR 2
#define MODE_ONE_MINUS_COLOR_OVER_TEXTURE 3

//uniform int Instances;

uniform int UseAlphaMask;
void discardIfAlphaMasked(){
	if(UseAlphaMask == 1){
		if(texture(alphaMaskTex, Input.TexCoord).r < 0.5) discard;
	}
}

out uvec4 outColor;	

void finishFragment(vec4 c){
    vec3 cc = mix(LightColor.rgb*c.rgb, LightColor.rgb, Metalness);
    vec3 difcolor = cc;
    vec3 difcolor2 = LightColor.rgb*c.rgb;
    vec3 rn;
    if(Instances == 0){
        rn = (InitialRotation * RotationMatrix * vec4(Input.Normal, 0)).xyz;
    } else {
        rn = (InitialRotation * RotationMatrixes[Input.instanceId] * vec4(Input.Normal, 0)).xyz;
    }
    vec3 radiance = mix(difcolor2, difcolor*Roughness, Metalness);
    outColor = uvec4(packUnorm4x8(vec4(radiance, Roughness)), packSnorm4x8(vec4(rn, Metalness)), 0,0);
}

void main()
{
	discardIfAlphaMasked();
    vec3 wpos = Input.WorldPos;	

	float depth = distance(wpos, LightPosition);
    if(DrawMode == MODE_TEXTURE_ONLY) finishFragment(texture(currentTex, Input.TexCoord));
	else if(DrawMode == MODE_COLOR_ONLY) finishFragment(input_Color);
	else if(DrawMode == MODE_TEXTURE_MULT_COLOR) finishFragment(texture(currentTex, Input.TexCoord) * input_Color);
	else if(DrawMode == MODE_ONE_MINUS_COLOR_OVER_TEXTURE) 
        finishFragment(vec4(1) - (input_Color / (texture(currentTex, Input.TexCoord) + vec4(1, 1, 1, 0))));
	gl_FragDepth = toLogDepth(depth);
}