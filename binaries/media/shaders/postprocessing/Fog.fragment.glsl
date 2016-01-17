#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

#include noise4D.glsl

out vec4 outColor;


void main()
{
    outColor = vec4(0,0,0, texture(distanceTex, UV).r);
}