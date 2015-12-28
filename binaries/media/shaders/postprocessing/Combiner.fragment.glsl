#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include FXAA.glsl
#include Shade.glsl
#include noise3D.glsl

#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;


float centerDepth;
uniform float Brightness;
uniform int UseFog;
uniform int UseLightPoints;
uniform int UseDepth;
uniform int UseDeferred;
uniform int UseHBAO;
uniform int UseVDAO;
uniform int UseRSM;
uniform int SimpleLightsCount;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

layout (std430, binding = 0) buffer RandomsBufferX
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand2s(seeder)) ;
}

float randsPointer = 0;
uniform int RandomsCount;
float getRand(){
    float r = rand(vec2(randsPointer, randsPointer*2.42354) + Time);
    randsPointer+=0.2;
    //if(randsPointer >= RandomsCount) randsPointer = 0;
    return r;
}

vec3 lookupFog(vec2 fuv){
    vec3 outc = vec3(0);
    int counter = 0;
    float depthCenter = texture(depthTex, fuv).r;
    for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
    {
        for(float g2 = 0; g2 < 6.0; g2+=1.0)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.001);
            vec3 color = texture(fogTex, fuv + gauss).rgb;
            float depthThere = texture(fogTex, fuv + gauss).a;
            if(abs(depthThere - depthCenter) < 0.01){
                outc += color;
                counter++;
            }
        }
    }
    return counter == 0 ? texture(fogTex, fuv).rgb : outc / counter;
}
vec3 random3dSample(){
    return normalize(vec3(
        getRand() * 2 - 1, 
        getRand() * 2 - 1, 
        getRand() * 2 - 1
    ));
}
// using this brdf makes cosine diffuse automatically correct
vec3 BRDF(vec3 reflectdir, vec3 norm, float roughness){
    vec3 displace = random3dSample();
    displace = displace * sign(dot(norm, displace));
    float dt = dot(displace, reflectdir) * 0.5 + 0.5;
    float mixfactor = mix(0, 1, roughness);
    
    return mix(displace, reflectdir, roughness);
}
mat3 TBN;
vec3 BiasedBRDF(vec3 reflectdir, vec3 norm, float roughness, vec2 uv){
    vec3 displace = TBN * hemisphereSample_cos(uv.x, uv.y);
   // displace = displace * sign(dot(norm, displace));
    float dt = dot(displace, reflectdir) * 0.5 + 0.5;
    float mixfactor = mix(0, 1, roughness);
    
    return mix(displace, reflectdir, roughness);
}

vec3 emulateSkyWithDepth(vec2 uv){
    vec3 worldPos = (reconstructCameraSpace(uv));
    float depth = length(worldPos)*0.001;
    worldPos = FromCameraSpace(worldPos);
    depth = depth * clamp(1.0 / (abs(worldPos.y) * 0.0001), 0.0, 1.0);
    return vec3(1) * depth;
}

vec3 ball(vec3 colour, float sizec, float xc, float yc){
	float xdist = (abs(UV.x - xc));
	float ydist = (abs(UV.y - yc)) * ratio;

	float d = sizec / length(vec2(xdist, ydist));
	return colour * (d);
}

vec3 lightPoints(){
    vec3 color = vec3(0);
	for(int i=0;i<LightsCount;i++){

		mat4 lightPV = (LightsPs[i] * LightsVs[i]);

		vec4 clipspace = (VPMatrix) * vec4((LightsPos[i]), 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;

        float badass_depth = distance(LightsPos[i], CameraPosition);
        float logg = length(reconstructCameraSpace(sspace1));
        float mixv = 1.0 - smoothstep(0.1, 2.5, distance(sspace1*resolution.xy * 0.01, UV*resolution.xy * 0.009));

        if(logg > badass_depth) {
            color += ball(vec3(LightsColors[i].rgb*1.0),LightPointSize / ( badass_depth) * 0.1, sspace1.x, sspace1.y);
            //color += ball(vec3(LightsColors[i]*2.0 * overall),12.0 / dist, sspace1.x, sspace1.y) * 0.03f;
        }

	}

    return color;
}


uniform float VDAOGlobalMultiplier;

float AOValue = 1.0;

#include EnvironmentLight.glsl
#include Direct.glsl

float getAO(vec2 uv, vec3 normal){
    float outc = 0.0;
    float counter = 0;
    float depthCenter = texture(depthTex, uv).r;
	float pixel = 1.0 / textureSize(aoTex, 0).y;
    for(float g = 0; g < mPI2 * 2; g+=0.5)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.2)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * g2 * 0.001 + pixel);
            vec3 n = texture(aoTex, uv + gauss).rgb;
            float ao = texture(aoTex, uv + gauss).a;
            //if(dot(n, normal) > 0.8){
			float force = pow(max(0.0, dot(n, normal)), 5);
			outc += ao * force;
			counter += force;
            //}
        }
    }
    return counter == 0 ? texture(aoTex, uv).a : outc / counter;
}


vec3 ApplyLighting(vec2 uv, vec3 albedo, vec3 position, vec3 normal, float roughness, float metalness, float IOR){
	if(UseHBAO == 1) AOValue = getAO(uv, normal);
	vec3 directlight = DirectLight(CameraPosition, albedo, normal, position, roughness, metalness);
	vec3 envlight = VDAOGlobalMultiplier * EnvironmentLight(albedo, position, normal, fract(metalness), roughness, IOR);

	
    if(UseVDAO == 1 && UseHBAO == 0) directlight += envlight;
    if(UseHBAO == 1 && UseVDAO == 1) directlight += envlight * AOValue;
	return directlight;
}

vec3 ApplyLighting(vec3 albedo, vec3 position, vec3 normal, float roughness, float metalness, float IOR){
	return ApplyLighting(gl_FragCoord.xy / resolution.xy, albedo, position, normal, roughness, metalness, IOR);
}

vec2 newUV = UV;
vec3 Lightning(){
    if(texture(diffuseColorTex, newUV).r >= 999){ 
		return texture(cubeMapTex, normalize(reconstructCameraSpace(newUV))).rgb;
    }
    vec3 albedo = texture(diffuseColorTex, newUV).rgb;
    vec3 position = FromCameraSpace(reconstructCameraSpace(newUV));
    vec3 normal = normalize(texture(normalsTex, newUV).rgb);
    float roughness = texture(diffuseColorTex, newUV).a;
    float metalness =  texture(normalsTex, newUV).a;
    float IOR =  0.0;
	
	

    return ApplyLighting(newUV, albedo, position, normal, roughness, metalness, IOR);
}

uniform int DisablePostEffects;

float sns(vec2 p, float scale, float tscale){
    return snoise(vec3(p.x*scale, p.y*scale, Time * tscale * 0.5));
}
float getwater( vec2 position ) {

    float color = 0.0;
   // color += sns(position + vec2(Time/3, Time/13), 0.1, 1.2) * 40;
   // color += sns(position, 0.1, 1.2) * 25;
    color += sns(position, 0.25, 2.)*3;
    color += sns(position, 0.38, 3.)*1;
    color += sns(position, 1., 2.)*0.9;
    //color += sns(position, 7., 6.)*0.3;
    color += sns(position, 7., 2.)*0.2;
    color += sns(position, 33., 2.) * 0.02;
    return color / 3.0;

}
vec3 getwatern( vec2 position ) {

    vec3 a = vec3(position, getwater(position));
	vec2 m = vec2(0.01, 0.0);
    vec3 a1 = vec3(position - m.xy, getwater(position - m.xy));
    vec3 a2 = vec3(position - m.yx, getwater(position - m.yx));
	return normalize(cross(a1 - a, a2 - a)).xzy;
}
vec2 refractUV(vec3 pos, vec3 norm){
	vec3 rdir = normalize(pos - CameraPosition);
	vec3 crs1 = cross(vec3(0, 1, 0), rdir);
	vec3 crs2 = cross(crs1, rdir);
	vec3 rf = refract(rdir, norm, 0.6);
	return UV - vec2(dot(rf, crs1), dot(rf, crs2)) * 0.1;
}
float WaterLightingMult = 0.0;
vec3 ApplyWater(){
	vec3 recon = reconstructCameraSpace(UV);
	float dd = length(recon);
	vec3 vdir = normalize(recon);
	float dist = intersectPlane(Ray(CameraPosition, vdir), vec3(0,1,0), vec3(0,(CameraPosition.y < 1 ? -1 : 1),0));
	vec3 oc = vec3(0);
	vec3 position = CameraPosition + vdir * dist;
	float dd2 = distance(CameraPosition, position + vec3(0, 1-getwater(position.xz), 0));
	if(dist > 0 && dd > dd2){
		vec3 albedo = vec3(0.8, 0.88, 1.0);
		vec3 normal = getwatern(position.xz);
		normal.xz *= 0.1;
		normal = normalize(normal);
		float roughness = 0.1;
		float metalness = 0.1;
		float IOR = 0.5;
		newUV = refractUV(position, normal);
		oc = ApplyLighting(newUV, albedo, position, normal, roughness, metalness, IOR);
		
		//float tA = getwater(position.xz + 24.0);
		//WaterLightingMult = pow(dot(normal, vec3(0,1,0)), 99)*12;
	}
	return oc;
}
void main()
{
    Seed(UV + Time);
    randsPointer = (randomizer * 0.86786 ) ;
    vec2 nUV = UV;
    vec3 color1 = vec3(0);
    if(UseDeferred == 1) {
		vec3 water = ApplyWater();
        color1 += water + Lightning() * (1.0 + WaterLightingMult);
        //color1 += softLuminance(UV);
		//vec3 rc = FromCameraSpace(reconstructCameraSpace(UV));
		//color1 += rc * 0.1;
        //color1 += UseHBAO == 1 ? (softLuminance(nUV) * texture(HBAOTex, nUV).a) : (softLuminance(nUV));
    } else {
		color1 = texture(diffuseColorTex, UV).rgb;
	}
    
    //color1 += texture(HBAOTex, nUV).rrr;
    color1 += lightPoints();
    if(UseFog == 1) color1 += lookupFog(nUV);

    if(UseDepth == 1) color1 += emulateSkyWithDepth(nUV);

    centerDepth = texture(depthTex, UV).r;

    gl_FragDepth = centerDepth;

	if(DisablePostEffects == 0){
		color1 *= Brightness;
	}
    //float Y = dot(vec3(0.30, 0.59, 0.11), color1);
    //float YD = Brightness * (Brightness + 1.0) / (Brightness + 1.0);
    //color1 *= YD * Y;
    outColor = vec4(clamp(color1, 0.0, 10000.0), 1.0);
}
