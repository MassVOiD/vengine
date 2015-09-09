#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl

out vec4 outColor;


float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 motionBlurExperiment(vec2 uv){
    vec3 outc = texture(currentTex, uv).rgb;
    vec3 centerPos = texture(worldPosTex, uv).rgb;
    vec2 nearestUV = uv;
    float worldDistance = 999999;
    if(distance(texture(lastWorldPosTex, uv).rgb, centerPos) < 0.001) return texture(currentTex, uv).rgb;

    for(float g = 0; g < mPI2; g+=0.3)
    {
        for(float g2 = 0.1; g2 < 1.0; g2+=0.1)
        {
            vec2 dsplc = vec2(sin(g + g2)*ratio, cos(g + g2)) * g2 * (rand(UV + vec2(g,g2)) * 0.01);
            vec3 pos = texture(lastWorldPosTex, uv + dsplc).rgb;
            float ds = distance(pos, centerPos);
            if(worldDistance > ds){
                worldDistance = ds;
                nearestUV = uv + dsplc;
            }
        }
    }
    //if(distance(nearestUV, uv) < 0.001) return outc;
    int counter = 0;
    outc = vec3(0);
    vec2 direction = (nearestUV - uv)*2;
    for(float g = 0; g < 1; g+=0.1)
    {
        outc += texture(currentTex, mix(uv - direction, uv + direction, g)).rgb;
        counter++;
    }
    return outc / counter;
}

void main()
{
	gl_FragDepth = texture(depthTex, UV).r;
    outColor = vec4(texture(currentTex, UV).rgb, 1);
}