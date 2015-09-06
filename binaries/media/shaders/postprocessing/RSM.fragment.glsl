#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;
layout(binding = 32) uniform sampler2D backDepth;
layout(binding = 33) uniform sampler2D meshDataTex;

//layout (binding = 11, r32ui) volatile uniform uimage3D full3dScene;

const int MAX_SIMPLE_LIGHTS = 20;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

float meshRoughness;
float meshSpecular;
float meshDiffuse;


out vec4 outColor;
bool IgnoreLightingFragment = false;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
layout (std430, binding = 6) buffer RandomsBuffer
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand(seeder)) ;
}

int randsPointer = 0;
uniform int RandomsCount;
float getRand(){
    float r = Randoms[randsPointer];
    randsPointer++;
    if(randsPointer >= RandomsCount) randsPointer = 0;
    return r;
}
vec2 HitPos = vec2(0);
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
                mix(v1, v2, ix) - reverseLog(texture(sampler, muv).r),
                ret);
        if(tmp > ret) HitPos = muv;
        ret = tmp;
    }
    //if(abs(ret) > 0.1) return 0;
    return abs(ret) - 0.00;
}
float textureMaxFromLineNegate(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 9999;
    for(float i=0;i<1;i+=0.33)
        ret = min(mix(v1, v2, i) - texture(sampler, mix(p1, p2, i)).r, ret);

    return ret;
}

vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}

mat4 PV = (ProjectionMatrix * ViewMatrix);
float testVisibility3d(vec2 cuv, vec3 w1, vec3 w2) {
    vec4 clipspace = (PV) * vec4((w1), 1.0);
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) * vec4((w2), 1.0);
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    float d3d1 = (length(ToCameraSpace(w1)));
    float d3d2 = (length(ToCameraSpace(w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
    //float mx2 = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));

    return mx;
}

vec3 LightingPhysical(
    vec3 lightColor,
    float albedo,
    float gloss,
    vec3 normal,    
    vec3 lightDir,
    vec3 viewDir,
    float atten )
    {
        // calculate diffuse term
        float n_dot_l = clamp( dot( normal, lightDir ) * atten, 0.0, 1.0);
        vec3 diffuse = n_dot_l * lightColor;

        // calculate specular term
        vec3 h = normalize( lightDir + viewDir );

        float n_dot_h = clamp( dot( normal, h ), 0.0, 1.0);
        float normalization_term = ( ( meshSpecular * meshRoughness ) + 2.0 ) / 8.0;
        float blinn_phong = pow( n_dot_h, meshSpecular * meshRoughness );
        float specular_term = blinn_phong * normalization_term;
        float cosine_term = n_dot_l;

        float h_dot_l = dot( h, lightDir );
        float base = 1.0 - h_dot_l;
        float exponential =	pow( base, 5.0 );

        vec3 specColor = vec3(1) * gloss;
        vec3 fresnel_term = specColor + ( 1.0 - specColor ) * exponential;

        vec3 specular = specular_term * cosine_term * fresnel_term * lightColor;

        vec3 final_output = diffuse * ( 1 - fresnel_term );
        return final_output;
    }

float step2(float a, float b, float x){
    return step(a, x) * (1.0-step(b, x));
}
    
#define PI 3.14159265

#define M_PI 3.1415926535897932384626433832795

float CalculateFallof( float dist){
    //return 1.0 / pow(((dist) + 1.0), 2.0);
    return dist == 0 ? 3 : (3) / (4*M_PI*dist*dist+1);
    
}
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
    float F = pow(1.0 - VdotN, fresnel);

    //Multiply terms and done
    return  G * F * D / max(3.14159265 * VdotN, 0.001);
}
uniform int UseRSM;
vec3 hbao(){
    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0, counter = 0, div = 1.0/(length(posc)+1.0);
    float octaves[] = float[3](1.5, 2.3, 7.0);
    float roughness =  1.0-texture(meshDataTex, UV).a;
    for(int p=0;p<octaves.length();p++){
        for(float g = 0; g < mPI2; g+=0.2){
            vec3 pos = texture(worldPosTex,  UV + (vec2(sin(g)*ratio, cos(g)) * (getRand() * octaves[p])) * div).rgb;
            buf += max(0, sign(length(posc) - length(pos)))
                 * (max(0, 1.0-pow(1.0-max(0, dot(norm, normalize(pos - posc))), (roughness)*26+1)))
                 * max(0, (6.0 - length(pos - posc))/10.0);
            counter+=0.3;
        }
    }

    return vec3(0.7)* (1.0 - min(buf / counter, 0.99));
}
void main()
{
    Seed(UV+2);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    if(UseRSM != 1) return;
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
    vec3 cameraRelativeToVPos = -vec3(fragmentPosWorld3d.xyz);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    int foundSun = 0;

    float octaves[] = float[4](0.8, 2.0, 4.0, 6.0);
    
    #define RSMSamples 6
    for(int i=0;i<LightsCount;i++){
        //break;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && foundSun == 1) continue;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) foundSun = 1;
        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        mat4 invlightPV = inverse(LightsPs[i] * LightsVs[i]);
        vec3 centerpos = LightsPos[i];
        for(int x=0;x<RSMSamples;x++){
            for(int y=0;y<RSMSamples;y++){
                //float rd = rand(UV);
                vec2 scruv = vec2(float(x) / RSMSamples, float(y) /RSMSamples);
                //scruv = vec2(sin(scruv.x+scruv.y), cos(scruv.x+scruv.y)) * scruv.y;
                //scruv = scruv * 0.5 + 0.5;
                scruv = vec2(getRand(), getRand());
                float ldep = lookupDepthFromLight(i, scruv);
                vec3 lcolor = lookupColorFromLight(i, scruv)*LightsColors[i].rgb;
                scruv.y = 1.0 - scruv.y;
                scruv = scruv * 2 - 1;
                vec4 reconstructDir = invlightPV * vec4(scruv, 1.0, 1.0);
                reconstructDir.xyz /= reconstructDir.w;
                vec3 dir = normalize(
                    reconstructDir.xyz - centerpos
                );

                // not optimizable
                vec3 newpos = dir * reverseLogEx(ldep, LightsFarPlane[i]) + LightsPos[i];
                float distanceToLight = distance(fragmentPosWorld3d.xyz, newpos);
                vec3 lightRelativeToVPos = normalize(newpos - fragmentPosWorld3d.xyz);
                float att = CalculateFallof(distanceToLight) * CalculateFallof(reverseLogEx(ldep, LightsFarPlane[i]))* LightsColors[i].a*10;
                //float vi = testVisibility3d(nUV, fragmentPosWorld3d.xyz + lightRelativeToVPos*6.5, fragmentPosWorld3d.xyz);
                //vi = (step(0.0, -vi))+smoothstep(0.0, 2.91, vi);
                
                float bl = 1;
                for(int p=0;p<octaves.length();p++){
                
                    float av = testVisibility3d(nUV, fragmentPosWorld3d.xyz + lightRelativeToVPos*octaves[p], fragmentPosWorld3d.xyz);
                    
                    bl *= av <= 0.00 || av > 1.9 ? 1.0 : 0.05;
                }
                float vi = bl;
              //float vi = 1;
                //    vi = 1.0-vi;
                /*float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
                fresnel = fresnel * fresnel * fresnel + 1.0;
               
                    color1 += vi* fresnel * LightingPhysical(
                        LightsColors[i].rgb*LightsColors[i].a,
                        meshDiffuse,
                        meshSpecular,
                        normal.xyz,
                        (lightRelativeToVPos),
                        normalize(cameraRelativeToVPos),
                        att);*/
                         float specularComponent = clamp(cookTorranceSpecular(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            max(0.02, meshRoughness), 1
            ), 0.0, 1.0);

            
            float diffuseComponent = clamp(orenNayarDiffuse(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            meshRoughness, 1
            ), 0.0, 1.0);   
            //float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
            //fresnel = fresnel * fresnel * fresnel + 1.0;
            
            vec3 illumalbedo = vec3((lcolor.r+lcolor.g+lcolor.b)*0.333);
            vec3 cc = mix(lcolor*colorOriginal, lcolor, meshMetalness);
            
            vec3 difcolor = cc * diffuseComponent;
            vec3 difcolor2 = lcolor*colorOriginal * diffuseComponent;
            vec3 specolor = cc * specularComponent;
            
            vec3 radiance = mix(difcolor2 + specolor, difcolor*meshRoughness + specolor, meshMetalness);
            
            float culler = max(0, 1.0-distance(scruv, vec2(0.5))*2);
            
            color1 += (radiance) * att *3;
                
                
               // color1 += ((colorOriginal * (diffuseComponent * lcolor)) 
               // + (mix(colorOriginal, lcolor*colorOriginal, meshRoughness) * specularComponent))
               // * att * vi * LightsColors[i].a;   
                
            }
        }
    }
    outColor = vec4(clamp(color1 / (RSMSamples*RSMSamples), 0, 1)*hbao(), texture(texDepth, nUV).r);
   // outColor = vec4(0,0,0, 1);
}
