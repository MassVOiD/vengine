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
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
        float ret = 0;
        for(int i=0; i<14; i++){
            float ix = getRand();
            vec2 muv = mix(p1, p2, ix);
            float expr = min(muv.x, muv.y) * -(max(muv.x, muv.y)-1.0);
            ret = min(0, sign(expr)) * ret + min(0, -sign(expr)) * (max(mix(v1, v2, ix) - texture(sampler, muv).r, ret));
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
vec3 Radiosity()
{
       if(texture(texColor, UV).r >= 999) return texture(CubeMap, normalize(texture(worldPosTex, UV).rgb)).rgb;
        vec3 posCenter = texture(worldPosTex, UV).rgb;
        vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
        vec3 ambient = vec3(0);
        const int samples = 0;
        const int octaves = 4;
        
        vec3 dir = reflect(posCenter, normalCenter);
        float fresnel = 1.0 - max(0, dot(-normalize(posCenter), normalize(normalCenter)));
        fresnel = fresnel * fresnel * fresnel + 1.0;
        // choose between noisy and slower, but better looking variant         
        Seed(UV);
        randsPointer = int(randomizer * 123.86786) % RandomsCount;
        // or faster, non noisy variant, which is also cool looking
        //const float randomizer = 138.345341;

        float initialAmbient = 0.01;

        uint counter = 0;   
        //float meshRoughness = texture(meshDataTex, UV).a;
        float meshRoughness =0.0;

        for(int i=0; i<samples; i++)
        {
                //randsPointer = int(i*4 * 123.86786) % RandomsCount;
               // float rd = randomizer * float(i) * 12.1125345;
                float weight = 0.8;
                vec3 displace = random3dSample();
                vec3 displace2 = displace * sign(dot(normalCenter, displace));
                float dotdist = max(meshRoughness, meshRoughness * dot(displace2, dir));
                
                displace = mix(displace2, dir, dotdist * meshRoughness);
                displace = mix(displace, dir, meshRoughness);
                displace *= sign(dot(normalCenter, displace));
                 vec3 color =texture(CubeMap, displace).rgb;
                 vec3 color2 =texture(texColor, UV).rgb;
                 color = mix(color2 * color, color, meshRoughness);
                float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)) + 0.8);
                //if(dotdiffuse == 0) { counter+=octaves;continue; }
                float av = reverseLog(testVisibility3d(posCenter, posCenter + displace*length(posCenter)*0.2));
        
                //if(av > 0 && av < 1.9) break;
                float vis = av <= 0 || av > 1.9 ? 1.9 :  0;
                
                ambient += color* dotdiffuse * weight * (vis/1.9) * fresnel;
                // else { counter += octaves - div; break; }
                //displace = displace * 1.94;
                counter++;
                
        }
        vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
        return (rs + initialAmbient);
}
void main()
{
        vec3 radio = Radiosity();
        outColor = vec4(radio, 1);
}
