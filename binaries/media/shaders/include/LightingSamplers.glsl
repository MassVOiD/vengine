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

layout(binding = 6) uniform sampler2DArrayShadow shadowMapsArray;
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
