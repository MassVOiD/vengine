#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl


out vec4 outColor;


const int MAX_FOG_SPACES = 256;
uniform int FogSpheresCount;
uniform vec3 FogSettings[MAX_FOG_SPACES]; //x: FogDensity, y: FogNoise, z: FogVelocity
uniform vec4 FogPositionsAndSizes[MAX_FOG_SPACES]; //w: Size
uniform vec4 FogVelocitiesAndBlur[MAX_FOG_SPACES]; //w: Blur
uniform vec4 FogColors[MAX_FOG_SPACES];


#include noise3D.glsl

float hsh( float n )
{
  return fract(sin(n)*43758.5453123);
}
float nse3d(in vec3 x)
{
  vec3 p = floor(x);
  vec3 f = fract(x);
  f = f*f*(3.0-2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;
    return mix(mix(mix( hsh(n+  0.0), hsh(n+  1.0), f.x), 
    mix( hsh(n+ 57.0), hsh(n+ 58.0), f.x), f.y), 
      mix(mix( hsh(n+113.0), hsh(n+114.0), f.x), 
      mix( hsh(n+170.0), hsh(n+171.0), f.x), f.y), f.z);
}
float hash1(float p)
{
	return fract(sin(p * 172.435) * 29572.683) - 0.5;
}

float hash2(vec2 p)
{
	vec2 r = (456.789 * sin(789.123 * p.xy));
	return fract(r.x * r.y * (1.0 + p.x));
}
float ns(float p)
{
	float fr = fract(p);
	float fl = floor(p);
	return mix(hash1(fl), hash1(fl + 1.0), fr);
}

float densA = 1.0, densB = 2.0;
float fbm(vec3 p)
{
    vec3 q = p;
    
    p += (nse3d(p * 3.0) - 0.5) * 0.3;
    
    //float v = nse3d(p) * 0.5 + nse3d(p * 2.0) * 0.25 + nse3d(p * 4.0) * 0.125 + nse3d(p * 8.0) * 0.0625;
    
    p.y += 0.2;
    
    float mtn = 0.15;
    
    float v = 0.0;
    float fq = 1.0, am = .45;
    for(int i = 0; i < 6; i++)
    {
        v += nse3d(p * fq + mtn * fq) * am;
        fq *= 2.;
        am *= 0.5;
    }
    return v;
}
float fbm(float p)
{
	return (ns(p) * 0.4 + ns(p * 2.0 - 10.0) * 0.125 + ns(p * 8.0 + 10.0) * 0.025);
}
float density(vec3 p)
{
    vec2 pol = vec2(atan(p.y, p.x), length(p.yx));
    
    float v = fbm(p);
    
    float fo = atan((pol.y - 1.5),(pol.y - 1.5)+(densA + densB) * 0.5);
    fo *= (densB - densA);
   // v *= exp(fo * fo * -5.0);
    
    float edg = .4323;
    return smoothstep(edg, edg + 0.05, v);
}
//#define ENABLE_FOG_NOISE
float clouds(vec3 p){
p.z += Time;
    float v = (snoise(p / 4.0) + 1.0) / 2.0;
    float edg = .4323;
    return smoothstep(edg, edg + 0.05, v);
    //return v;
}

vec3 raymarchFog(vec3 start, vec3 end, float sampling){
	vec3 color1 = vec3(0);

	//vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;	
	
    bool foundSun = false;
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(end, 1.0);
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && lightClipSpace.z < 0.0){
            if(foundSun) continue;
            else foundSun = true;
            //att = 1;
        }
		
		
		float fogDensity = 0.0;
		float fogMultiplier = 2.4;
        vec2 fuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
		vec3 lastPos = start - mix(start, end, sampling);
		for(float m = 0.0; m< 1.0;m+= 0.01){
			vec3 pos = mix(start, end, m);
            float distanceMult = clamp(distance(lastPos, pos) * 2, 0, 33) * 6;
            //float distanceMult = 5;
            lastPos = pos;
			float att = 1.0 / pow(((distance(pos, LightsPos[i])/1.0) + 1.0), 2.0) * LightsColors[i].a;
			att = 1;
            if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) att = 0.1;
			lightClipSpace = lightPV * vec4(pos, 1.0);
			#ifdef ENABLE_FOG_NOISE
			//float fogNoise = clouds(pos);
			
			// rain
			float fogNoise = (snoise(vec3(pos.x*7, pos.y * 7, pos.z*7)) + 1.0) / 2.0;
			
			// snow
			//float fogNoise = (density(vec3(pos.x*0.5, pos.y * 5, pos.z*5)) + 1.0) / 2.0;
			//fogNoise = clamp((fogNoise - 0.8) * 20, 0, 1);
			
			#else
			float fogNoise = 1.0;
			#endif
			//float idle = 1.0 / 1000.0 * fogNoise * fogMultiplier;
			float idle = 0.0;
			if(lightClipSpace.z < 0.0){ 
				fogDensity += idle;
				continue;
            }
            vec2 frfuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
            float badass_depth = toLogDepthEx(distance(pos, LightsPos[i]), LightsFarPlane[i]);
            float diff = (badass_depth - lookupDepthFromLight(i, frfuv));
			if(diff < 0) {
				float culler = clamp(1.0 - distance(frfuv, vec2(0.5)) * 2.0, 0.0, 1.0);
				//float fogNoise = 1.0;
				fogDensity += idle + 1.0 / 200.0 * culler * fogNoise * fogMultiplier * att * distanceMult;
			} else {
				fogDensity += idle;
			}
		}
		color1 += LightsColors[i].xyz * fogDensity;
		
	}
    vec3 worldPos = FromCameraSpace(texture(worldPosTex, UV).rgb);
	return color1 * clamp(1.0 / (abs(worldPos.y) * 0.1), 0, 1);
}

vec3 makeFog(){
	vec3 cspaceEnd = texture(worldPosTex, UV).xyz;
    if(length(cspaceEnd) > 800) cspaceEnd = normalize(cspaceEnd) * 800;
	vec3 fragmentPosWorld3d = FromCameraSpace(cspaceEnd);
    return clamp(vec3(raymarchFog(CameraPosition, fragmentPosWorld3d, FogSamples)), 0.0, 1.0);
}

void main()
{
    outColor = vec4(makeFog(), texture(depthTex, UV).r);
}