uniform vec4 input_Color;
layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outNormals;

//layout (binding = 22, r32ui) coherent uniform uimage3D full3dScene;

#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

#include ParallaxOcclusion.glsl

bool markAsParallax = false;

uniform int MaterialType;
#define MaterialTypeSolid 0
#define MaterialTypeRandomlyDisplaced 1
#define MaterialTypeWater 2
#define MaterialTypeSky 3
#define MaterialTypeWetDrops 4
#define MaterialTypeRainsDropSystem 12
#define MaterialTypeRainsOptimizedSphere 13

uniform int DropsCount;
uniform float DropsMaxRadius;
uniform float DropsStrength;
layout (std430, binding = 4) buffer DropsBuffer
{
    vec4 Drops[];
}; 

vec3 determineWave(vec3 w, vec3 n){
    vec3 newn = n;
    for(int i=0;i<DropsCount;i++){
        vec3 dpos = Drops[i].xyz;
        float dradius = Drops[i].a;
        float dt = abs(dot(n, normalize(dpos - w)));
        float a = abs(distance(dpos, w) - dradius);
        float frad = mix(1.0, 0.0, min(a * 10.0f, 1.0));
        float dff = 1.0 - (distance(dpos, w) / DropsMaxRadius);
        float si = sign((distance(dpos, w) - dradius));
        newn = normalize(newn + (dff * frad * si * normalize(w - dpos) * 1.2f));
    }
    return newn;
}


uniform float NormalMapScale;


uniform int UseSpecularMap;
uniform int UseRoughnessMap;
uniform int UseMetalnessMap;
uniform int InvertNormalMap;


uniform int ShadingMode;
#define SHADING_MDOE_RAWROUGH_RAWMETAL 0
#define SHADING_MDOE_RAWROUGH_ 1
#define SHADING_MDOE_SPEC 1

float makeSphereHeight(vec2 uvxs) {
	float r = length(uvxs);
	if(r >= 1.0) discard;
	return sqrt(1.0 - r*r);
}

vec3 makeSphereNormals(vec2 uv, float r) {
	vec3 d = normalize(vec3(uv, r));
	return d;
}

vec2 getTexel(sampler2D t){
	return 1.0 / vec2(textureSize(t, 0));
}

vec3 examineBumpMap(){
	vec2 iuv = Input.TexCoord;
	float bc = texture(bumpMapTex, iuv, 0).r;
	vec2 dsp = getTexel(bumpMapTex);
	float bdx = texture(bumpMapTex, iuv).r - texture(bumpMapTex, iuv+vec2(dsp.x, 0)).r;
	float bdy = texture(bumpMapTex, iuv).r - texture(bumpMapTex, iuv+vec2(0, dsp.y)).r;

	vec3 tang = normalize(Input.Tangent.xyz)*6;
	vec3 bitan = normalize(cross(Input.Tangent.xyz, Input.Normal))*6 * Input.Tangent.w;;

	return normalize(vec3(0,0,1) + bdx * tang + bdy * bitan);
}

void finishFragment(vec4 incolor, vec2 UV){
	if(incolor.a < 0.01) discard;
	
    vec4 color = incolor;
    vec3 wpos = Input.WorldPos;
	
    vec3 normalNew  = normalize(Input.Normal);
    
    mat3 TBN = inverse(transpose(mat3(
        normalize(Input.Tangent.xyz),
        normalize(cross(Input.Normal, (Input.Tangent.xyz))) * Input.Tangent.w,
        normalize(Input.Normal)
    )));   
	
	//vec2 UV = Input.TexCoord;
	if(UseBumpMap == 1){  
		//UV = adjustParallaxUV();
		wpos -= (RotationMatrixes[Input.instanceId] * vec4(normalNew, 0)).xyz * (1.0 - texture(bumpMapTex, UV).r) * parallaxScale;
	} 	
    
    if(MaterialType == MaterialTypeRainsOptimizedSphere){
        vec2 uvs = UV * 2.0 - 1.0;
        if(dot(normalNew, CameraPosition -Input.WorldPos) <=0) 
            uvs *= -1;
        float h = makeSphereHeight(uvs);
        normalNew = TBN * makeSphereNormals(uvs, h);
        
        if(dot(normalNew, CameraPosition -Input.WorldPos) <=0) {
            normalNew *= -1;
        }
        wpos += normalNew * h;
    }
	if(UseNormalMap == 1){  
		vec3 map = texture(normalMapTex, UV ).rgb;
		map = map * 2 - 1;
		if(InvertNormalMap == 1){
		//	map.r = - map.g;
		} else {
		//	map.g = - map.r;
		}
		map.r = - map.r;
		map.g = - map.g;
		normalNew = TBN * mix(vec3(0, 0, 1), map, 1); 
	} 
	if(UseNormalMap == 0 && UseBumpMap == 1){
		normalNew = TBN * examineBumpMap();
	}

#define MaterialTypeParallax 11
	if(MaterialType == MaterialTypeParallax){
		float factor = ( 1.0 - texture(bumpMapTex, UV).r);
		if(Input.Data.x < 0.99){
			if(factor > Input.Data.x) discard;
		}
	}
	
	vec3 rn = (RotationMatrixes[Input.instanceId] * vec4(normalNew, 0)).xyz;
	uint id = InstancedIds[Input.instanceId];
	
	
	if(MaterialType == MaterialTypeRainsDropSystem) rn = determineWave(wpos, rn); 

    float outRoughness = clamp(Roughness, 0.0, 1.0);
    float outMetalness = 0;
    float outSpecular = 0;
    
	if(UseRoughnessMap == 1) outRoughness = texture(roughnessMapTex, UV).r; 
    else outRoughness = clamp(Roughness, 0.0, 1.0);
    
	if(UseMetalnessMap == 1) outMetalness = texture(metalnessMapTex, UV).r; 
    else outMetalness = Metalness;
	//if(UseSpecularMap == 1) outSpecular = texture(specularMapTex, UV).r; 
  //  else outSpecular = SpecularComponent;
    
	outColor = vec4((color.xyz), clamp(outRoughness, 0.0, 1.0));
	outNormals = vec4(rn, outMetalness);
	updateDepthFromWorldPos(wpos);
}