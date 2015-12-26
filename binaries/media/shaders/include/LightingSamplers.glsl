layout(binding = 0) uniform sampler2D currentTex;
layout(binding = 1) uniform sampler2D depthTex;
layout(binding = 2) uniform sampler2D normalsTex;
layout(binding = 3) uniform samplerCube cubeMapTex;
layout(binding = 4) uniform sampler2D lastIndirectTex;
layout(binding = 5) uniform sampler2D fogTex;

#define diffuseColorTex currentTex
#define numbersTex normalsTex
#define normalMapTex normalsTex
#define bumpMapTex lastIndirectTex
#define aoTex lastIndirectTex
#define metalnessMapTex fogTex
#define roughnessMapTex depthTex

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
layout(binding = 27) uniform sampler2DShadow lightDepth21;
layout(binding = 28) uniform sampler2DShadow lightDepth22;
layout(binding = 29) uniform sampler2DShadow lightDepth23;
layout(binding = 30) uniform sampler2DShadow lightDepth24;
layout(binding = 31) uniform sampler2DShadow lightDepth25;