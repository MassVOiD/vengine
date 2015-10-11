#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

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
    
    #define RSMSamples 64*64
    for(int i=0;i<LightsCount;i++){
        //break;
        for(int x=0;x<RSMSamples;x+=3){
        
            //RSMLight light = rsmLights[int(getRand()*64*64)];
            RSMLight light = rsmLights[x];
            vec3 lcolor = light.Color.rgb;
            float lrough = light.Position.a;
            vec3 lnormal = light.normal.rgb;
            float lmetal = light.normal.a;
            vec3 newpos = light.Position.xyz;
            
            
            float distanceToLight = distance(fragmentPosWorld3d.xyz, newpos);
            vec3 lightRelativeToVPos = normalize(newpos - fragmentPosWorld3d.xyz);
            vec3 lightRelativeToVPos2 = normalize(newpos - LightsPos[i]);
            float incomeDiffuse = max(0, dot(lightRelativeToVPos2, -lnormal));
            float att = CalculateFallof(distanceToLight) *  LightsColors[i].a;
            
            float specularComponent = cookTorranceSpecular(
            lightRelativeToVPos,
            cameraRelativeToVPos,
            normal.xyz,
            max(0.01, meshRoughness)
            );

            
            float diffuseComponent = orenNayarDiffuse(
            lightRelativeToVPos,
            cameraRelativeToVPos,
            normal.xyz,
            meshRoughness
            );   
            

            
            //float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
            //fresnel = fresnel * fresnel * fresnel*(1.0-meshMetalness)*(1.0-meshRoughness)*0.4 + 1.0;
            
            vec3 cc = mix(lcolor*colorOriginal, lcolor, meshMetalness)*incomeDiffuse;
            
            vec3 difcolor = cc * diffuseComponent * att;
            vec3 difcolor2 = lcolor*colorOriginal * diffuseComponent * att;
            vec3 specolor = cc * specularComponent * att;
            
            vec3 radiance = mix(difcolor2 + specolor, difcolor * meshRoughness + specolor, meshMetalness);
            
            vec3 refl = reflect(-lightRelativeToVPos2, lnormal);
            float spfsm = max(0, dot(refl, lightRelativeToVPos));
            spfsm = pow(spfsm, (255 * (1.0-lrough) + 1)) * max(0, sign(dot(lightRelativeToVPos, lnormal))) * (5-lrough);
            spfsm = cookTorranceSpecular(
            -lightRelativeToVPos2,
            -lightRelativeToVPos,
            lnormal,
            max(0.01, lrough)
            ) * 32;
            
            color1 += radiance * spfsm;
            
           // color1 += CalculateFallof(distanceToLight*6) * 2793 * cc * (1.0 - texture(HBAOTex, UV).r);
            
            
            // color1 += ((colorOriginal * (diffuseComponent * lcolor)) 
            // + (mix(colorOriginal, lcolor*colorOriginal, meshRoughness) * specularComponent))
            // * att * vi * LightsColors[i].a;   
            
        }
    
    }
    return 0.5*color1 / (RSMSamples/5);
}


void main()
{   
    if(texture(diffuseColorTex, UV).r >= 999){ 
        outColor = vec4(0,0,0,1);
        return;
    }
    vec3 color1 = vec3(0);
    
    Seed(UV+3);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    vec4 last = texture(lastIndirectTex, UV);
    vec4 ou = vec4(0);
    if(UseRSM == 1)ou = vec4(RSM(), 1);
    ou.a = texture(depthTex, UV).r;
    outColor = clamp(mix(ou, last, 0.0), 0.0, 1.0);
    
    
}