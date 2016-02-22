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

//layout(binding = 2) uniform sampler2D normalsTex;
//#define bloomMidPassTex normalsTex
#extension GL_ARB_bindless_texture : require
//#extension AMD_gpu_shader_int64 : require
//layout(binding = 5) uniform sampler2D diffuseTex;
uniform uvec2 bumpTexAddr;
#define bumpTex sampler2D(bumpTexAddr)

uniform uvec2 alphaTexAddr;
#define alphaTex sampler2D(alphaTexAddr)

uniform uvec2 diffuseTexAddr;
#define diffuseTex sampler2D(diffuseTexAddr)

uniform uvec2 normalsTexAddr;
#define normalsTex sampler2D(normalsTexAddr)

uniform uvec2 specularTexAddr;
#define specularTex sampler2D(specularTexAddr)

uniform uvec2 roughnessTexAddr;
#define roughnessTex sampler2D(roughnessTexAddr)

#define bloomMidPassTex sampler2D(roughnessTexAddr)

layout(binding = 8) uniform sampler2D distanceTex;
/*

layout(binding = 9)  uniform samplerCube cubeMapTex1;
layout(binding = 10) uniform samplerCube cubeMapTex2;
layout(binding = 11) uniform samplerCube cubeMapTex3;
layout(binding = 12) uniform samplerCube cubeMapTex4;
layout(binding = 13) uniform samplerCube cubeMapTex5;
layout(binding = 14) uniform samplerCube cubeMapTex6;
layout(binding = 15) uniform samplerCube cubeMapTex7;
layout(binding = 16) uniform samplerCube cubeMapTex8;
layout(binding = 17) uniform samplerCube cubeMapTex9;
layout(binding = 18) uniform samplerCube cubeMapTex10;
layout(binding = 19) uniform samplerCube cubeMapTex11;
layout(binding = 20) uniform samplerCube cubeMapTex12;
layout(binding = 21) uniform samplerCube cubeMapTex13;
layout(binding = 22) uniform samplerCube cubeMapTex14;
layout(binding = 23) uniform samplerCube cubeMapTex15;
layout(binding = 24) uniform samplerCube cubeMapTex16;
layout(binding = 25) uniform samplerCube cubeMapTex17;
layout(binding = 26) uniform samplerCube cubeMapTex18;
layout(binding = 27) uniform samplerCube cubeMapTex19;
layout(binding = 28) uniform samplerCube cubeMapTex20;
layout(binding = 29) uniform samplerCube cubeMapTex21;
layout(binding = 30) uniform samplerCube cubeMapTex22;
layout(binding = 31) uniform samplerCube cubeMapTex23;
*/
uniform int CubeMapsCount;
uniform vec4 CubeMapsPositions[233];
uniform vec4 CubeMapsFalloffs[233];
uniform uvec2 CubeMapsAddrs[233];


#ifdef USE_MSAA

ivec2 txsize = textureSize(albedoRoughnessTex);
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

ivec2 txsize = textureSize(albedoRoughnessTex, 0);
vec4 textureMSAAFull(sampler2D tex, vec2 inUV){
    return texture(tex, inUV);
}
vec4 textureMSAA(sampler2D tex, vec2 inUV, int samplee){
    return texture(tex, inUV); 
}
#endif
