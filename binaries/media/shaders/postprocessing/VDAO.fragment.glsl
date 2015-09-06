#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D texDirect;
layout(binding = 1) uniform sampler2D texColor;
layout(binding = 2) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;
layout(binding = 33) uniform sampler2D meshDataTex;
layout(binding = 29) uniform samplerCube CubeMap;
out vec4 outColor;

float rand(vec2 co){
    return fract(sin(dot(co.xy,vec2(12.9898,78.233))) * 43758.5453);
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

vec3 random3dSample(){
    return normalize(vec3(
    getRand() * 2 - 1, 
    getRand() * 2 - 1, 
    getRand() * 2 - 1
    ));
}
vec2 HitPos = vec2(-1);
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(int i=0; i<11; i++){
        float ix = getRand();
        vec2 muv = mix(p1, p2, ix);
        float tmp = 
            max(
                mix(v1, v2, ix) - reverseLog(texture(sampler, muv).r),
                ret);
        if(tmp > ret) HitPos = muv;
        ret = tmp;
    }
    //if(abs(ret) > 0.1) return 0;
    return abs(ret);
}

vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}
vec2 sspace1;
mat4 PV = (ProjectionMatrix * ViewMatrix);
bool shouldBreak = false;
float testVisibility3d(vec3 w1, vec3 w2) {

    vec4 clipspace2 = (PV) *vec4(FromCameraSpace(w2), 1.0);
    if(clipspace2.z < 0) return 0;
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    vec2 dir = normalize(sspace2 - sspace1);
    //vec2 p = vec2( dot(dir, vec2(1,0)) , dot(dir, vec2(0,1)) );
    float d3d1 = (length((w1)));
    float d3d2 = (length((w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
    //float mx2 = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
    //mx = mx * (step(0, sspace1.x) + step(0, sspace1.y) + step(0, sspace2.x)+ step(0, sspace2.y) + step(0, clipspace.z) + step(0, clipspace2.z));
    return mx;
}
#include noise3D.glsl

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
#define PI 3.14159265
vec3 maphemisphere(float ex){
        vec2 samp = vec2(getRand() * 2 - 1, getRand() * 2 - 1);
		float cos_phi = cos(2.0 * PI * samp.x);
		float sin_phi = sin(2.0 * PI * samp.x);	
		float cos_theta = pow((1.0 - samp.y), 1.0 / (ex + 1.0));
		float sin_theta = sqrt (1.0 - cos_theta * cos_theta);
		float pu = sin_theta * cos_phi;
		float pv = sin_theta * sin_phi;
		float pw = cos_theta;
        return vec3(pu, pv, pw);
}
mat3 rotationMatrix(vec3 axis, float angle)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return mat3(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c);
}

#define M_PI 3.1415926535897932384626433832795

float CalculateFallof( float dist){
    //return 1.0 / pow(((dist) + 1.0), 2.0);
    return dist == 0 ? 1 : (3) / (4*M_PI*dist*dist);
    
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

uniform int UseVDAO;
uniform int UseHBAO;
vec3 Radiosity()
{
    if(texture(texColor, UV).r >= 999) return texture(CubeMap, normalize(texture(worldPosTex, UV).rgb)).rgb*1.9;
    
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    vec3 albedo = texture(texColor, UV).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    mat3 rotmat = rotationMatrix(cross(vec3(0), normalCenter), dot(normalCenter, vec3(0)));
    vec3 ambient = vec3(0);
    const int samples = 8;
    
    float octaves[] = float[4](0.8, 3.0, 7.9, 10.0);
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
    fresnel = fresnel * fresnel * fresnel + 1.0;
    
    float initialAmbient = 0.0;

    uint counter = 0;   
    float meshRoughness1 = 1.0 - texture(meshDataTex, UV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    
    vec3 colorplastic = vec3(0.851, 0.788, 0);
    vec4 clipspace = (PV) *vec4(FromCameraSpace(posCenter), 1.0);
    sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    float brfds[] = float[1]( meshRoughness1);
    for(int bi = 0; bi < brfds.length(); bi++){
        for(int i=0; i<samples; i++)
        {
                
            float meshRoughness =brfds[bi];
            vec3 displace = normalize(BRDF(dir, normalCenter, meshRoughness));
            
            vec3 color = adjustGamma(texture(CubeMap, displace).rgb, mix(0.7, 1.0, meshMetalness)) ;
            color = mix(color*albedo, color, meshMetalness);
         //   color = vec3(0.75, 0.75, 0.75);
            
            float dotdiffuse = max(0, dot(displace, normalCenter));
            /*
            //if(dotdiffuse == 0) { continue; }
            float bl = 1;
            for(int p=0;p<octaves.length();p++){
            
                float av = (testVisibility3d(posCenter, posCenter + octaves[p]*displace));
                
                bl *= (av == 0 || av > 11.9 ? 1.0 : 0);
            }
            float vis = 1;
            
            vec3 ac = texture(texColor, HitPos).rgb;
            vec3 bc = texture(CubeMap, texture(normalsTex, HitPos).xyz).xyz;
            vec3 cc = mix(ac*bc, bc, texture(meshDataTex, UV).z);
            
            
            vec3 hpwpos = texture(worldPosTex, HitPos).xyz;
            vec3 ambientcolor = (max(0, -dot(normalCenter, texture(normalsTex, HitPos).xyz)+0.6)) * cc;
            ambientcolor = mix(ambientcolor*albedo, ambientcolor, meshMetalness);
            ambient += ambientcolor * 5 * CalculateFallof(distance(posCenter, hpwpos)/3+1);*/
            
            ambient += color* dotdiffuse;
           // ambient += colorplastic*0.3* dotdiffuse * weight * (vis/1.9) * fresnel;
            
            counter++;
            
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs );
}

vec3 hbao(){
    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0, counter = 0, div = 1.0/(length(posc)+1.0);
    float octaves[] = float[3](0.5, 1.3, 3.0);
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

    return vec3(0.3)* (1.0 - min(buf / counter, 0.99));
}

vec3 pathtrace(vec3 incoming, vec2 uv, float attmod){
    if(texture(texColor, uv).r >= 999) return texture(CubeMap, normalize(texture(worldPosTex, uv).rgb)).rgb*1.9;
    
    vec3 posCenter = texture(worldPosTex, uv).rgb;
    vec3 albedo = texture(texColor, uv).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, uv).rgb);
    mat3 rotmat = rotationMatrix(cross(vec3(0), normalCenter), dot(normalCenter, vec3(0)));
    vec3 ambient = vec3(0);
    const int samples = 16;
    
    float octaves[] = float[4](0.8, 3.0, 6.0, 12.0);
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
    fresnel = fresnel * fresnel * fresnel + 1.0;
    
    float initialAmbient = 0.0;

    uint counter = 0;   
    float meshRoughness1 = 1.0 - texture(meshDataTex, uv).a;
    float meshMetalness =  texture(meshDataTex, uv).z;
    
    vec3 colorplastic = vec3(0.851, 0.788, 0);
    vec4 clipspace = (PV) *vec4(FromCameraSpace(posCenter), 1.0);
    sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    float brfds[] = float[1]( meshRoughness1);
    HitPos = vec2(-1);
    for(int bi = 0; bi < brfds.length(); bi++){
                
        float meshRoughness =brfds[bi];
        vec3 displace = BRDF(dir, normalCenter, meshRoughness);
        
        vec3 color = adjustGamma(texture(CubeMap, displace).rgb, mix(0.7, 1.0, meshMetalness)) ;
        //color = vec3(0.75, 0.75, 0.75);
        color = mix(color*albedo, color, meshMetalness);
        
        float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)));
        
        if(dotdiffuse == 0) { continue; }
        float bl = 1;
        for(int p=0;p<octaves.length();p++){
        
            float av = (testVisibility3d(posCenter, posCenter + octaves[p]*0.8*displace));
            
            bl *= av <= 0 || av > 1.9 ? 1.0 : 0.00;
        }
        float vis = 1.0-bl;
        if(HitPos.x > -.1){
            vec3 ac = texture(texDirect, HitPos).rgb;
            vec3 bc = incoming;
            vec3 cc = mix(ac, ac, texture(meshDataTex, uv).z);
            
            
            vec3 hpwpos = texture(worldPosTex, HitPos).xyz;
            vec3 ambientcolor = (max(0, -dot(normalCenter, texture(normalsTex, HitPos).xyz))) * cc;
            ambientcolor = mix(ambientcolor*albedo, ambientcolor, meshMetalness);
            ambient += attmod * dotdiffuse * ambientcolor * CalculateFallof(distance(posCenter, hpwpos));
            
            //ambient += color* dotdiffuse * (vis);
           // ambient += colorplastic*0.3* dotdiffuse * weight * (vis/1.9) * fresnel;
        }
        counter++;
        
    
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs*5);
}

vec3 pathtracestart(){
    vec3 buf = vec3(0);
    int count = 0;
    for(int i=0;i<7;i++){
        vec3 accum = vec3(0);
        vec3 color = vec3(1);
        vec3 lwpos = texture(worldPosTex, UV).rgb;
        float att = 1;
        int brk = 9;
        vec2 uv = UV;
        while(brk-- > 0){
            vec3 o = pathtrace(vec3(1), uv, att);
            accum += o;
            //color *= o;
            if(HitPos.x > -1) {
                uv = HitPos;
                att *= CalculateFallof(distance(lwpos, texture(worldPosTex, HitPos).xyz));
                lwpos = texture(worldPosTex, HitPos).xyz;
            }
        }
        count++;
        buf += accum;
    }
    return buf / count;
}

void main()
{
    Seed(UV);
    randsPointer = int(randomizer * 123.86786) % RandomsCount;
    vec3 radio = vec3(0);
    if(UseVDAO == 1)radio += Radiosity() * hbao();
    if(UseHBAO == 1)radio += hbao();
   //radio *= hbao();
    outColor = vec4(radio, 1);
}
