
out vec4 outColor;

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

uniform float NormalMapScale;

uniform int InvertNormalMap;

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
	float bc = texture(bumpTex, iuv, 0).r;
	vec2 dsp = getTexel(bumpTex);
	float bdx = texture(bumpTex, iuv).r - texture(bumpTex, iuv+vec2(dsp.x, 0)).r;
	float bdy = texture(bumpTex, iuv).r - texture(bumpTex, iuv+vec2(0, dsp.y)).r;

	vec3 tang = normalize(Input.Tangent.xyz)*6;
	vec3 bitan = normalize(cross(Input.Tangent.xyz, Input.Normal))*6 * Input.Tangent.w;;

	return normalize(vec3(0,0,1) + bdx * tang + bdy * bitan);
}

struct FragmentData
{
	vec3 diffuseColor;
	vec3 specularColor;
	vec3 normal;
	vec3 tangent;
	vec3 worldPos;
	vec3 cameraPos;
	float cameraDistance;
	float alpha;
	float roughness;
	float bump;
};

FragmentData currentFragment;

uniform int UseVDAO;
uniform int UseHBAO;
uniform int UseFog;
uniform int DisablePostEffects;
uniform float VDAOGlobalMultiplier;
vec2 UV = gl_FragCoord.xy / textureSize(distanceTex, 0);
vec2 UVX = gl_FragCoord.xy / textureSize(distanceTex, 0);
float AOValue;
#include Shade.glsl
#include EnvironmentLight.glsl
#include Direct.glsl
#include AmbientOcclusion.glsl

vec3 ApplyLighting(FragmentData data){
	if(UseHBAO == 1) AOValue = AmbientOcclusion(data);
	vec3 directlight = DirectLight(data);
	vec3 envlight = UseVDAO == 1 ? (VDAOGlobalMultiplier * EnvironmentLight(data)) : vec3(0);

	
	if(UseVDAO == 1 && UseHBAO == 0) directlight += envlight;
	if(UseHBAO == 1 && UseVDAO == 1) directlight += envlight * AOValue;
	if(UseHBAO == 1 && UseVDAO == 0) directlight += AOValue;
	//directlight += data.diffuseColor * (UseHBAO == 1 ? AOValue : 1.0) * 0.01;
	
	return directlight;
}


void main(){
	// if(diffuse.a < 0.01) discard;
	//outColor = vec4(1.0);
	//return;
	currentFragment = FragmentData(
		DiffuseColor,
		SpecularColor,
		normalize(Input.Normal),
		normalize(Input.Tangent.xyz),
		Input.WorldPos,
		ToCameraSpace(Input.WorldPos),
		distance(CameraPosition, Input.WorldPos),
		Alpha,
		Roughness,
		0.0
	);	
	
	vec2 UV = Input.TexCoord;
	if(UseBumpTex == 1) {
        UV = adjustParallaxUV();
    }
	
	mat3 TBN = inverse(transpose(mat3(
		normalize(Input.Tangent.xyz),
		normalize(cross(Input.Normal, (Input.Tangent.xyz))) * Input.Tangent.w,
		normalize(Input.Normal)
	)));   
	
	if(UseNormalsTex == 1){  
		vec3 map = texture(normalsTex, UV * NormalMapScale ).rgb;
		map = map * 2 - 1;
		if(InvertNormalMap == 1){
			    map.r = - map.r;
			    map.g = - map.g;
		} else {
			//    map.g = - map.r;
		}
		map.r = - map.r;
		map.g = - map.g;
		currentFragment.normal = TBN * map;
	} 
	if(UseNormalsTex == 0 && UseBumpTex == 1){
		currentFragment.normal = TBN * examineBumpMap();
	}
	if(UseRoughnessTex == 1) currentFragment.roughness = max(0.07, texture(roughnessTex, UV).r);
	if(UseAlphaTex == 1) currentFragment.alpha = texture(alphaTex, UV).r; 
	if(UseDiffuseTex == 1) currentFragment.diffuseColor = texture(diffuseTex, UV).rgb; 
	if(UseSpecularTex == 1) currentFragment.specularColor = texture(specularTex, UV).rgb; 
	
	currentFragment.normal = (RotationMatrixes[Input.instanceId] * vec4(currentFragment.normal, 0)).xyz;
//currentFragment.roughness = 0;
	vec3 resultforward = vec3(0);
   // if(texture(distanceTex, UVX).r < 0.01)resultforward.rgb = vec3(1);
	//else 
	resultforward = ApplyLighting(currentFragment);
	outColor = vec4(resultforward, currentFragment.alpha);
}