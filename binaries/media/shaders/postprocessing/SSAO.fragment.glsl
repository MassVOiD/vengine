#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

out vec2 outColor;

layout (std430, binding = 6) buffer RandomsBuffer
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand2s(seeder)) ;
}

float randsPointer = 0;
uniform int RandomsCount;

float getRand2(){
    float r = rand2s(vec2(randsPointer, randsPointer*2.42354 + Time));
    randsPointer+=1.23456;
    //if(randsPointer >= 1234.123123) randsPointer = 0.0;
    return r;
}

vec3 random3dSample(){
    return normalize(vec3(
        getRand2() * 2 - 1, 
        getRand2() * 2 - 1, 
        getRand2() * 2 - 1
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
vec2 projectOnScreen(vec3 worldcoord){
    vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(worldcoord, 1.0);
    vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
    if(clipspace.z < 0.0) return vec2(-1);
    return sspace1;
}
uniform int UseHBAO;
float hbao(){

    // gather data
    uint idvals = texture(meshIdTex, UV).g;
    /*
    uint packpart1 = packUnorm4x8(vec4(AORange, AOStrength, AOAngleCutoff, SubsurfaceScatteringMultiplier));
    uint packpart2 = packUnorm4x8(vec4(VDAOMultiplier, VDAOSamplingMultiplier, VDAORefreactionMultiplier, 0));
    */
    vec4 vals = unpackUnorm4x8(idvals);
    float aorange = vals.x * 2 + 0.1;
    float aostrength = vals.y * 4 + 0.1;
    float aocutoff = 1.0 - vals.z;
    
    

    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0.0, div = 1.0/(length(posc)+1.0);
    float counter = 0.0;
    vec3 dir = normalize(reflect(posc, norm));
    float meshRoughness = 1.0 - texture(meshDataTex, UV).a;
    float samples = mix(12.0, 1.0, meshRoughness);
    const float ringsize = 0.9;
    for(float g = 0.0; g < samples; g+=1)
    {
        float minang = 0;
        vec3 displace = normalize(BRDF(dir, norm, meshRoughness)) * ringsize;
        vec2 sspos2 = projectOnScreen(FromCameraSpace(posc) + displace);
        vec2 diruv = normalize(sspos2 - UV);
        //sspos2 = UV + diruv*0.4;
        for(float g2 = 0.01; g2 < 1.0; g2+=0.2)
        {
            vec2 gauss = mix(UV, sspos2, getRand2());
            vec3 pos = texture(worldPosTex,  gauss).rgb;
            float dt = max(0, dot(norm, normalize(pos - posc)));
            minang = max(dt * max(0, (ringsize - length(pos - posc))/ringsize), minang);
        }
        if(minang > aocutoff) minang = 1;
        buf += minang;
        counter+=1.0;
    }
    return pow(1.0 - (buf/counter), aostrength);
}

uniform float AOGlobalModifier;
void main()
{   
    vec3 color1 = vec3(0);
    
    Seed(UV+1);
    randsPointer = float(randomizer * 113.86786 );
    vec2 au = vec2(0);
    if(UseHBAO == 1){
       // if(UV.x < 0.5) au = vec4(hbao(), 0, 0, 1);
       //else au = vec4(Radiosity(), 0, 0, 1);
       au = vec2(pow(hbao(), AOGlobalModifier), texture(depthTex, UV).r);
    }
    outColor = clamp(au.rg, 0.0, 1.0);
    
    
}