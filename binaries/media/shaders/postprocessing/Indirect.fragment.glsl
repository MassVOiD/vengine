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


bool IgnoreLightingFragment = false;
out vec4 outColor;

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
vec2 HitPos = vec2(-2);
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(int i=0; i<16; i++){
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

vec3 RSM(){
    if(UseRSM != 1) return vec3(0);
    float alpha = texture(diffuseColorTex, UV).a;
    vec2 nUV = UV;
    if(alpha < 0.99){
        //nUV = refractUV();
    }
    vec3 colorOriginal = texture(diffuseColorTex, nUV).rgb;
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
    if(texture(diffuseColorTex, UV).a < 0.99){
        color1 += texture(diffuseColorTex, UV).rgb * texture(diffuseColorTex, UV).a;
    }
    gl_FragDepth = texture(depthTex, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = normalize(-vec3(fragmentPosWorld3d.xyz));
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    int foundSun = 0;

    float octaves[] = float[4](0.8, 2.0, 4.0, 6.0);
    
    #define RSMSamples 25
    for(int i=0;i<LightsCount;i++){
        //break;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && foundSun == 1) continue;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) foundSun = 1;
        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        mat4 invlightPV = inverse(LightsPs[i] * LightsVs[i]);
        vec3 centerpos = LightsPos[i];
        
        vec3 lightRelativeToSample = normalize(centerpos - fragmentPosWorld3d.xyz);
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
            uvec2 packed =  lookupColorFromLight(i, scruv);
            //packUnorm4x8(vec4(radiance, Roughness)), packSnorm4x8(vec4(normal, Metalness))
            vec4 upackA = unpackUnorm4x8(packed.r);
            vec4 upackB = unpackSnorm4x8(packed.g);
            vec3 lcolor = upackA.rgb;
            float lrough = upackA.a;
            vec3 lnormal = upackB.rgb;
            float lmetal = upackB.a;
            
            scruv.y = 1.0 - scruv.y;
            scruv = scruv * 2 - 1;
            reconstructDir = invlightPV * vec4(scruv, 1.0, 1.0);
            reconstructDir.xyz /= reconstructDir.w;
            float revlog = reverseLogEx(ldep, LightsFarPlane[i]);
            // not optimizable
            vec3 newpos = normalize(reconstructDir.xyz - centerpos) * revlog + LightsPos[i];
            
            
            float distanceToLight = distance(fragmentPosWorld3d.xyz, newpos);
            vec3 lightRelativeToVPos = normalize(newpos - fragmentPosWorld3d.xyz);
            vec3 lightRelativeToVPos2 = normalize(newpos - centerpos);
            float att = CalculateFallof(distanceToLight + revlog) *  LightsColors[i].a*10;
            
            float vi = testVisibility3d(nUV, fragmentPosWorld3d.xyz + lightRelativeToVPos*3.2, fragmentPosWorld3d.xyz);
            vi = HitPos.x > 0 ? mix(1.0, 0.0, distance(FromCameraSpace(texture(worldPosTex, HitPos).rgb), fragmentPosWorld3d.xyz) / 3.2) : 1;

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
            
            vec3 refl = reflect(-lightRelativeToVPos2, lnormal);
            float spfsm = max(0, dot(refl, lightRelativeToVPos));
            spfsm = pow(spfsm, 255 * (1.0-lrough) + 1) * max(0, sign(dot(lightRelativeToVPos, lnormal))) * (5-lrough);
            spfsm = clamp(cookTorranceSpecular(
            -lightRelativeToVPos2,
            -lightRelativeToVPos,
            lnormal,
            max(0.01, lrough), 1
            ), 0.0, 1.0)*5;
            
            color1 += (radiance * att * spfsm) * vi;
            
            
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
uniform int UseVDAO;
vec3 Radiosity()
{
    
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    vec3 albedo = texture(diffuseColorTex, UV).rgb;
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
    float brfds[] = float[2](min(meshMetalness, meshRoughness1), meshRoughness1);
    for(int bi = 0; bi < brfds.length(); bi++)
    {
        for(int i=0; i<samples; i++)
        {
            float meshRoughness =brfds[bi];
            vec3 displace = normalize(BRDF(dir, normalCenter, meshRoughness));
            vec3 color = adjustGamma(texture(cubeMapTex, displace).rgb, mix(0.7, 1.0, meshMetalness)) ;
            color = mix(color*albedo, color, meshMetalness);
            float dotdiffuse = max(0, dot(displace, normalCenter));
            ambient += color* dotdiffuse * fresnel;
            counter++;
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs);
}

void main()
{   
    if(texture(diffuseColorTex, UV).r >= 999){ 
        outColor = texture(cubeMapTex, normalize(texture(worldPosTex, UV).rgb)).rgba;
        return;
    }
    vec3 color1 = vec3(0);
    
    Seed(UV+2);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    vec4 last = texture(lastIndirectTex, UV);
    vec4 ou = vec4(0);
    if(UseRSM == 1 && UseVDAO == 1){
        ou = vec4(Radiosity() + RSM(), 1);
        
    } else if(UseRSM == 0 && UseVDAO == 1){
        ou = vec4(Radiosity(), 1);
        
    } else if(UseRSM == 1 && UseVDAO == 0){
        ou = vec4(RSM(), 1);
        
    } else {
        ou = vec4(color1, 1);
    }
    outColor = clamp(mix(ou, last, 0.99), 0.0, 1.0);
    
    
}