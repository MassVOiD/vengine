#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

out vec4 outColor;

layout (std430, binding = 6) buffer RandomsBuffer
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand2s(seeder)) ;
}

int randsPointer = 0;
uniform int RandomsCount;
float getRand(){
    float r = Randoms[randsPointer];
    randsPointer++;
    if(randsPointer >= RandomsCount) randsPointer = 0;
    return r;
}

uniform int UseHBAO;
float hbao(){
    vec3 posc = texture(worldPosTex, UV).rgb;
    vec3 norm = texture(normalsTex, UV).rgb;
    float buf = 0, counter = 0, div = 1.0/(length(posc)+1.0);
    float octaves[] = float[2](0.5, 2.0);
    float roughness =  1.0-texture(meshDataTex, UV).a;
    for(int p=0;p<octaves.length();p++){
        for(float g = 0; g < mPI2; g+=0.4){
           // float rda = getRand() * mPI2;
            vec3 pos = texture(worldPosTex,  UV + (vec2(sin(g)*ratio, cos(g)) * (getRand() * octaves[p])) * div).rgb;
            buf += max(0, sign(length(posc) - length(pos)))
            * (max(0, 1.0-pow(1.0-max(0, dot(norm, normalize(pos - posc))), (roughness)*26+1)))
            * max(0, (6.0 - length(pos - posc))/10.0);
            counter+=0.4;
        }
    }

    return pow(1.0 - buf / counter, 1.2);
}

void main()
{   
    vec3 color1 = vec3(0);
    
    Seed(UV+2);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    vec4 ou = vec4(0);
    if(UseHBAO == 1){
        ou = vec4(hbao(), 0, 0, 1);
    }
    outColor = clamp(ou, 0.0, 1.0);
    
    
}