#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

uniform int UseRSM;

float meshRoughness;
float meshSpecular;
float meshDiffuse;

uniform float VDAOGlobalMultiplier;


out vec4 outColor;

#include EnvironmentLight.glsl
#include Direct.glsl


uniform int UseVDAO;
uniform int UseHBAO;

void main()
{   

    outColor = vec4(0);
    
    
}