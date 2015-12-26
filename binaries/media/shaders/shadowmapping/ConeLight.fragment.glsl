#version 430 core
//in vec3 normal;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include ParallaxOcclusion.glsl
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

void main()
{
    vec3 wpos = Input.WorldPos;	
	//if(UseBumpMap == 1){
	//	vec2 UV = adjustParallaxUV();
	//	wpos -= (RotationMatrixes[Input.instanceId] * vec4(normalize(Input.Normal), 0)).xyz * (1.0 - texture(bumpMapTex, UV).r) * parallaxScale;
	//}
	//if(DrawMode == MODE_TEXTURE_ONLY && texture(currentTex, Input.TexCoord).a < 0.01) discard;
	float depth = distance(wpos, LightPosition);
	gl_FragDepth = toLogDepth(depth);
}