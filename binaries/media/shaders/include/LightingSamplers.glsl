layout(binding = 0) uniform sampler2D currentTex;
#ifdef USE_MSAA
layout(binding = 30) uniform sampler2DMS diffuseColorTex;
#else
layout(binding = 30) uniform sampler2D diffuseColorTex;
#endif
layout(binding = 3) uniform samplerCube cubeMapTex;
layout(binding = 4) uniform sampler2D lastIndirectTex;
layout(binding = 5) uniform sampler2D fogTex;

#define bumpMapTex lastIndirectTex
#define metalnessMapTex fogTex
#define edgesTex roughnessMapTex

layout(binding = 27) uniform sampler2D numbersTex;
#define aoTex numbersTex
#define distanceTex aoTex
layout(binding = 29) uniform sampler2D normalMapTex;
layout(binding = 28) uniform sampler2D roughnessMapTex;

layout(binding = 6) uniform sampler2DShadow lightDepth0;
layout(binding = 7) uniform sampler2DShadow lightDepth1;
layout(binding = 8) uniform sampler2DShadow lightDepth2;
layout(binding = 9) uniform sampler2DShadow lightDepth3;
layout(binding = 10) uniform sampler2DShadow lightDepth4;
layout(binding = 11) uniform sampler2DShadow lightDepth5;
layout(binding = 12) uniform sampler2DShadow lightDepth6;
layout(binding = 13) uniform sampler2DShadow lightDepth7;
layout(binding = 14) uniform sampler2DShadow lightDepth8;
layout(binding = 15) uniform sampler2DShadow lightDepth9;
layout(binding = 16) uniform sampler2DShadow lightDepth10;
layout(binding = 17) uniform sampler2DShadow lightDepth11;
layout(binding = 18) uniform sampler2DShadow lightDepth12;
layout(binding = 19) uniform sampler2DShadow lightDepth13;
layout(binding = 20) uniform sampler2DShadow lightDepth14;
layout(binding = 21) uniform sampler2DShadow lightDepth15;
layout(binding = 22) uniform sampler2DShadow lightDepth16;
layout(binding = 23) uniform sampler2DShadow lightDepth17;
layout(binding = 24) uniform sampler2DShadow lightDepth18;
layout(binding = 25) uniform sampler2DShadow lightDepth19;
layout(binding = 26) uniform sampler2DShadow lightDepth20;
#ifdef USE_MSAA
int getMSAASamples(vec2 uv){
    float edge = texture(edgesTex, uv).r;
    return MSAA_SAMPLES;//int(mix(1, MSAASamples, edge));
}

ivec2 txsize = textureSize(diffuseColorTex);
vec4 textureMSAAFull(sampler2DMS tex, vec2 inUV){
    vec4 color11 = vec4(0.0);
    int samples = getMSAASamples(inUV);
    ivec2 texcoord = ivec2(vec2(txsize) * inUV); 
    for (int i=0;i<samples;i++)
    {
        color11 += texelFetch(tex, texcoord, i);  
    }

    color11/= samples; 
    return color11;
}
vec4 textureMSAA(sampler2DMS tex, vec2 inUV, int samplee){
    ivec2 texcoord = ivec2(vec2(txsize) * inUV);
    return texelFetch(tex, texcoord, samplee);  
}
#else
int getMSAASamples(vec2 uv){
    return 1;
}
vec4 textureMSAAFull(sampler2D tex, vec2 inUV){
    return texture(tex, inUV);
}
vec4 textureMSAA(sampler2D tex, vec2 inUV, int samplee){
    return texture(tex, inUV); 
}
#endif
