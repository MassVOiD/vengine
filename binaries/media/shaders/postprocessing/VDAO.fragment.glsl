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
vec2 HitPos = vec2(0);
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(int i=0; i<6; i++){
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

mat4 PV = (ProjectionMatrix * ViewMatrix);
bool shouldBreak = false;
float testVisibility3d(vec3 w1, vec3 w2) {
    vec4 clipspace = (PV) *vec4(FromCameraSpace(w1), 1.0);
    if(clipspace.z < 0) return 0;
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) *vec4(FromCameraSpace(w2), 1.0);
    if(clipspace2.z < 0) return 0;
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    vec2 dir = normalize(sspace2 - sspace1);
    vec2 p = vec2( dot(dir, vec2(1,0)) , dot(dir, vec2(0,1)) );
    float d3d1 = toLogDepth(length((w1)));
    float d3d2 = toLogDepth(length((w2)));
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
    return vec3pow(c, 1.0/gamma);
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
vec3 Radiosity()
{
    if(texture(texColor, UV).r >= 999) return texture(CubeMap, normalize(texture(worldPosTex, UV).rgb)).rgb*1.9;
    
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    //vec3 albedo = texture(texColor, UV).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    mat3 rotmat = rotationMatrix(cross(vec3(0), normalCenter), dot(normalCenter, vec3(0)));
    vec3 ambient = vec3(0);
    const int samples = 8;
    
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
    fresnel = fresnel * fresnel * fresnel + 1.0;
    
    float initialAmbient = 0.0;

    uint counter = 0;   
    float meshRoughness = 1.0 - texture(meshDataTex, UV).a;
    
    vec3 colorplastic = vec3(0.851, 0.788, 0);
    
    float brfds[] = float[2](0.0, 1.0);
    for(int bi = 0; bi < brfds.length(); bi++){
        for(int i=0; i<samples; i++)
        {
            float weight = 0.8;
                
            //float meshRoughness =brfds[bi];
            vec3 displace = random3dSample();
           vec3 displace2 = displace * sign(dot(normalCenter, displace));
            float dotdist = max(meshRoughness, meshRoughness * dot(displace2, dir));
            
            displace = mix(displace2, dir, dotdist * meshRoughness);
            displace = mix(displace, dir, meshRoughness);
            displace *= sign(dot(normalCenter, displace));
            
            vec3 color = adjustGamma(texture(CubeMap, displace).rgb, mix(1.0, 0.3, meshRoughness)) ;
            //color = mix(color);
            
            float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)) + 0.8);
            
            if(dotdiffuse == 0) { continue; }
            
            float av = reverseLog(testVisibility3d(posCenter, posCenter + 5*displace/(length(posCenter)*0.0001+1.0)));
            
            float vis = av <= 0 || av > 22.9 ? 1.9 :  0;
            
            vec3 hpwpos = texture(worldPosTex, HitPos).xyz;
            vec3 ambientcolor = (-dot(normalCenter, texture(normalsTex, HitPos).xyz)+0.6) * texture(texColor, HitPos).rgb * texture(CubeMap, texture(normalsTex, HitPos).xyz).rgb;
            if(vis == 0) ambient += ambientcolor * ((1.0-distance(UV, HitPos))*10/(length(posCenter)+1))*1.6;
            
            ambient += color* dotdiffuse * weight * (vis) * fresnel;
           // ambient += colorplastic*0.3* dotdiffuse * weight * (vis/1.9) * fresnel;
            
            counter++;
            
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs);
}

vec3 hbao(){
    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0, counter = 0, div = 1.0/(length(posc)+1.0);
    for(float g = 0; g < mPI2; g+=0.52){
        for(float g2 = 0.1; g2 < 1.0; g2+=0.29){
            vec3 pos = texture(worldPosTex,  UV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (getRand() * 4.09)) * div).rgb;
            float skip = max(0, sign(length(posc) - length(pos)));
            buf += skip * (dot(norm, normalize(pos - posc))) * max(0, (10.0 - length(pos - posc))/10.0);
            counter+=0.3;
        }
    }
    return vec3(1) * (1.0 - min(buf / counter, 0.9));
}

void main()
{
    Seed(UV);
    randsPointer = int(randomizer * 123.86786) % RandomsCount;
    vec3 radio = Radiosity();
    outColor = vec4(radio, 1);
}
