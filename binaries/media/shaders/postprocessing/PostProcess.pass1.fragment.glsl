#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl
#define mPI (3.14159265)
#define mPI2 (2*3.14159265)

layout(binding = 0) uniform sampler2DMS texColor;
layout(binding = 1) uniform sampler2DMS texDepth;

const int samples = 8;

layout(binding = 31) uniform sampler2D worldPosTex;
layout(binding = 32) uniform sampler2D worldPosTexDepth;

uniform float LensBlurAmount;
uniform float CameraCurrentDepth;

out vec4 outColor;

const int MAX_LINES_2D = 256;

uniform int Lines2dCount;
uniform vec3 Lines2dStarts[MAX_LINES_2D];
uniform vec3 Lines2dEnds[MAX_LINES_2D];
uniform vec3 Lines2dColors[MAX_LINES_2D];

const int MAX_FOG_SPACES = 256;
uniform int FogSpheresCount;
uniform vec3 FogSettings[MAX_FOG_SPACES]; //x: FogDensity, y: FogNoise, z: FogVelocity
uniform vec4 FogPositionsAndSizes[MAX_FOG_SPACES]; //w: Size
uniform vec4 FogVelocitiesAndBlur[MAX_FOG_SPACES]; //w: Blur
uniform vec4 FogColors[MAX_FOG_SPACES];

uniform vec2 resolution;

float div= 1.0/samples;
ivec2 ctexSize = textureSize(texColor);
ivec2 dtexSize = textureSize(texDepth);

vec3 fetchColor(vec2 inUV){
	vec4 color11 = vec4(0.0);
	ivec2 texcoord = ivec2(ctexSize * inUV); 
	for (int i=0;i<samples;i++)
	{
		color11 += texelFetch(texColor, texcoord, i);  
	}

	color11*= div; 
	return color11.rgb;
}
vec3 fetchColorFast(vec2 inUV){
	vec4 color11 = vec4(0.0);
	ivec2 texcoord = ivec2(ctexSize * inUV); 
	color11 = texelFetch(texColor, texcoord, 0);  
	return color11.rgb;
}
float fetchDepth(vec2 inUV){
	ivec2 texcoord = ivec2(dtexSize * inUV); 
	return texelFetch(texDepth, texcoord,7).r;
}
float fetchDepthSampled(vec2 inUV){
	float color11 = 0.0;
	ivec2 texcoord = ivec2(dtexSize * inUV); 
	for (int i=0;i<samples;i++)
	{
		color11 += texelFetch(texDepth, texcoord, i).r;  
	}

	color11*= div; 
	return color11;
}

vec2 hash2x2(vec2 co) {
	return vec2(
	fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453),
	fract(sin(dot(co.yx ,vec2(12.9898,78.233))) * 43758.5453));
}

float ratio = resolution.y/resolution.x;
	
vec3 ball(vec3 colour, float sizec, float xc, float yc){
	float xdist = (abs(UV.x - xc));
	float ydist = (abs(UV.y - yc)) * ratio;
	
	//float d = (xdist * ydist);
	float d = sizec / length(vec2(xdist, ydist));
	
	return colour * (d);
}

#define MATH_E 2.7182818284

float reverseLog(float dd){
	return pow(MATH_E, dd - 1.0) / LogEnchacer;
}
float getNearDiff(float dist, float limit){
	float diff = 0.0;
	int counter = 0;
	for(float i=-2.0;i<2.0;i+=0.3){
		for(float g=-2.0;g<2.0;g+=0.3){
			float depth = abs(limit - reverseLog(fetchDepth(UV + vec2(i*dist*ratio,g*dist))));
			if(depth < 0.3 && depth > 0.002) diff += 1.0;
			counter++;
		}
	}
	return diff/counter;
}
float getAveragedDepth(float dist){
	float diff = 0.0;
	int counter = 0;
	for(float i=-2.0;i<2.0;i+=0.3){
		for(float g=-2.0;g<2.0;g+=0.3){
			float depth = reverseLog(fetchDepth(UV + vec2(i*dist*ratio,g*dist)));
			diff += depth;
			counter++;
		}
	}
	return diff/counter;
}
vec3 tholdMagicGI(float dist, float limit){
	vec3 center = texture(worldPosTex, UV).rgb;
	vec3 gi = vec3(0.0);
	vec3 lastOKVal = vec3(0.0);
	for(float g2 = 0; g2 < 8.0; g2+=2.0){ 
		for(float g = 0; g < mPI2; g+=0.6){ 
			vec3 texfrag = texture(worldPosTex, UV + vec2(sin(g) * ratio, cos(g))*dist*g2).rgb;
			if(distance(texfrag, center) < 0.7){
				for(int e=0;e<LightsCount;e++){
					float ddiff = distance(center, LightsPos[e]) - distance(texfrag, LightsPos[e]);
					float centerdiff = distance(center, texfrag);
					if(ddiff > 0.0) {
						lastOKVal = LightsColors[e].rgb * LightsColors[e].a * (0.001 / LightsCount * ddiff/centerdiff);
						gi += lastOKVal;
					}
				}
			} else gi += lastOKVal;
		}
	}
	return gi;
}

	
vec3 getFakeGlobalIllumination(float originalDepth){
	if(originalDepth > 0.999) return vec3(0.0);
	float reversed = reverseLog(originalDepth);
	vec3 val = (tholdMagicGI(0.02 / reversed, reversed));
	return val;
}
float tholdMagicSSAO(float dist, float limit){
	float diff = 0.0;
	int counter = 0;
	for(float i=-2.0;i<2.0;i+=0.3){
		for(float g=-2.0;g<2.0;g+=0.3){
			float cdist = fetchDepthSampled(UV + vec2(i*dist*ratio,g*dist));
			if(cdist < limit && limit - cdist < 0.003) diff += 1.0;
			counter++;
		}
	}
	return diff/counter;
}

	
float getSSAOAmount(float originalDepth){
	if(originalDepth > 0.999) return 0.0;
	float reversed = reverseLog(originalDepth);
	float val = (tholdMagicSSAO(0.003, originalDepth));
	if(val < 0.6) return 0.0;
	return (val - 0.6) * 0.1;
}

float getNearDiffByColor(vec3 originalColor){
	float diff = 0.0;
	for(int i=-2;i<2;i++){
		for(int g=-2;g<2;g++){
			vec3 color = fetchColorFast(UV + vec2(i/400.0,g/400.0)).rgb;
			diff += distance(originalColor, color);
		}
	}
	return diff;
}

vec3 blur(float amount){
	vec3 outc = vec3(0);
	for(float g = 0; g < 14.0; g+=1.0){ 
		for(float g2 = 0; g2 < 14.0; g2+=1.0){ 
			vec2 gauss = vec2(getGaussianKernel(int(g)) * amount, getGaussianKernel(int(g2)) * amount);
			vec3 color = fetchColorFast(UV + gauss);
			outc += color;
		}
	}
	return outc / (14.0*14.0);
}
/*
vec3 line(vec2 start, vec2 end, vec3 color){
	float inter1 = (start.x - position.x) / (start.x - end.x);
	float inter2 = (start.y - position.y) / (start.y - end.y);
	vec2 ppos = mix(start, end, max(inter1, inter2));
	float anglediff = 1.0;
	if(distance(ppos, position) < 0.01) anglediff = 0.0;
	if(position.x < min(start.x, end.x)) anglediff = 1.0;
	if(position.y < min(start.y, end.y)) anglediff = 1.0;
	if(position.x > max(start.x, end.x)) anglediff = 1.0;
	if(position.y > max(start.y, end.y)) anglediff = 1.0;
	return color * (clamp(1.0 - anglediff, 0.0, 1.0));
}*/

/* Branching as boobies (line width fixed) */
float aligned(vec2 A, vec2 B, vec2 C){
	float widthK = .75/resolution.x*max(distance(A, B),max(distance(A, C), distance(B, C)));
	return 1.-smoothstep(widthK, widthK*4. ,abs((A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y))));
}
/* Branching as boobies */
vec3 line(vec2 A, vec2 B, vec3 color){
	float dAB = distance(A, B);
	vec2 A2p = A-UV;
	vec2 B2p = B-UV;
	return color*aligned(A, B, UV)*step(distance(A, UV), dAB)*step(distance(B, UV), dAB);
}


vec3 mod289(vec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(vec4 x) {
     return mod289(((x*34.0)+1.0)*x);
}

vec4 taylorInvSqrt(vec4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(vec3 v)
  { 
  const vec2  C = vec2(1.0/6.0, 1.0/3.0) ;
  const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

// First corner
  vec3 i  = floor(v + dot(v, C.yyy) );
  vec3 x0 =   v - i + dot(i, C.xxx) ;

// Other corners
  vec3 g = step(x0.yzx, x0.xyz);
  vec3 l = 1.0 - g;
  vec3 i1 = min( g.xyz, l.zxy );
  vec3 i2 = max( g.xyz, l.zxy );

  //   x0 = x0 - 0.0 + 0.0 * C.xxx;
  //   x1 = x0 - i1  + 1.0 * C.xxx;
  //   x2 = x0 - i2  + 2.0 * C.xxx;
  //   x3 = x0 - 1.0 + 3.0 * C.xxx;
  vec3 x1 = x0 - i1 + C.xxx;
  vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
  vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
  i = mod289(i); 
  vec4 p = permute( permute( permute( 
             i.z + vec4(0.0, i1.z, i2.z, 1.0 ))
           + i.y + vec4(0.0, i1.y, i2.y, 1.0 )) 
           + i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

// Gradients: 7x7 points over a square, mapped onto an octahedron.
// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
  float n_ = 0.142857142857; // 1.0/7.0
  vec3  ns = n_ * D.wyz - D.xzx;

  vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

  vec4 x_ = floor(j * ns.z);
  vec4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

  vec4 x = x_ *ns.x + ns.yyyy;
  vec4 y = y_ *ns.x + ns.yyyy;
  vec4 h = 1.0 - abs(x) - abs(y);

  vec4 b0 = vec4( x.xy, y.xy );
  vec4 b1 = vec4( x.zw, y.zw );

  //vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
  //vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
  vec4 s0 = floor(b0)*2.0 + 1.0;
  vec4 s1 = floor(b1)*2.0 + 1.0;
  vec4 sh = -step(h, vec4(0.0));

  vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
  vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

  vec3 p0 = vec3(a0.xy,h.x);
  vec3 p1 = vec3(a0.zw,h.y);
  vec3 p2 = vec3(a1.xy,h.z);
  vec3 p3 = vec3(a1.zw,h.w);

//Normalise gradients
  vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
  p0 *= norm.x;
  p1 *= norm.y;
  p2 *= norm.z;
  p3 *= norm.w;

// Mix final noise value
  vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
  m = m * m;
  return 42.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), 
                                dot(p2,x2), dot(p3,x3) ) );
}

	
vec3 blurWhitening(){
	vec3 outc = vec3(0);
	for(float g = 0; g < mPI2; g+=0.6){ 
		for(float g2 = 0; g2 < 14.0; g2+=2.0){ 
			vec2 gauss = vec2(sin(g)*ratio, cos(g)) * (g2 * 0.001);
			vec3 color = fetchColor(UV + gauss);
			float luminance = length(color); // luminance from 1.4 to 1.7320
			if(luminance > 1.0){
				luminance = (luminance - 1.0) / 0.320;
				outc += 0.042857142 * luminance;
			}
		}
	}
	return outc;
}

//#define ENABLE_LINES
//#define ENABLE_SSAO
//#define ENABLE_FOG
//#define ENABLE_FOG_NOISE
#define ENABLE_LIGHTPOINTS
//#define ENABLE_BLOOM

void main()
{
	float aperture = 178.0;
	float apertureHalf = 0.5 * aperture * (mPI / 180.0);
	float maxFactor = sin(apertureHalf);

	vec2 xy = 2.0 * UV - 1.0;
	float d = length(xy);
	vec2 newUV = UV;
	if (d < (2.0-maxFactor))
	{
		d = length(xy * maxFactor);
		float z = sqrt(1.0 - d * d);
		float r = atan(d, z) / mPI;
		float phi = atan(xy.y, xy.x);

		newUV.x = r * cos(phi) + 0.5;
		newUV.y = r * sin(phi) + 0.5;
	}

	vec3 color1 = fetchColor(newUV);
	//vec3 color1 = vec3(1);
	float depth = fetchDepth(newUV);
	
	//color1 = vec3(edge);

	#ifdef ENABLE_SSAO
	//color1 += vec3(getFakeGlobalIllumination(depth) / 2);
	color1 -= vec3(getSSAOAmount(depth));
	//return;
	#endif

	//FXAA
	//float edge = getNearDiff(0.0001, reverseLog(depth));
	//if(edge > 0.002)color1 = blur(0.1);
	
	#ifdef ENABLE_LINES
	for(int i=0;i<Lines2dCount;i++){
		vec3 startWorld = Lines2dStarts[i];
		vec3 endWorld = Lines2dEnds[i];
		vec3 lineColor = Lines2dColors[i];
		
		vec4 startClipspace = (ProjectionMatrix * ViewMatrix) * vec4(startWorld, 1.0);
		vec2 startSSpace = ((startClipspace.xyz / startClipspace.w).xy + 1.0) / 2.0;
		
		vec4 endClipspace = (ProjectionMatrix * ViewMatrix) * vec4(endWorld, 1.0);
		vec2 endSSpace = ((endClipspace.xyz / endClipspace.w).xy + 1.0) / 2.0;
		
		color1 += line(startSSpace, endSSpace, lineColor);
	}
	#endif

	vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;
	
	//color1 = normalize(fragmentPosWorld3d);
	
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
		
		#ifdef ENABLE_FOG
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
		#endif
		//color1 = clamp(color1, 0, 1);
	
		#ifdef ENABLE_LIGHTPOINTS
		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(LightsPos[i], 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;
		
		vec4 clipspace2 = lightPV * vec4(CameraPosition, 1.0);
		if(clipspace2.z >= 0.0) {
			vec2 sspace = ((clipspace2.xyz / clipspace2.w).xy + 1.0) / 2.0;
			float dist = distance(CameraPosition, LightsPos[i]);
			float lndist = toLogDepth(dist);
			float overall = 0.0;
			for(float g2 = 0; g2 < 8.0; g2+=2.0){ 
				for(float g = 0; g < mPI2; g+=0.6){ 
					float percent = lookupDepthFromLight(i, sspace + vec2(sin(g) * ratio, cos(g))*0.001*g2);
					float newdist = 1.0f - (lndist - percent);
					if(newdist > 1) overall += 1.0;
				}
			}
			overall /= 100;
			if(overall > 0.01) {
				color1 += ball(vec3(LightsColors[i]*2.0 * overall),2.1/ dist, sspace1.x, sspace1.y);
				//color1 += ball(vec3(LightsColors[i]*2.0 * overall),12.0 / dist, sspace1.x, sspace1.y) * 0.03f;
			}
		}
		#endif
		
	}
	
	//color1 = edge > 0.01 ? vec3(1) : vec3(0);

	#ifdef ENABLE_BLOOM
	color1 += blurWhitening();
	#endif

	//color1 *= 1.0 - (pow(distance(UV, vec2(0.5, 0.5)) * 2.0, 2));
		
	//if(UV.x > 0.5){
	//	color1.x = log(color1.x + 1.0);
	//	color1.y = log(color1.y + 1.0);
	//	color1.z = log(color1.z + 1.0);
	//}
	
		
    outColor = vec4(clamp(color1, 0, 1), 1);
	gl_FragDepth = depth;
}