#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

#include noise4D.glsl

out vec4 outColor;

float GetFrequency(sampler2DMS sampler){
    ivec3 pix = ivec3(1, 1, 0);// trick
    ivec2 pix2 = ivec2(pix.x, -pix.y);
    float f = 0;
    ivec2 iUV = ivec2(UV * textureSize(sampler));
    vec4 center = texelFetch(sampler, iUV, 0);
    
    f += distance(center, texelFetch(sampler, iUV + pix.xz, 0));
    f += distance(center, texelFetch(sampler, iUV - pix.xz, 0));
    
    f += distance(center, texelFetch(sampler, iUV + pix.zy, 0));
    f += distance(center, texelFetch(sampler, iUV - pix.zy, 0));
    
    f += distance(center, texelFetch(sampler, iUV + pix.xy, 0));
    f += distance(center, texelFetch(sampler, iUV - pix.xy, 0));
    
    f += distance(center, texelFetch(sampler, iUV + pix.xy, 0));
    f += distance(center, texelFetch(sampler, iUV - pix.xy, 0));
    
    f += distance(center, texelFetch(sampler, iUV + pix2.xy, 0));
    f += distance(center, texelFetch(sampler, iUV - pix2.xy, 0));
    
    f += distance(center, texelFetch(sampler, iUV + pix2.xy, 0));
    f += distance(center, texelFetch(sampler, iUV - pix2.xy, 0));
    
    return f / 12.0;
}

float GetFrequencyRed(sampler2DMS sampler){
    ivec3 pix = ivec3(1, 1, 0);// trick
    ivec2 pix2 = ivec2(pix.x, -pix.y);
    float f = 0;
    ivec2 iUV = ivec2(UV * textureSize(sampler));
    float center = texelFetch(sampler, iUV, 0).r;
    
    f += abs(center - texelFetch(sampler, iUV + pix.xz, 0).r);
    f += abs(center - texelFetch(sampler, iUV - pix.xz, 0).r);
                    
    f += abs(center - texelFetch(sampler, iUV + pix.zy, 0).r);
    f += abs(center - texelFetch(sampler, iUV - pix.zy, 0).r);
                    
    f += abs(center - texelFetch(sampler, iUV + pix.xy, 0).r);
    f += abs(center - texelFetch(sampler, iUV - pix.xy, 0).r);
                    
    f += abs(center - texelFetch(sampler, iUV + pix.xy, 0).r);
    f += abs(center - texelFetch(sampler, iUV - pix.xy, 0).r);
                    
    f += abs(center - texelFetch(sampler, iUV + pix2.xy, 0).r);
    f += abs(center - texelFetch(sampler, iUV - pix2.xy, 0).r);
                    
    f += abs(center - texelFetch(sampler, iUV + pix2.xy, 0).r);
    f += abs(center - texelFetch(sampler, iUV - pix2.xy, 0).r);
    
    return f*10.0;
}

void main()
{
    #ifdef USE_MSAA
    float h = GetFrequency(normalsTex)*1.1;// + GetFrequencyRed(depthTex);
    #else
    float h = 0;
    #endif
    outColor = vec4(clamp(h*0.1, 0.0, 1.0));
}