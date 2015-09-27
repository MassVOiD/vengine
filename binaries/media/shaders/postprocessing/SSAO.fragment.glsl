#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

out vec4 outColor;

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

uniform int UseHBAO;
float hbao(){
    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0, counter = 0, div = 1.0/(length(posc)+1.0);
    float octaves[] = float[](0.8, 3.8);
    float roughness =  1.0-texture(meshDataTex, UV).a;
    for(int p=0;p<octaves.length();p++){
        for(float g = 0; g < mPI2; g+=0.3){
           // float rda = getRand() * mPI2;
            vec3 pos = texture(worldPosTex,  UV + (vec2(sin(g)*ratio, cos(g)) * (getRand() * octaves[p])) * div).rgb;
            float dt = 1.0 - min(1, max(0, dot(norm, normalize(pos - posc)) - 0.5));
           // float dt = 1.0 - min(1, max(0, dot(norm, normalize(pos - posc))));
            
          //  dt = pow(dt*0.95, roughness * 22 + 1);
            
            buf += max(0, sign(length(posc) - length(pos)))
            * (1.0 - dt)
            * max(0, (3.0 - length(pos - posc))/10.0);
            counter+=0.02;
        }
    }

    return pow(1.0 - buf / counter, 1.1);
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
float hitposMixPrecentage = 0;
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(int i=0; i<2; i++){
        float ix = getRand();
        vec2 muv = mix(p1, p2, ix);
        float expr = min(muv.x, muv.y) * -(max(muv.x, muv.y)-1.0);
        float tmp = min(0, sign(expr)) * 
            ret + 
            min(0, -sign(expr)) * 
            max(
                mix(v1, v2, ix) - texture(sampler, muv).r,
                ret);
        if(tmp > ret) {HitPos = muv; hitposMixPrecentage = ix;}
        ret = tmp;
    }
    //if(abs(ret) > 0.1) return 0;
    return abs(ret) - 0.0006;
}
vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}
mat4 PV = (ProjectionMatrix * ViewMatrix);
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
float Radiosity()
{
    
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    float meshSpecular = texture(worldPosTex, UV).a;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    float ambient = 0;
    const int samples = 66;
    
    float octaves[] = float[](0.1, 0.5, 1.9);
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
    
    float initialAmbient = 0.0;

    float counter = 0;   
    float meshRoughness1 = 1.0 - texture(meshDataTex, UV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    fresnel = fresnel * fresnel * fresnel*(1.0-meshMetalness)*(meshRoughness1)*0.4 + 1.0;
    
    float brfds[] = float[2](min(meshMetalness, meshRoughness1), meshRoughness1);
    int smp = brfds.length() * octaves.length() * samples;
    for(int bi = 0; bi < brfds.length(); bi++)
    {
        for(int p=0; p<octaves.length(); p++)
        {
            for(int i=0; i<samples; i++)
            {
                float meshRoughness =brfds[bi];
                vec3 displace = normalize(BRDF(dir, normalCenter, meshRoughness));
                
                float vi = testVisibility3d(UV, FromCameraSpace(posCenter), FromCameraSpace(posCenter) + displace*octaves[p]);
                vi = HitPos.x > 0  && reverseLog(vi) < 0.1 ? hitposMixPrecentage : 1;
                
               // float dotdiffuse = max(0, dot(displace, normalCenter));
                ambient += vi;
            }
        }
    }
    float rs = ambient / smp;
    return pow(rs, 13.6);
}
void main()
{   
    vec3 color1 = vec3(0);
    
    Seed(UV+1);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    vec4 au = vec4(0);
    if(UseHBAO == 1){
        au = vec4(Radiosity(), 0, 0, 1);
    }
    au.b = 0.5;
    au.a = texture(depthTex, UV).r;
    outColor = clamp(au.rgba, 0.0, 1.0);
    
    
}