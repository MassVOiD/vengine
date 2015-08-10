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
layout(binding = 29) uniform samplerCube CubeMap;
out vec4 outColor;

float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
        float ret = 0;
        for(float i=0; i<1; i+=0.1)
                ret = max(mix(v1, v2, i) - texture(sampler, mix(p1, p2, i)).r, ret);
        if(abs(ret) > 0.1) return 0;
        return clamp(abs(ret) - 0.0006, 0, 1);
}

vec2 saturatev2(vec2 v){
        return clamp(v, 0.0, 1.0);
}

mat4 PV = (ProjectionMatrix * ViewMatrix);
float testVisibility3d(vec3 w1, vec3 w2) {
        vec4 clipspace = (PV) *vec4(FromCameraSpace(w1), 1.0);
        vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
        vec4 clipspace2 = (PV) *vec4(FromCameraSpace(w2), 1.0);
        vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
        float d3d1 = toLogDepth(length((w1)));
        float d3d2 = toLogDepth(length((w2)));
        float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
        //float mx2 = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
        //mx = mx * (step(0, sspace1.x) + step(0, sspace1.y) + step(0, sspace2.x)+ step(0, sspace2.y) + step(0, clipspace.z) + step(0, clipspace2.z));
        return mx;
}
float rand(vec2 co){
        return fract(sin(dot(co.xy,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 Radiosity()
{

        vec3 posCenter = texture(worldPosTex, UV).rgb;
        vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
        vec3 ambient = vec3(0);
        const int samples = 33;
        const int octaves = 4;
        vec3 dir = reflect(posCenter, normalCenter);

        // choose between noisy and slower, but better looking variant
        float randomizer = 138.345341 * rand(UV) + RandomSeed2;
        // or faster, non noisy variant, which is also cool looking
        //const float randomizer = 138.345341;

        float initialAmbient = 0.01;

        uint counter = 0;
        float meshRoughness = texture(meshDataTex, UV).a;

        for(int i=0; i<samples; i++)
        {
                float rd = randomizer * float(i) * 12.1125345;
                float weight = 0.3;
                vec3 displace = (vec3(
                                         (fract(rd)),
                                         fract(rd*12.2562),
                                         (fract(rd*7.121214) * 2 - 1)
                                         )) ;
                displace = mix(displace, dir,0)*0.1;
                 vec3 color = texture(CubeMap, normalize(displace)).rgb;
                float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)) + 0.8)*2;
                //if(dotdiffuse == 0) { counter+=octaves;continue; }
                for(int div = 0; div < octaves; div++)
                {
                        float vis = testVisibility3d(posCenter, posCenter + displace) == 0 ? 1 : 0;

                        ambient += color* dotdiffuse * weight * vis;
                        // else { counter += octaves - div; break; }
                        displace = displace * 3.94;
                        weight = weight * 1.47;
                        counter++;
                }
        }
        vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
        return (rs + initialAmbient);
}
void main()
{
        vec3 radio = Radiosity();
        outColor = vec4(radio, 1);
}
