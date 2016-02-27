layout(location = 0) out vec4 outAlbedoRoughness;
layout(location = 1) out vec4 outNormalsDistance;
layout(location = 2) out vec4 outSpecularBump;


//vec2 UV = gl_FragCoord.xy / textureSize(distanceTex, 0);

uniform int UseVDAO;
uniform int UseHBAO;
uniform int UseFog;
uniform int DisablePostEffects;
uniform float VDAOGlobalMultiplier;

#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

//layout (binding = 22, r32ui) coherent uniform uimage3D full3dScene;



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

FragmentData currentFragment;

vec2 UVX = gl_FragCoord.xy / textureSize(distanceTex, 0);
float AOValue = 1;
/*
include Shade.glsl
include EnvironmentLight.glsl
include Direct.glsl
include AmbientOcclusion.glsl

vec3 ApplyLighting(FragmentData data){
	if(UseHBAO == 1) AOValue = AmbientOcclusion(data);
	vec3 directlight = DirectLight(data);
	vec3 envlight = UseVDAO == 1 ? (VDAOGlobalMultiplier * EnvironmentLight(data)) : vec3(0);

	directlight += envlight * AOValue;
	
	if(data.diffuseColor.x > 1.0 && data.diffuseColor.y > 1.0 && data.diffuseColor.z > 1.0) directlight = (data.diffuseColor - 1.0) * AOValue;
	return directlight;
}

uniform int GIContainer;*/

void main(){
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
		1.0,
		Roughness,
		0.0
	);	
	
	vec2 UV = Input.TexCoord;
	if(UseBumpTex) {
        UV = adjustParallaxUV();
    }
	
	mat3 TBN = mat3(
		normalize(Input.Tangent.xyz),
		normalize(cross(Input.Normal, (Input.Tangent.xyz))) * Input.Tangent.w,
		normalize(Input.Normal)
	);   
	
	if(UseNormalsTex){  
		vec3 map = texture(normalsTex, UV ).rgb;
		map = map * 2 - 1;

		map.r = - map.r;
		map.g = - map.g;
		
		currentFragment.normal = TBN * map;
	} 
	if(!UseNormalsTex && UseBumpTex){
		currentFragment.normal = TBN * examineBumpMap();
	}
	if(UseRoughnessTex) currentFragment.roughness = max(0.07, texture(roughnessTex, UV).r);
	if(UseAlphaTex) currentFragment.alpha = texture(alphaTex, UV).r; 
	if(UseDiffuseTex) currentFragment.diffuseColor = texture(diffuseTex, UV).rgb; 
	//if(UseDiffuseTex && !UseAlphaTex)currentFragment.alpha = texture(diffuseTex, UV).r; 
	if(UseSpecularTex) currentFragment.specularColor = texture(specularTex, UV).rgb; 
	if(UseBumpTex) currentFragment.bump = texture(bumpTex, UV).r; 
	if(currentFragment.alpha < 0.01) discard;
	
	currentFragment.normal = (RotationMatrixes[Input.instanceId] * vec4(currentFragment.normal, 0)).xyz;
	
	//currentFragment.diffuseColor = mix(vec3(1), vec3(1,0,0), (Input.Data.x-1.0) / 31.0);
//currentFragment.roughness = 0;
	//vec3 resultforward = vec3(0);
   // if(texture(distanceTex, UVX).r < 0.01)resultforward.rgb = vec3(1);
	//else 
	//resultforward = ApplyLighting(currentFragment);
	
	//resultforward += traceRay(Input.WorldPos, normalize(reflect(normalize(currentFragment.cameraPos), currentFragment.normal)));

	
	//outColor = vec4(resultforward, currentFragment.alpha);
	
	outAlbedoRoughness = vec4(currentFragment.diffuseColor, currentFragment.roughness);
	outNormalsDistance = vec4(currentFragment.normal, currentFragment.cameraDistance);
	outSpecularBump = vec4(currentFragment.specularColor, currentFragment.bump);
}