#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

out vec4 outColor;

float lookupAO(vec2 fuv, float radius, int samp){
     float outc = 0;
     float counter = 0;
     float depthCenter = textureMSAA(originalNormalsTex, fuv, samp).a;
 	vec3 normalcenter = textureMSAA(originalNormalsTex, fuv, samp).rgb;
     for(float g = 0; g < mPI2; g+=0.8)
     {
         for(float g2 = 0; g2 < 1.0; g2+=0.33)
         {
             vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.012 * radius);
             float color = textureLod(aoTex, fuv + gauss, 0).r;
             float depthThere = textureMSAA(originalNormalsTex, fuv + gauss, samp).a;
 			vec3 normalthere = textureMSAA(originalNormalsTex, fuv + gauss, samp).rgb;
 			float weight = pow(max(0, dot(normalthere, normalcenter)), 32);
 			outc += color * weight;
 			counter+=weight;
             
         }
     }
     return counter == 0 ? textureLod(aoTex, fuv, 0).r : outc / counter;
 }
 
void main()
{

    vec3 color = texture(lastStageResultTex, UV).rgb;
    
    float AOValue = 1.0;
    if(UseHBAO == 1) AOValue = lookupAO(UV, 1.0, 0);
    
    if(UseVXGI == 1) color += AOValue * texture(vxgiTex, UV).rgb;
    color *= Brightness;
    
    outColor = clamp(vec4(color, 1.0), 0.0, 10000.0);
}