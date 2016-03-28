#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

out vec4 outColor;

void main()
{

    vec3 color = texture(lastStageResultTex, UV).rgb;
    if(UseSSReflections == 1) color += texture(ssRefTex, UV).rgb;
    
    outColor = clamp(vec4(color, 1.0), 0.0, 10000.0);
}