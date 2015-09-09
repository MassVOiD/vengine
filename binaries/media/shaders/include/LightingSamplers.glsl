
layout(binding = 2) uniform usampler2D lightDepth0Color;
layout(binding = 3) uniform sampler2D lightDepth0;
layout(binding = 4) uniform usampler2D lightDepth1Color;
layout(binding = 5) uniform sampler2D lightDepth1;
layout(binding = 6) uniform usampler2D lightDepth2Color;
layout(binding = 7) uniform sampler2D lightDepth2;
layout(binding = 8) uniform usampler2D lightDepth3Color;
layout(binding = 9) uniform sampler2D lightDepth3;
layout(binding = 10) uniform usampler2D lightDepth4Color;
layout(binding = 11) uniform sampler2D lightDepth4;
layout(binding = 12) uniform usampler2D lightDepth5Color;
layout(binding = 13) uniform sampler2D lightDepth5;

layout(binding = 0) uniform sampler2D currentTex;
layout(binding = 1) uniform sampler2D depthTex;

layout(binding = 14) uniform sampler2D diffuseColorTex;
layout(binding = 15) uniform sampler2D worldPosTex;
layout(binding = 16) uniform sampler2D normalsTex;
layout(binding = 17) uniform sampler2D meshDataTex;
layout(binding = 18) uniform sampler2D meshIdTex;
layout(binding = 19) uniform samplerCube cubeMapTex;
layout(binding = 20) uniform sampler2D lastIndirectTex;
layout(binding = 21) uniform sampler2D lastWorldPosTex;

layout(binding = 22) uniform sampler2D indirectTex;
layout(binding = 23) uniform sampler2D HBAOTex;
layout(binding = 24) uniform sampler2D fogTex;
layout(binding = 25) uniform sampler2D numbersTex;
layout(binding = 26) uniform sampler2D bloomTex;

layout(binding = 27) uniform sampler2D normalMapTex;
layout(binding = 28) uniform sampler2D alphaMaskTex;
layout(binding = 29) uniform sampler2D bumpMapTex;

