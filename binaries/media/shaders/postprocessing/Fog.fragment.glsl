#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
//layout(binding = 31) uniform sampler2D normalsTex;

out vec4 outColor;

const int MAX_FOG_SPACES = 256;
uniform int FogSpheresCount;
uniform vec3 FogSettings[MAX_FOG_SPACES]; //x: FogDensity, y: FogNoise, z: FogVelocity
uniform vec4 FogPositionsAndSizes[MAX_FOG_SPACES]; //w: Size
uniform vec4 FogVelocitiesAndBlur[MAX_FOG_SPACES]; //w: Blur
uniform vec4 FogColors[MAX_FOG_SPACES];

// perlin 3d noise
vec3 mod289(vec3 x) {  return x - floor(x * (1.0 / 289.0)) * 289.0;}vec4 mod289(vec4 x) {  return x - floor(x * (1.0 / 289.0)) * 289.0;}vec4 permute(vec4 x) {     return mod289(((x*34.0)+1.0)*x);}vec4 taylorInvSqrt(vec4 r){  return 1.79284291400159 - 0.85373472095314 * r;}float snoise(vec3 v)  {   const vec2  C = vec2(1.0/6.0, 1.0/3.0) ;  const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);  vec3 i  = floor(v + dot(v, C.yyy) );  vec3 x0 =   v - i + dot(i, C.xxx) ;  vec3 g = step(x0.yzx, x0.xyz);  vec3 l = 1.0 - g;  vec3 i1 = min( g.xyz, l.zxy );  vec3 i2 = max( g.xyz, l.zxy );  vec3 x1 = x0 - i1 + C.xxx;  vec3 x2 = x0 - i2 + C.yyy;   vec3 x3 = x0 - D.yyy;       i = mod289(i);   vec4 p = permute( permute( permute(              i.z + vec4(0.0, i1.z, i2.z, 1.0 ))           + i.y + vec4(0.0, i1.y, i2.y, 1.0 ))            + i.x + vec4(0.0, i1.x, i2.x, 1.0 ));  float n_ = 0.142857142857;  vec3  ns = n_ * D.wyz - D.xzx;  vec4 j = p - 49.0 * floor(p * ns.z * ns.z);   vec4 x_ = floor(j * ns.z);  vec4 y_ = floor(j - 7.0 * x_ );     vec4 x = x_ *ns.x + ns.yyyy;  vec4 y = y_ *ns.x + ns.yyyy;  vec4 h = 1.0 - abs(x) - abs(y);  vec4 b0 = vec4( x.xy, y.xy );  vec4 b1 = vec4( x.zw, y.zw );  vec4 s0 = floor(b0)*2.0 + 1.0;  vec4 s1 = floor(b1)*2.0 + 1.0;  vec4 sh = -step(h, vec4(0.0));  vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;  vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;  vec3 p0 = vec3(a0.xy,h.x);  vec3 p1 = vec3(a0.zw,h.y);  vec3 p2 = vec3(a1.xy,h.z);  vec3 p3 = vec3(a1.zw,h.w);  vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));  p0 *= norm.x;  p1 *= norm.y;  p2 *= norm.z;  p3 *= norm.w;  vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);  m = m * m;  return 42.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1),dot(p2,x2), dot(p3,x3) ) );}


//#define ENABLE_FOG_NOISE

void main()
{
	vec3 color1 = vec3(0);

	vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;	
	
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
		
		
		float fogDensity = 0.0;
		
		for(float m = 0.0; m< 1.0;m+= 0.02){
			vec3 pos = mix(CameraPosition, fragmentPosWorld3d, m);
			vec4 lightClipSpace = lightPV * vec4(pos, 1.0);
			#ifdef ENABLE_FOG_NOISE
			float fogNoise = (snoise(pos / 4.0 + vec3(0, -Time*0.2, 0)) + 1.0) / 2.0;
			#else
			float fogNoise = 1.0;
			#endif
			float idle = 1.0 / 2500.0 * fogNoise;
			if(lightClipSpace.z < 0.0){ 
				fogDensity += idle;
				continue;
			}
			vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
			if(lightScreenSpace.x < 0.0 || lightScreenSpace.x > 1.0 || lightScreenSpace.y < 0.0 || lightScreenSpace.y > 1.0){ 
				fogDensity += idle;
				continue;
			}
			if(toLogDepth(distance(pos, LightsPos[i])) < lookupDepthFromLight(i,lightScreenSpace)) {
				float culler = clamp(1.0 - distance(lightScreenSpace, vec2(0.5)) * 2.0, 0.0, 1.0);
				//float fogNoise = 1.0;
				fogDensity += idle + 1.0 / 200.0 * culler * fogNoise;
			} else {
				fogDensity += idle;
			}
		}
		color1 += LightsColors[i].xyz * LightsColors[i].a * fogDensity;
		
	}
	
    outColor = vec4(color1, 1);
}