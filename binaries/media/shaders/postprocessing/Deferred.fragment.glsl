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
    return dist == 0 ? 3 : (3) / (4*PI*dist*dist+1);
    
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
    outColor = clamp(vec4(color1, 1.0), 0.0, 1.0);
    
    
}