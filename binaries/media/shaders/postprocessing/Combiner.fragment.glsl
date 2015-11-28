#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include UsefulIncludes.glsl
#include Lighting.glsl
#include FXAA.glsl
#include Shade.glsl

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
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.01);
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
vec2 projectOnScreen(vec3 worldcoord){
    vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(worldcoord, 1.0);
    vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
    if(clipspace.z < 0.0) return vec2(-1);
    return sspace1;
}
vec3 softLuminance(vec2 fuv){
    if(texture(diffuseColorTex, UV).r >= 999){ 
        return vec3(0);
    }
    vec3 outc = vec3(0);
    float counter = 1.0;
    vec3 posCenter = texture(worldPosTex, fuv).rgb;
	uint tangentEncoded = texture(meshIdTex, fuv).a;
    vec3 tangent = unpackSnorm4x8(tangentEncoded).xyz;
    vec3 normCenter = texture(normalsTex, fuv).rgb;
    vec3 dir = normalize(reflect(posCenter, normCenter));
    float meshRoughness = 1.0 - texture(meshDataTex, fuv).a; // glossyness actually
   // float samples = mix(32.0, 1.0, meshRoughness);
    vec3 nnormal = mix(dir, normCenter, 1.0 - meshRoughness);
    float dist = length(posCenter);

    TBN = inverse(transpose(mat3(
        tangent,
        cross(normCenter, tangent),
        normCenter
    )));
	float samples = mix(5, 5, 1.0 - meshRoughness);
    float stepsize = PI*2 / samples;
    float ringsize = length(posCenter)*0.8;
    //for(float g = 0; g < samples; g+=1)
    for(float g = 0.0; g <= PI*2; g+=stepsize)
    {
        float minang = 0;

        //vec3 displace = normalize(BRDF(dir, norm, meshRoughness)) * ringsize;
		float grd = getRand() * stepsize;
        vec2 zx = vec2(sin(g+grd), cos(g+grd));
        vec3 displace = mix((TBN * normalize(vec3(zx, sqrt(1.0 - length(zx))))), dir, meshRoughness) * ringsize;
        //vec3 displace = normalize(BRDFBiased(dir, norm, meshRoughness, (vec2(getRand2(), getRand2())))) * ringsize;
        
   // for(float gx = 0.0; gx < samples + 1; gx+=1)
   // {
        //vec2 rdr = vec2(gx, gy) / samples;
       // vec2 rdr = vec2(getRand(), getRand());
       // vec3 displace = normalize(BiasedBRDF(dir, normCenter, meshRoughness, rdr)) * 0.8 * dist;
        vec2 sspos2 = projectOnScreen(FromCameraSpace(posCenter) + displace);
        for(float g2 = 0.01; g2 < 1.0; g2+=0.2)
        {
            float fv = getRand();
            fv *= fv;
            vec2 gauss = mix(fuv, sspos2, fv);
            //if(texture(diffuseColorTex, gauss).r >= 999){ 
            //    continue;
           // }
            vec3 color = UseRSM == 1 ? (texture(currentTex, gauss).rgb + texture(indirectTex, gauss).rgb + texture(lastIndirectTex, gauss).rgb*1.0) : texture(currentTex, gauss).rgb + texture(lastIndirectTex, gauss).rgb*1.0;
            vec3 pos = texture(worldPosTex, gauss).rgb;
            vec3 norm = texture(normalsTex, gauss).rgb;
            float ftp = max(0, dot(norm, normalize(posCenter - pos)));
            outc += shadePhoton(fuv, color) * ftp;
            counter+=1;
        }
    }
    return outc*5 / counter;
}
vec3 blurByUV(sampler2D sampler, vec2 fuv, float force){
    vec3 outc = vec3(0);
    int counter = 0;
    for(float g = 0; g < mPI2; g+=GOLDEN_RATIO)
    {
        for(float g2 = 0; g2 < 3.0; g2+=1.0)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.001 * force);
            vec3 color = texture(sampler, fuv + gauss).rgb;
            outc += color;
            counter++;
        }
    }
    return outc / counter;
}
vec3 blurByUV2(sampler2D sampler, vec2 fuv, float force){
    vec3 outc = vec3(0);
    float counter = 0;
    float depthCenter = texture(depthTex, fuv).r;
    for(float g = 0; g < mPI2; g+=0.3)
    {
        for(float g2 = 0.02; g2 < 1.0; g2+=0.02)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.01 * force);
            vec3 color = clamp(texture(sampler, fuv + gauss).rgb, 0.02, 1.0);
            float depthThere = texture(depthTex, fuv + gauss).r;
           // if(abs(depthThere - depthCenter) < 0.001){
                outc += color*length(color);
                counter+=length(color);
           // }
        }
    }
    return clamp(outc / counter, 0.02, 1.0);
}
vec3 blurssao(sampler2D sampler, vec2 fuv, float force){
    float roughs = texture(meshDataTex, fuv).a;
    vec3 outc = vec3(0);
    float counter = 0;
    vec3 norm = texture(normalsTex, fuv).xyz;
    float depthCenter = texture(depthTex, fuv).r;
    for(float g = 0; g < mPI2; g+=0.1)
    {
        for(float g2 = 0.0002; g2 < 1.0; g2+=0.05)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * force)*0.004;
            vec3 color = texture(sampler, fuv + gauss).rgb;
            float depthd = texture(sampler, fuv + gauss).a;
            if(abs(depthCenter-depthd)<0.005){
                outc += color;
                counter+=1;
            }
        }
    }
    vec3 a = texture(sampler, fuv).rgb;
    if(counter == 0) return a;
    return outc/counter;
}

vec3 mixAlbedo(vec3 a){
    return mix(a*texture(diffuseColorTex, UV).rgb, a, texture(meshDataTex, UV).z);
}




vec3 emulateSkyWithDepth(vec2 uv){
    vec3 worldPos = (texture(worldPosTex, uv).rgb);
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

		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4((LightsPos[i]), 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;

        float badass_depth = distance(LightsPos[i], CameraPosition);
        float logg = length(texture(worldPosTex, sspace1).rgb);
        float mixv = 1.0 - smoothstep(0.1, 2.5, distance(sspace1*resolution.xy * 0.01, UV*resolution.xy * 0.009));

        if(logg > badass_depth) {
            color += ball(vec3(LightsColors[i].rgb*1.0),LightPointSize / ( badass_depth) * 0.1, sspace1.x, sspace1.y);
            //color += ball(vec3(LightsColors[i]*2.0 * overall),12.0 / dist, sspace1.x, sspace1.y) * 0.03f;
        }

	}

    return color;
}

uniform float VDAOGlobalMultiplier;

#include EnvironmentLight.glsl
#include Direct.glsl
#include AmbientOcclusion.glsl

vec3 Lightning(){
    if(texture(diffuseColorTex, UV).r >= 999){ 
		return texture(cubeMapTex, normalize(texture(worldPosTex, UV).rgb)).rgb;
    }
    vec3 albedo = texture(diffuseColorTex, UV).rgb;
    vec3 position = FromCameraSpace(texture(worldPosTex, UV).rgb);
    vec3 normal = normalize(texture(normalsTex, UV).rgb);
    float metalness =  texture(meshDataTex, UV).z;
    float roughness =  texture(meshDataTex, UV).a;
    float specular = texture(worldPosTex, UV).a;
    float IOR =  0.0;
	
	vec3 directlight = DirectLight(CameraPosition, albedo, normal, position, roughness, metalness, specular);
	vec3 envlight = VDAOGlobalMultiplier * EnvironmentLight(albedo, position, normal, metalness, roughness, specular,IOR);

	
    if(UseVDAO == 1 && UseHBAO == 0) directlight += envlight;
    if(UseHBAO == 1) {
		uint tangentEncoded = texture(meshIdTex, UV).a;
		vec3 tangent = unpackSnorm4x8(tangentEncoded).xyz;
		float ao = AmbientOcclusion(position, normal, tangent, roughness, metalness);
		if(UseVDAO == 0) envlight = vec3(1);
		directlight += envlight * ao;
		
	}

    return directlight;
}

uniform int DisablePostEffects;

void main()
{
    Seed(UV);
    randsPointer = int(randomizer * 113.86786 ) % RandomsCount;
    vec2 nUV = UV;
    vec3 color1 = vec3(0);
    if(UseDeferred == 1) {
        color1 += Lightning();
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
		vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2);
		color1.rgb = vec3(pow(color1.r, gamma.r),
		pow(color1.g, gamma.g),
		pow(color1.b, gamma.b));
	}
    //float Y = dot(vec3(0.30, 0.59, 0.11), color1);
    //float YD = Brightness * (Brightness + 1.0) / (Brightness + 1.0);
    //color1 *= YD * Y;
    outColor = vec4(color1, 1.0);
}
