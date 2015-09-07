#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;
layout(binding = 33) uniform sampler2D meshDataTex;
layout(binding = 34) uniform sampler2D meshIdTex;
layout(binding = 35) uniform sampler2D lastTex;
layout(binding = 29) uniform samplerCube CubeMap;

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
    return dist == 0 ? 3 : (3) / (4*PI*dist*dist+1);
    
}

float hbao(){
    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0, counter = 0, div = 1.0/(length(posc)+1.0);
    float octaves[] = float[2](0.5, 2.0);
    float roughness =  1.0-texture(meshDataTex, UV).a;
    for(int p=0;p<octaves.length();p++){
        for(float g = 0; g < mPI2; g+=0.4){
           // float rda = getRand() * mPI2;
            vec3 pos = texture(worldPosTex,  UV + (vec2(sin(g)*ratio, cos(g)) * (getRand() * octaves[p])) * div).rgb;
            buf += max(0, sign(length(posc) - length(pos)))
            * (max(0, 1.0-pow(1.0-max(0, dot(norm, normalize(pos - posc))), (roughness)*26+1)))
            * max(0, (6.0 - length(pos - posc))/10.0);
            counter+=0.4;
        }
    }

    return pow(1.0 - buf / counter, 1.1);
}

vec3 RSM(){
    if(UseRSM != 1) return vec3(0);
    float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
    if(alpha < 0.99){
        //nUV = refractUV();
    }
    vec3 colorOriginal = texture(texColor, nUV).rgb;
    vec4 normal = texture(normalsTex, nUV);
    meshDiffuse = normal.a;
    meshSpecular = texture(worldPosTex, nUV).a;
    vec3 poscenter = texture(worldPosTex, nUV).rgb;
    meshRoughness = texture(meshDataTex, nUV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    vec3 color1 = vec3(0);
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        color1 = colorOriginal;
        IgnoreLightingFragment = true;
    } else {
        color1 = vec3(0);
    }

    //vec3 color1 = colorOriginal * 0.2;
    if(texture(texColor, UV).a < 0.99){
        color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    }
    gl_FragDepth = texture(texDepth, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = normalize(-vec3(fragmentPosWorld3d.xyz));
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    int foundSun = 0;

    float octaves[] = float[4](0.8, 2.0, 4.0, 6.0);
    
    #define RSMSamples 8
    for(int i=0;i<LightsCount;i++){
        //break;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && foundSun == 1) continue;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) foundSun = 1;
        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        mat4 invlightPV = inverse(LightsPs[i] * LightsVs[i]);
        vec3 centerpos = LightsPos[i];
        
        vec2 scruv;
        vec4 reconstructDir;
        vec3 dir;
        for(int x=0;x<RSMSamples;x++){
            //float rd = rand(UV);
           // vec2 scruv = vec2(float(x) / RSMSamples, float(y) /RSMSamples);
            //scruv = vec2(sin(scruv.x+scruv.y), cos(scruv.x+scruv.y)) * scruv.y;
            //scruv = scruv * 0.5 + 0.5;
            scruv = vec2(getRand(), getRand());
            
            float ldep = lookupDepthFromLight(i, scruv);            
            vec3 lcolor =  lookupColorFromLight(i, scruv).rgb;
            
            scruv.y = 1.0 - scruv.y;
            scruv = scruv * 2 - 1;
            reconstructDir = invlightPV * vec4(scruv, 1.0, 1.0);
            reconstructDir.xyz /= reconstructDir.w;
            float revlog = reverseLogEx(ldep, LightsFarPlane[i]);
            // not optimizable
            vec3 newpos = normalize(reconstructDir.xyz - centerpos) * revlog + LightsPos[i];
            
            float distanceToLight = distance(fragmentPosWorld3d.xyz, newpos);
            vec3 lightRelativeToVPos = normalize(newpos - fragmentPosWorld3d.xyz);
            float att = CalculateFallof(distanceToLight + revlog) *  LightsColors[i].a*10;

           // if(dot(normal.xyz, lightRelativeToVPos) < 0) continue;
           // if(dot(lightRelativeToVPos, normalize(reconstructDir.xyz - centerpos)) < 0.5) continue;
            
            float specularComponent = clamp(cookTorranceSpecular(
            lightRelativeToVPos,
            cameraRelativeToVPos,
            normal.xyz,
            max(0.01, meshRoughness), 1
            ), 0.0, 1.0);

            
            float diffuseComponent = clamp(orenNayarDiffuse(
            lightRelativeToVPos,
            cameraRelativeToVPos,
            normal.xyz,
            meshRoughness, 1
            ), 0.0, 1.0);   
            

            
            //float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
            //fresnel = fresnel * fresnel * fresnel*(1.0-meshMetalness)*(1.0-meshRoughness)*0.4 + 1.0;
            
            vec3 cc = mix(lcolor*colorOriginal, lcolor, meshMetalness);
            
            vec3 difcolor = cc * diffuseComponent;
            vec3 difcolor2 = lcolor*colorOriginal * diffuseComponent;
            vec3 specolor = cc * specularComponent;
            
            vec3 radiance = mix(difcolor2 + specolor, difcolor*meshRoughness + specolor, meshMetalness);
            
            color1 += (radiance * att);
            
            
            // color1 += ((colorOriginal * (diffuseComponent * lcolor)) 
            // + (mix(colorOriginal, lcolor*colorOriginal, meshRoughness) * specularComponent))
            // * att * vi * LightsColors[i].a;   
            
        }
    
    }
    return color1 / (RSMSamples);
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
vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}

uniform int UseVDAO;
uniform int UseHBAO;
vec3 Radiosity()
{
    
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    vec3 albedo = texture(texColor, UV).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    vec3 ambient = vec3(0);
    const int samples = 16;
    
    float octaves[] = float[4](0.8, 3.0, 7.9, 10.0);
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
    
    float initialAmbient = 0.0;

    uint counter = 0;   
    float meshRoughness1 = 1.0 - texture(meshDataTex, UV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    fresnel = fresnel * fresnel * fresnel*(1.0-meshMetalness)*(meshRoughness1)*0.4 + 1.0;
    
    vec3 colorplastic = vec3(0.851, 0.788, 0);
    float brfds[] = float[1]( meshRoughness1);
    for(int bi = 0; bi < brfds.length(); bi++)
    {
        for(int i=0; i<samples; i++)
        {
            float meshRoughness =brfds[bi];
            vec3 displace = normalize(BRDF(dir, normalCenter, meshRoughness));
            vec3 color = adjustGamma(texture(CubeMap, displace).rgb, mix(0.7, 1.0, meshMetalness)) ;
            color = mix(color*albedo, color, meshMetalness);
            float dotdiffuse = max(0, dot(displace, normalCenter));
            ambient += color* dotdiffuse * fresnel;
            counter++;
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs *0.2);
}

void main()
{   
    if(texture(texColor, UV).r >= 999){ 
        outColor = texture(CubeMap, normalize(texture(worldPosTex, UV).rgb)).rgba;
        return;
    }
  //  float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
   // if(alpha < 0.99){
        //nUV = refractUV();
   // }
    vec3 colorOriginal = texture(texColor, nUV).rgb;    
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
    gl_FragDepth = texture(texDepth, nUV).r;
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
            
            float specularComponent = clamp(cookTorranceSpecular(
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
            
            vec3 difcolor = cc * diffuseComponent;
            vec3 difcolor2 = LightsColors[i].rgb*colorOriginal * diffuseComponent;
            vec3 specolor = cc * specularComponent;
            
            vec3 radiance = mix(difcolor2 + specolor, difcolor*meshRoughness + specolor, meshMetalness);
            
          //  float culler = max(0, 1.0-distance(lightScreenSpace, vec2(0.5))*2);
            
            color1 += (radiance) * percent * att;
            
            
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
    Seed(UV+2);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    vec4 last = texture(lastTex, UV);
    vec4 ou = vec4(0);
    if(UseRSM == 1 && UseHBAO == 1 && UseVDAO == 1){
        ou = vec4(color1 + hbao() * (Radiosity() + RSM()), 1);
        
    } else if(UseRSM == 0 && UseHBAO == 1 && UseVDAO == 1){
        ou = vec4(color1 + hbao() * Radiosity(), 1);
        
    } else if(UseRSM == 1 && UseHBAO == 0 && UseVDAO == 1){
        ou = vec4(color1 + Radiosity() + RSM(), 1);
        
    } else if(UseRSM == 1 && UseHBAO == 1 && UseVDAO == 0){
        ou = vec4(color1 + hbao() * (RSM()), 1);
        
    } else if(UseRSM == 0 && UseHBAO == 0 && UseVDAO == 1){
        ou = vec4(color1 + Radiosity(), 1);
        
    } else if(UseRSM == 1 && UseHBAO == 0 && UseVDAO == 0){
        ou = vec4(color1 + RSM(), 1);
        
    } else if(UseRSM == 0 && UseHBAO == 1 && UseVDAO == 0){
        ou = vec4(color1 + hbao(), 1);
        
    } else {
        ou = vec4(color1, 1);
    }
    outColor = clamp(mix(ou, last, 0.9), 0.0, 1.0);
    
    
}