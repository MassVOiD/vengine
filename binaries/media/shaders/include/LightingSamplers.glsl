// let get it done well this time
layout(binding = 0) uniform sampler2DArrayShadow shadowMapsArray;
#ifdef USE_MSAA
layout(binding = 1) uniform sampler2DMS forwardOutputTex;
#else
layout(binding = 1) uniform sampler2D forwardOutputTex;
#endif
layout(binding = 2) uniform sampler2D normalsTex;
#define bloomMidPassTex normalsTex
layout(binding = 3) uniform sampler2D bumpTex;
layout(binding = 4) uniform sampler2D alphaTex;
layout(binding = 5) uniform sampler2D diffuseTex;
layout(binding = 6) uniform sampler2D specularTex;
layout(binding = 7) uniform sampler2D roughnessTex;

layout(binding = 8) uniform sampler2D distanceTex;


layout(binding = 9) uniform samplerCube cubeMapTex1;

layout(binding = 10) uniform samplerCube cubeMapTex2;
layout(binding = 11) uniform samplerCube cubeMapTex3;
layout(binding = 12) uniform samplerCube cubeMapTex4;
layout(binding = 13) uniform samplerCube cubeMapTex5;
layout(binding = 14) uniform samplerCube cubeMapTex6;
layout(binding = 15) uniform samplerCube cubeMapTex7;
layout(binding = 16) uniform samplerCube cubeMapTex8;
layout(binding = 17) uniform samplerCube cubeMapTex9;
layout(binding = 18) uniform samplerCube cubeMapTex10;

uniform int CubeMapsCount;
uniform vec4 CubeMapsPositions[10];
uniform vec4 CubeMapsFalloffs[10];


#ifdef USE_MSAA

ivec2 txsize = textureSize(forwardOutputTex);
vec4 textureMSAAFull(sampler2DMS tex, vec2 inUV){
    vec4 color11 = vec4(0.0);
    ivec2 texcoord = ivec2(vec2(txsize) * inUV); 
    for (int i=0;i<MSAA_SAMPLES;i++)
    {
        color11 += texelFetch(tex, texcoord, i);  
    }

    color11/= MSAA_SAMPLES; 
    return color11;
}
vec4 textureMSAA(sampler2DMS tex, vec2 inUV, int samplee){
    ivec2 texcoord = ivec2(vec2(txsize) * inUV);
    return texelFetch(tex, texcoord, samplee);  
}

#else

ivec2 txsize = textureSize(forwardOutputTex, 0);
vec4 textureMSAAFull(sampler2D tex, vec2 inUV){
    return texture(tex, inUV);
}
vec4 textureMSAA(sampler2D tex, vec2 inUV, int samplee){
    return texture(tex, inUV); 
}
#endif
