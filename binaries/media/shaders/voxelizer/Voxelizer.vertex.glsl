#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl


uniform vec3 CenterToZero;
uniform float NormalizationDivisor;

out vec4 Position;

void main(){
    vec4 v = vec4(in_position,1);
   // v.xyz *= NormalizationDivisor;
   // v.xyz -= CenterToZero * NormalizationDivisor;
    Position = v;

    gl_Position = vec4(v.xyz, 1);
}