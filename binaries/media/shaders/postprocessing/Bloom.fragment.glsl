#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#define mPI (3.14159265)
#define GOLDEN_RATIO (1.6180339)

out vec4 outColor;


vec3 blurWhitening(){
    vec3 outc = vec3(0.0);
    float counter = 0;
    vec3 color = clamp(texture(currentTex, UV).rgb, 0.0, 1.0);
    float luminance = max(0.0, length(color)-112312312.9); // luminance from 1.4 to 1.7320
    //if(luminance > 1.0)
    //{
        //luminance = (luminance - 1.0) / 0.320;
        outc += color * luminance;
        //outc.a = (luminance - 1.0) / 0.320;
    //}    
    for(float g2 = -1.0; g2 < 1.0; g2+=0.02)
    { 
        vec2 gauss = vec2(g2 * 0.5, 0);
        vec3 colora = texture(currentTex, UV + gauss).rgb;
        float luminance = length(colora); // luminance from 1.4 to 1.7320
        //if(luminance > 1.2)
        //{
        //    outc += (colora * vec3(0.13, 0.13, 0.54));
        //}            
        
        gauss = vec2(0, g2 * 0.5);
        colora = texture(currentTex, UV + gauss).rgb;
        luminance = length(colora); // luminance from 1.4 to 1.7320
        //if(luminance > 1.4)
        //{
            //outc += (colora * vec3(0.13, 0.13, 0.54));
        //}            
    //    counter+=2.0;
    }    
    return color * luminance + outc / counter * 2.0;
}


void main()
{
    outColor = vec4(blurWhitening(), 1.0);
}