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

#define MaterialTypeRainsOptimizedSphere 13
out uvec4 outColor;	
void discardSphereBounds(vec2 uvxs) {
	float r = length(uvxs);
	if(r >= 1.0) discard;
}
    
void finishFragment(vec4 c){   
	if(c.a < 0.01) discard; 
    if(MaterialType == MaterialTypeRainsOptimizedSphere){
        vec2 uvs = Input.TexCoord * 2.0 - 1.0;
        discardSphereBounds(uvs);
    }
    float outRoughness = 0;
    float outMetalness = 0;
    if(UseRoughnessMap == 1) outRoughness = texture(roughnessMapTex, Input.TexCoord).r; 
    else outRoughness = Roughness;
    if(UseMetalnessMap == 1) outMetalness = texture(metalnessMapTex, Input.TexCoord).r; 
    else outMetalness = Metalness;
    float outSpecular = 0;
    if(UseSpecularMap == 1) outSpecular = texture(specularMapTex, Input.TexCoord).r; 
    else outSpecular = SpecularComponent;
    vec3 cc = mix(LightColor.rgb*c.rgb, LightColor.rgb, outMetalness);
    vec3 difcolor = cc;
    vec3 difcolor2 = LightColor.rgb*c.rgb;
    vec3 rn = (InitialRotation * RotationMatrixes[int(Input.instanceId)] * vec4(Input.Normal, 0)).xyz;
    
    
    //vec3 radiance = mix(difcolor2, difcolor*outRoughness, outMetalness);
    outColor = uvec4(packUnorm4x8(vec4(c.xyz, outRoughness)), packSnorm4x8(vec4(rn, outMetalness)), 0,0);
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