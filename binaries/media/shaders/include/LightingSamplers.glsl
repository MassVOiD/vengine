layout(binding = 0) uniform sampler2D currentTex;
layout(binding = 1) uniform sampler2D depthTex;

layout(binding = 2) uniform sampler2D lightDepth0;
layout(binding = 3) uniform sampler2D lightDepth1;
layout(binding = 4) uniform sampler2D lightDepth2;
layout(binding = 5) uniform sampler2D lightDepth3;
layout(binding = 6) uniform sampler2D lightDepth4;
layout(binding = 7) uniform sampler2D lightDepth5;


layout(binding = 14) uniform sampler2D diffuseColorTex;
layout(binding = 16) uniform sampler2D normalsTex;
layout(binding = 17) uniform sampler2D meshDataTex;
layout(binding = 18) uniform usampler2D meshIdTex;
layout(binding = 19) uniform samplerCube cubeMapTex;
layout(binding = 20) uniform sampler2D lastIndirectTex;
//layout(binding = 21) uniform sampler2D lastWorldPosTex;

layout(binding = 22) uniform sampler2D indirectTex;
layout(binding = 23) uniform sampler2D HBAOTex;
layout(binding = 24) uniform sampler2D fogTex;
layout(binding = 25) uniform sampler2D numbersTex;
layout(binding = 26) uniform sampler2D bloomTex;

layout(binding = 27) uniform sampler2D normalMapTex;
layout(binding = 28) uniform sampler2D alphaMaskTex;
layout(binding = 29) uniform sampler2D bumpMapTex;
layout(binding = 30) uniform sampler2D roughnessMapTex;
layout(binding = 31) uniform sampler2D metalnessMapTex;
layout(binding = 32) uniform sampler2D specularMapTex;

