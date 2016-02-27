// let get it done well this time
layout(binding = 0) uniform sampler2DArray shadowMapsArray;
#ifdef USE_MSAA
layout(binding = 1) uniform sampler2DMS albedoRoughnessTex;
layout(binding = 2) uniform sampler2DMS normalsDistancetex;
layout(binding = 3) uniform sampler2DMS specularBumpTex;
#else
layout(binding = 1) uniform sampler2D albedoRoughnessTex;
layout(binding = 2) uniform sampler2D normalsDistancetex;
layout(binding = 3) uniform sampler2D specularBumpTex;
#endif

layout(binding = 4) uniform sampler2D lastStageResultTex;
layout(binding = 5) uniform sampler2DArray shadowMapsColorsArray;
layout(binding = 6) uniform sampler2DArray shadowMapsNormalsArray;


layout(binding = 7) uniform sampler2D deferredTex;

layout(binding = 9) uniform sampler2D ssRefTex;

layout(binding = 8) uniform sampler2D distanceTex;

uniform int CubeMapsCount;
uniform vec4 CubeMapsPositions[233];
uniform vec4 CubeMapsFalloffs[233];
uniform uvec2 CubeMapsAddrs[233];


#ifdef USE_MSAA

ivec2 txsize = textureSize(albedoRoughnessTex);
float MSAADifference(sampler2DMS tex, vec2 inUV){
    ivec2 texcoord = ivec2(vec2(txsize) * inUV); 
    vec4 color11 = texelFetch(tex, texcoord, 0);
    float diff = 0;
    for (int i=1;i<MSAA_SAMPLES;i++)
    {
        vec4 color2 = texelFetch(tex, texcoord, i);
        diff += distance(color11, color2);  
        color11 = color2;
    }
    return diff;
}

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

float MSAADifference(sampler2D tex, vec2 inUV){
    return 0;
}

ivec2 txsize = textureSize(albedoRoughnessTex, 0);
vec4 textureMSAAFull(sampler2D tex, vec2 inUV){
    return texture(tex, inUV);
}
vec4 textureMSAA(sampler2D tex, vec2 inUV, int samplee){
    return texture(tex, inUV); 
}
#endif
