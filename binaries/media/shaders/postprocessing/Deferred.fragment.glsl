#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

layout (std430, binding = 6) buffer RandomsBuffer
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand2s(seeder)) ;
}

int randsPointer = 0;
uniform int RandomsCount;
float getRand(){
    float r = Randoms[randsPointer];
    randsPointer++;
    if(randsPointer >= RandomsCount) randsPointer = 0;
    return r;
}

uniform int UseRSM;

const int MAX_SIMPLE_LIGHTS = 20;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

float meshRoughness;
float meshSpecular;
float meshDiffuse;


out vec4 outColor;
bool IgnoreLightingFragment = false;

vec2 refractUV(){
    vec3 rdir = normalize(CameraPosition - texture(worldPosTex, UV).rgb);
    vec3 crs1 = normalize(cross(CameraPosition, texture(worldPosTex, UV).rgb));
    vec3 crs2 = normalize(cross(crs1, rdir));
    vec3 rf = refract(rdir, texture(normalsTex, UV).rgb, 0.6);
    return UV - vec2(dot(rf, crs1), dot(rf, crs2)) * 0.3;
}

mat4 PV = (ProjectionMatrix * ViewMatrix);
#define PI 3.14159265

float orenNayarDiffuse(
vec3 lightDirection,
vec3 viewDirection,
vec3 surfaceNormal,
float roughness,
float albedo) {

    float LdotV = dot(lightDirection, viewDirection);
    float NdotL = dot(lightDirection, surfaceNormal);
    float NdotV = dot(surfaceNormal, viewDirection);

    float s = LdotV - NdotL * NdotV;
    float t = mix(1.0, max(NdotL, NdotV), step(0.0, s));

    float sigma2 = roughness * roughness;
    float A = 1.0 + sigma2 * (albedo / (sigma2 + 0.13) + 0.5 / (sigma2 + 0.33));
    float B = 0.45 * sigma2 / (sigma2 + 0.09);

    return albedo * max(0.0, NdotL) * (A + B * s / t) / PI;
}

float beckmannDistribution(float x, float roughness) {
    float NdotH = max(x, 0.001);
    float cos2Alpha = NdotH * NdotH;
    float tan2Alpha = (cos2Alpha - 1.0) / cos2Alpha;
    float roughness2 = roughness * roughness;
    float denom = 3.141592653589793 * roughness2 * cos2Alpha * cos2Alpha;
    return exp(tan2Alpha / roughness2) / denom;
}

float beckmannSpecular(
vec3 lightDirection,
vec3 viewDirection,
vec3 surfaceNormal,
float roughness) {
    return beckmannDistribution(dot(surfaceNormal, normalize(lightDirection + viewDirection)), roughness);
}

float cookTorranceSpecular(
vec3 lightDirection,
vec3 viewDirection,
vec3 surfaceNormal,
float roughness,
float fresnel) {

    float VdotN = max(dot(viewDirection, surfaceNormal), 0.0);
    float LdotN = max(dot(lightDirection, surfaceNormal), 0.0);

    //Half angle vector
    vec3 H = normalize(lightDirection + viewDirection);

    //Geometric term
    float NdotH = max(abs(dot(surfaceNormal, H)), 0.0);
    float VdotH = max(abs(dot(viewDirection, H)), 0.0001);
    float LdotH = max(abs(dot(lightDirection, H)), 0.0001);
    float G1 = (2.0 * NdotH * VdotN) / VdotH;
    float G2 = (2.0 * NdotH * LdotN) / LdotH;
    float G = min(1.0, min(G1, G2));

    //Distribution term
    float D = beckmannDistribution(NdotH, roughness);

    //Fresnel term
    float F = 1;

    //Multiply terms and done
    return  G * F * D / max(3.14159265 * VdotN, 0.001);
}

float CalculateFallof( float dist){
    //return 1.0 / pow(((dist) + 1.0), 2.0);
    return dist == 0 ? 3 : (3) / (14*PI*dist*dist+1);
    
}

vec3 shade(
    vec3 albedo, 
    vec3 normal,
    vec3 fragmentPosition, 
    vec3 lightPosition, 
    vec4 lightColor, 
    float roughness, 
    float metalness, 
    float specular
){
    vec3 lightRelativeToVPos =normalize( lightPosition - fragmentPosition);
    
    vec3 cameraRelativeToVPos = normalize(-ToCameraSpace(fragmentPosition));
    
    float distanceToLight = distance(fragmentPosition, lightPosition);
    float att = CalculateFallof(distanceToLight)* lightColor.a*10;
    if(att < 0.002) return vec3(0);
    
    float specularComponent = specular * clamp(cookTorranceSpecular(
        lightRelativeToVPos,
        cameraRelativeToVPos,
        normal,
        max(0.02, roughness), 1
        ), 0.0, 1.0);

    
    float diffuseComponent = clamp(orenNayarDiffuse(
        lightRelativeToVPos,
        cameraRelativeToVPos,
        normal,
        max(0.02, roughness), 1
        ), 0.0, 1.0);   

    vec3 cc = mix(lightColor.rgb*albedo, lightColor.rgb, metalness);
    
    vec3 difcolor = cc * diffuseComponent * att;
    vec3 difcolor2 = lightColor.rgb * albedo * diffuseComponent * att;
    vec3 specolor = cc * specularComponent;
    
    return mix(difcolor2 + specolor, difcolor*roughness + specolor, metalness);
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
    // at this point displace is hemisphere sampled "uniformly"
    
    float dt = dot(displace, reflectdir) * 0.5 + 0.5;
    // dt is difference between sample and ideal mirror reflection, 0 means completely other direction
    // dt will be used as "drag" to mirror reflection by roughness
    
    // for roughness 1 - mixfactor must be 0, for rughness 0, mixfactor must be 1
    float mixfactor = mix(0, 1, roughness);
    
    return mix(displace, reflectdir, roughness);
}
vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}
vec3 adjustGamma(vec3 c, float gamma){
    return vec3pow(c, 1.0/gamma)/gamma;
}

vec2 HitPos = vec2(-2);
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(int i=0; i<5; i++){
        float ix = getRand();
        vec2 muv = mix(p1, p2, ix);
        float expr = min(muv.x, muv.y) * -(max(muv.x, muv.y)-1.0);
        float tmp = min(0, sign(expr)) * 
            ret + 
            min(0, -sign(expr)) * 
            max(
                mix(v1, v2, ix) - texture(sampler, muv).r,
                ret);
        if(tmp > ret) HitPos = muv;
        ret = tmp;
    }
    //if(abs(ret) > 0.1) return 0;
    return abs(ret) - 0.0006;
}
vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}
float testVisibility3d(vec2 cuv, vec3 w1, vec3 w2) {
    HitPos = vec2(-2);
    vec4 clipspace = (PV) * vec4((w1), 1.0);
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) * vec4((w2), 1.0);
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    float d3d1 = toLogDepth(length(ToCameraSpace(w1)));
    float d3d2 = toLogDepth(length(ToCameraSpace(w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, depthTex));

    return mx;
}
uniform int UseVDAO;
vec3 Radiosity()
{
    
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    float meshSpecular = texture(worldPosTex, UV).a;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    vec3 ambient = vec3(0);
    const int samples = 14;
    
    float octaves[] = float[4](0.8, 3.0, 7.9, 10.0);
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
    
    float initialAmbient = 0.0;

    uint counter = 0;   
    float meshRoughness1 = 1.0 - texture(meshDataTex, UV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    fresnel = fresnel * fresnel * fresnel*(1.0-meshMetalness)*(meshRoughness1)*0.4 + 1.0;
    
    float brfds[] = float[2](min(meshMetalness, meshRoughness1), meshRoughness1);
    for(int bi = 0; bi < brfds.length(); bi++)
    {
        for(int i=0; i<samples; i++)
        {
            float meshRoughness =brfds[bi];
            vec3 displace = normalize(BRDF(dir, normalCenter, meshRoughness));
            
            float vi = testVisibility3d(UV, FromCameraSpace(posCenter), FromCameraSpace(posCenter) + displace * 0.3) <= 0 ? 1: 0;
            vi = HitPos.x > 0 ? mix(1.0, 0.0, distance(FromCameraSpace(texture(worldPosTex, HitPos).rgb), FromCameraSpace(posCenter)) / 3.2) : 1;
            
            vec3 color = adjustGamma(texture(cubeMapTex, displace).rgb, mix(0.7, 1.0, meshMetalness)) ;
            float dotdiffuse = max(0, dot(displace, normalCenter));
            ambient += color* dotdiffuse * fresnel ;
            counter++;
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs*0.2);
}
void main()
{   
    if(texture(diffuseColorTex, UV).r >= 999){ 
        outColor = texture(cubeMapTex, normalize(texture(worldPosTex, UV).rgb)).rgba;
        return;
    }
  //  float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
   // if(alpha < 0.99){
        //nUV = refractUV();
   // }
    vec3 colorOriginal = texture(diffuseColorTex, nUV).rgb;    
    vec4 normal = texture(normalsTex, nUV);
    meshDiffuse = normal.a;
    meshSpecular = texture(worldPosTex, nUV).a;
    meshRoughness = texture(meshDataTex, nUV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    vec3 color1 = vec3(0);
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        color1 = colorOriginal;
        IgnoreLightingFragment = true;
    } else {
        // color1 = colorOriginal * 0.01;
    }
    
    //vec3 color1 = colorOriginal * 0.2;
    //if(texture(texColor, UV).a < 0.99){
    //    color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    //}
    gl_FragDepth = texture(depthTex, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = normalize(-fragmentPosWorld3d.xyz);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
   // int foundSun = 0;
    if(!IgnoreLightingFragment) for(int i=0;i<LightsCount;i++){
        
       // if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && foundSun > 0)continue;
        //if(len < LightsRanges[i].x) continue;
        //if(len > LightsRanges[i].y) continue;

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(fragmentPosWorld3d.xyz, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   
        
        
        
        //int counter = 0;  

        // do shadows
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0){ 
            float percent = getShadowPercent(lightScreenSpace, fragmentPosWorld3d.xyz, i);
            vec3 abc = LightsPos[i];
            
            vec3 lightRelativeToVPos =normalize( abc - fragmentPosWorld3d.xyz);
            
            
            float distanceToLight = distance(fragmentPosWorld3d.xyz, abc);
            float att = CalculateFallof(distanceToLight)* LightsColors[i].a*10;
            //att = 1;
           // if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE)att = 1;
            if(att < 0.002) continue;
            
            float specularComponent = meshSpecular * clamp(cookTorranceSpecular(
            lightRelativeToVPos,
            cameraRelativeToVPos,
            normal.xyz,
            max(0.02, meshRoughness), 1
            ), 0.0, 1.0);

            
            float diffuseComponent = clamp(orenNayarDiffuse(
            lightRelativeToVPos,
            cameraRelativeToVPos,
            normal.xyz,
            meshRoughness, 1
            ), 0.0, 1.0);   
            
          //  float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
          //  fresnel = fresnel * fresnel * fresnel*(1.0-meshMetalness)*(1.0-meshRoughness)*0.4 + 1.0;
            
            //vec3 illumalbedo = vec3((LightsColors[i].r+LightsColors[i].g+LightsColors[i].b)*0.333);
            vec3 cc = mix(LightsColors[i].rgb*colorOriginal, LightsColors[i].rgb, meshMetalness);
            
            vec3 difcolor = cc * diffuseComponent * att;
            vec3 difcolor2 = LightsColors[i].rgb*colorOriginal * diffuseComponent * att;
            vec3 specolor = cc * specularComponent;
            
            vec3 radiance = mix(difcolor2 + specolor, difcolor*meshRoughness + specolor, meshMetalness);
            
          //  float culler = max(0, 1.0-distance(lightScreenSpace, vec2(0.5))*2);
            
            color1 += (radiance) * percent;
            
            
            //   if(percent < 0){
            //is in shadow! lets try subsufrace scattering
            // float subsc =  min(0.1, abs(LastProbeDistance));
            //  float amount = 1.0/(pow((subsc *500)+1, 2));
            //  amount = abs(amount);
            
            // color1 += colorOriginal *  max(0.1, amount);
            //    } 
           // if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) foundSun = 1;
        }
    }
    Seed(UV);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    if(UseVDAO == 1) color1 += Radiosity();
    outColor = clamp(vec4(color1, 1.0), 0.0, 1.0);
    
    
}