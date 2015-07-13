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
out vec4 outColor;

float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(float i=0;i<1;i+=0.1) 
        ret = max(mix(v1, v2, i) - texture(sampler, mix(p1, p2, i)).r, ret);
        
    return ret;
}

vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}

mat4 PV = (ProjectionMatrix * ViewMatrix);
float testVisibility3d(vec3 w1, vec3 w2) {
    vec4 clipspace = (PV) * vec4(FromCameraSpace(w1), 1.0);
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) * vec4(FromCameraSpace(w2), 1.0);
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    float d3d1 = toLogDepth(length((w1)));
    float d3d2 = toLogDepth(length((w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
    //float mx2 = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));

    return mx;
}
float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float Radiosity() 
{    

    vec3 posCenter = texture(worldPosTex, UV).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    float ambient = 0;
    const int samples = 64;
    const int octaves = 2;
    
    // choose between noisy and slower, but better looking variant
    //float randomizer = 138.345341 * rand(UV) + RandomSeed2;
    // or faster, non noisy variant, which is also cool looking
    const float randomizer = 138.345341;
    
    float initialAmbient = 0.01;
    
    uint counter = 0;
    
    for(int i=0;i<samples;i++)
    {
        float rd = randomizer * float(i);
        float weight = 1;
        vec3 displace = (vec3(
            (fract(rd) * 2 - 1) * 0.4, 
            fract(rd*12.2562), 
            (fract(rd*7.121214) * 2 - 1) * 0.4
        ))  * 1.3;
        float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)));
        //if(dotdiffuse == 0) { counter+=octaves;continue; }
        //for(int div = 0;div < octaves; div++)
        //{
            float vis = max(0, 1.0 - testVisibility3d(posCenter, posCenter + displace)*123);
            
            ambient += dotdiffuse * weight * vis;
            // else { counter += octaves - div; break; }
            //displace = displace * 1.94;
            //weight = weight * 0.47;
            counter++;
       // }
    }
    float rs = counter == 0 ? 0 : (ambient / (counter));
    return (rs + initialAmbient);
}
void main()
{   
    float radio = Radiosity();
    outColor = vec4(radio, radio, radio, 1);
}