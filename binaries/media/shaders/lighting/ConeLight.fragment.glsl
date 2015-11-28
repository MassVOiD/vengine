#version 430 core
//in vec3 normal;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
uniform vec3 LightPosition;
uniform vec4 input_Color;
uniform vec4 LightColor;
uniform int DrawMode;
uniform int MaterialType;
#define MODE_TEXTURE_ONLY 0
#define MODE_COLOR_ONLY 1
#define MODE_TEXTURE_MULT_COLOR 2
#define MODE_ONE_MINUS_COLOR_OVER_TEXTURE 3

//uniform int Instances;

uniform int UseAlphaMask;
uniform int UseRoughnessMap;
uniform int UseSpecularMap;
uniform int UseMetalnessMap;
/*vec3 shade(
    vec3 albedo, 
    vec3 normal,
    vec3 fragmentPosition, 
    vec3 lightPosition, 
    vec4 lightColor, 
    float roughness, 
    float metalness, 
    float specular,
    bool ignoreAtt
)*/
void discardIfAlphaMasked(){
	if(UseAlphaMask == 1){
		if(texture(alphaMaskTex, Input.TexCoord).r < 0.5) discard;
	}
    #define MaterialTypeParallax 11
        if(MaterialType == MaterialTypeParallax){
            float factor = ( 1.0 - texture(bumpMapTex, Input.TexCoord).r);
            //factor += 0.2 * rand2d(Input.TexCoord);
            if(Input.Data.z < 0.99){
                if(factor > Input.Data.x + 0.01) discard;
            }
        }        
}

void main()
{
	discardIfAlphaMasked();
    vec3 wpos = Input.WorldPos;	
	float depth = distance(wpos, LightPosition);
	gl_FragDepth = toLogDepth(depth);
}