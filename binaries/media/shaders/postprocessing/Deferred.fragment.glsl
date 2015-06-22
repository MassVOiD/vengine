#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;

const int MAX_SIMPLE_LIGHTS = 20;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

out vec4 outColor;
bool IgnoreLightingFragment = false;

vec2 refractUV(){
    vec3 rdir = normalize(CameraPosition - texture(worldPosTex, UV).rgb);
    vec3 crs1 = normalize(cross(CameraPosition, texture(worldPosTex, UV).rgb));
    vec3 crs2 = normalize(cross(crs1, rdir));
    vec3 rf = refract(rdir, texture(normalsTex, UV).rgb, 0.6);
    return UV - vec2(dot(rf, crs1), dot(rf, crs2)) * 0.3;
}

mat4 PV = (ProjectionMatrix * ViewMatrix);
vec2 projdir(vec3 start, vec3 end){
	//vec3 dirPosition = start + end;
	
	vec4 clipspace = (PV) * vec4((start), 1.0);
	vec2 sspace1 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
	clipspace = (PV) * vec4((end), 1.0);
	vec2 sspace2 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
	return (sspace2 - sspace1);
}

struct SampleData
{
  bool isInSurface;
  vec2 sampleUV;
  vec3 sampleNormal;
  vec3 sampleWorldPos;
  vec3 sampleColor;
};

SampleData getSampleData(vec3 sampl, vec3 displace){
	vec4 clipspace = (PV) * vec4(sampl, 1.0);
	vec2 sspace1 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
    vec3 wd = (texture(worldPosTex, sspace1).rgb);
    if(length(wd) < distance(CameraPosition, sampl)){
        // in surface
        vec3 wpos = FromCameraSpace(texture(worldPosTex, sspace1).rgb);
        return SampleData(
            true, 
            sspace1, 
            wpos,
            texture(normalsTex, sspace1).rgb,
            texture(texColor, sspace1).rgb);
    } else {
        // in free air
        return SampleData(
            false, 
            sspace1, 
            sampl,
            -displace,
            vec3(0));
    }
}

bool testVisibility3d(vec2 cuv, vec3 w1, vec3 w2) {
    //vec3 direction = normalize(w2 - w1);
    float d3d1 = length(w1);
    float d3d2 = length(w2);
    vec2 sdir = projdir(FromCameraSpace(w1), FromCameraSpace(w2));
    for(float i=0;i<1.0;i+= 0.2) { 
        vec2 ruv = mix(cuv, cuv + sdir, i);
        vec3 wd = texture(worldPosTex, ruv).rgb; 
        float rd3d = length(wd) + 0.01;
        if(rd3d < mix(d3d1, d3d2, i) && mix(d3d1, d3d2, i) - rd3d < 1.01) {
            return false;
        }
    }
    return true;
}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 Radiosity() 
{
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    vec3 ambient = vec3(0);
    const int samples = 28;
    const int octaves = 3;
    
    // choose between noisy and slower, but better looking variant
    float randomizer = 138.345341 * rand(UV) + Time;
    // or faster, non noisy variant, which is also cool looking
    //const float randomizer = 138.345341;
    
    vec3 ambientColor = vec3(1,1,1);
    
    uint counter = 0;
    
    for(int i=0;i<samples;i++)
    {
        float rd = randomizer * float(i);
        
        vec3 displace = vec3(
            fract(rd) * 2 - 1, 
            fract(rd*12.2562), 
            fract(rd*7.121214) * 2 - 1
        ) * clamp(length(posCenter), 0.1, 2.0);
        float dotdiffuse =  max(0, dot(normalize(displace),  (normalCenter)));
        for(int div = 0;div < octaves; div++)
        {
            if(testVisibility3d(UV, posCenter, posCenter + displace))
            {
                ambient += ambientColor * dotdiffuse;
            }
            displace = displace * 0.8;
            counter++;
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return rs;
}

vec3 ball(vec3 colour, float sizec, float xc, float yc){
	float xdist = (abs(UV.x - xc));
	float ydist = (abs(UV.y - yc)) * ratio;
	
	float d = sizec / length(vec2(xdist, ydist));
	return colour * (d);
}

vec3 lightPoints(){
    vec3 color = vec3(0);
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);

		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4((LightsPos[i]), 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;
		
        float badass_depth = (clipspace.z / clipspace.w) * 0.5 + 0.5;	
        float logg = texture(texDepth, UV).r;
        
        if(logg > badass_depth || IgnoreLightingFragment) {
            color += ball(vec3(LightsColors[i].rgb*2.0),LightPointSize / ( badass_depth) * 0.001, sspace1.x, sspace1.y);
            //color += ball(vec3(LightsColors[i]*2.0 * overall),12.0 / dist, sspace1.x, sspace1.y) * 0.03f;
        }
    
	}
	
	for(int i=0;i<SimpleLightsCount;i++){
	
		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(SimpleLightsPos[i], 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;
		float dist = distance(CameraPosition, SimpleLightsPos[i]);
		float revlog = reverseLog(texture(texDepth, sspace1).r);
		if(dist > revlog)continue;
		dist += 1.0;
		color += ball(vec3(SimpleLightsColors[i]*2.0 * SimpleLightsColors[i].a),SimpleLightPointSize * (0.8/ dist), sspace1.x, sspace1.y);

	
	}
    return color;
}

#define PI 3.14159265

float orenNayarDiffuse(
  vec3 lightDirection,
  vec3 viewDirection,
  vec3 surfaceNormal,
  float roughness,
  float albedo) {
  
  float LdotV = dot(lightDirection, viewDirection);
  float NdotL = dot(lightDirection, surfaceNormal);
  float NdotV = dot(surfaceNormal, viewDirection);

  float s = LdotV - NdotL * NdotV;
  float t = mix(1.0, max(NdotL, NdotV), step(0.0, s));

  float sigma2 = roughness * roughness;
  float A = 1.0 + sigma2 * (albedo / (sigma2 + 0.13) + 0.5 / (sigma2 + 0.33));
  float B = 0.45 * sigma2 / (sigma2 + 0.09);

  return albedo * max(0.0, NdotL) * (A + B * s / t) / PI;
}

float beckmannDistribution(float x, float roughness) {
  float NdotH = max(x, 0.0001);
  float cos2Alpha = NdotH * NdotH;
  float tan2Alpha = (cos2Alpha - 1.0) / cos2Alpha;
  float roughness2 = roughness * roughness;
  float denom = 3.141592653589793 * roughness2 * cos2Alpha * cos2Alpha;
  return exp(tan2Alpha / roughness2) / denom;
}

float beckmannSpecular(
  vec3 lightDirection,
  vec3 viewDirection,
  vec3 surfaceNormal,
  float roughness) {
  return beckmannDistribution(dot(surfaceNormal, normalize(lightDirection + viewDirection)), roughness);
}

float cookTorranceSpecular(
  vec3 lightDirection,
  vec3 viewDirection,
  vec3 surfaceNormal,
  float roughness,
  float fresnel) {

  float VdotN = max(dot(viewDirection, surfaceNormal), 0.0);
  float LdotN = max(dot(lightDirection, surfaceNormal), 0.0);

  //Half angle vector
  vec3 H = normalize(lightDirection + viewDirection);

  //Geometric term
  float NdotH = max(dot(surfaceNormal, H), 0.0);
  float VdotH = max(dot(viewDirection, H), 0.000001);
  float LdotH = max(dot(lightDirection, H), 0.000001);
  float G1 = (2.0 * NdotH * VdotN) / VdotH;
  float G2 = (2.0 * NdotH * LdotN) / LdotH;
  float G = min(1.0, min(G1, G2));
  
  //Distribution term
  float D = beckmannDistribution(NdotH, roughness);

  //Fresnel term
  float F = pow(1.0 - VdotN, fresnel);

  //Multiply terms and done
  return  G * F * D / max(3.14159265 * VdotN, 0.000001);
}

void main()
{   
    float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
    if(alpha < 0.99){
        //nUV = refractUV();
    }
    vec3 colorOriginal = texture(texColor, nUV).rgb;    
    vec4 normal = texture(normalsTex, nUV);
    vec3 color1 = vec3(0);
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        color1 = colorOriginal;
        IgnoreLightingFragment = true;
    } else {
        color1 = colorOriginal * 0.01;
    }
    
    //vec3 color1 = colorOriginal * 0.2;
    if(texture(texColor, UV).a < 0.99){
        color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    }
    gl_FragDepth = texture(texDepth, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    int foundSun = 0;
    if(!IgnoreLightingFragment) for(int i=0;i<LightsCount;i++){
        
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && foundSun > 0)continue;
        //if(len < LightsRanges[i].x) continue;
        //if(len > LightsRanges[i].y) continue;

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(fragmentPosWorld3d.xyz, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   
        
        
        
        //int counter = 0;

        // do shadows
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0){ 
            float percent = getShadowPercent(lightScreenSpace, fragmentPosWorld3d.xyz, i);
            if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE){
                vec3 abc = LightsPos[i];
               // if(percent <= 0) continue;

               // float distanceToLight = distance(fragmentPosWorld3d.xyz, abc);
                //float att = 1.0 / pow(((distanceToLight/1.0) + 1.0), 2.0) * LightsColors[i].a * 5.0;
                //if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE)att = 1;
                //if(att < 0.002) continue;
                
                
                vec3 lightRelativeToVPos = abc - fragmentPosWorld3d.xyz;
                
                float specularComponent = cookTorranceSpecular(
                    normalize(lightRelativeToVPos),
                    normalize(cameraRelativeToVPos),
                    normal.xyz,
                    0.8, 0.2
                );

                float diffuseComponent = orenNayarDiffuse(
                    normalize(lightRelativeToVPos),
                    normalize(cameraRelativeToVPos),
                    normal.xyz,
                    0.8, 0.2
                );
                
                percent = max(0, percent);
                color1 += ((colorOriginal * (diffuseComponent * LightsColors[i].rgb)) 
                + (LightsColors[i].rgb * specularComponent)) * percent;
                
                color1 *= LightsColors[i].a * 0.01;
                foundSun = 1;
            } else {
                vec3 abc = LightsPos[i];
                float distanceToLight = distance(fragmentPosWorld3d.xyz, abc);
                float att = 1.0 / pow(((distanceToLight/1.0) + 1.0), 2.0) * LightsColors[i].a * 30;
                //att = 1;
                if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE)att = 1;
                if(att < 0.002) continue;
                
                
                vec3 lightRelativeToVPos =normalize( abc - fragmentPosWorld3d.xyz);
                /*
                vec3 R = reflect(lightRelativeToVPos, normal.xyz);
                float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
                float specularComponent = clamp(pow(cosAlpha, 160.0 / normal.a), 0.0, 1.0) * fragmentPosWorld3d.a;*/
                
                float specularComponent = cookTorranceSpecular(
                    normalize(lightRelativeToVPos),
                    normalize(cameraRelativeToVPos),
                    normal.xyz,
                    0.5, 0.0
                );

                
                float diffuseComponent = orenNayarDiffuse(
                    normalize(lightRelativeToVPos),
                    normalize(cameraRelativeToVPos),
                    normal.xyz,
                    0.8, 0.9
                );
                //float diffuseComponent = max(0, dot(normalize(lightRelativeToVPos),  normal.xyz));
                //float diffuseComponent = 0;
                
                float culler = LightsMixModes[i] == LIGHT_MIX_MODE_ADDITIVE ? clamp((1.0 - distance(lightScreenSpace, vec2(0.5)) * 2.0), 0.0, 1.0) : 1.0;

                color1 += (((colorOriginal * (diffuseComponent * LightsColors[i].rgb)) 
                + (LightsColors[i].rgb * specularComponent))
                * att * culler * max(0, percent)) * LightsColors[i].a * 0.01;
                if(percent < 0){
                    //is in shadow! lets try subsufrace scattering
                    /*float amount = (-percent) * 0.3;
                    // todo - less lighting for more distance
                    float dotdiffuse2 = dot(normalize(lightRelativeToVPos), normalize (-normal.xyz));
                    float diffuseComponent2 = clamp(dotdiffuse2, 0.0, 1.0);                        
                    color1 += colorOriginal * culler * 10 * dotdiffuse2 * LightsColors[i].rgb *  att*  max(0, amount);*/
                }
                
            }
        }
        
    }
    

    for(int i=0;i<SimpleLightsCount;i++){
        
        vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(SimpleLightsPos[i], 1.0);
        vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
        //if(clipspace.z < 0.0) continue;
        
        
        float distanceToLight = distance(fragmentPosWorld3d.xyz, SimpleLightsPos[i]);
        //float dist = distance(CameraPosition, SimpleLightsPos[i]);
        float att = 1.0 / pow(((distanceToLight/1.0) + 1.0), 2.0) * SimpleLightsColors[i].a;
        //float revlog = reverseLog(texture(texDepth, nUV).r);
        
        vec3 lightRelativeToVPos = SimpleLightsPos[i] - fragmentPosWorld3d.xyz;
        vec3 R = reflect(lightRelativeToVPos, normal.xyz);
        float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
        float specularComponent = clamp(pow(cosAlpha, 80.0 / normal.a), 0.0, 1.0) * fragmentPosWorld3d.a;
        
        lightRelativeToVPos = SimpleLightsPos[i] - fragmentPosWorld3d.xyz;
        float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (normal.xyz));
        float diffuseComponent = clamp(dotdiffuse, 0.0, 1.0);
        color1 += ((colorOriginal * (diffuseComponent * SimpleLightsColors[i].rgb)) 
        + (SimpleLightsColors[i].rgb * specularComponent))
        *att;
    
        
    }
    color1 += lightPoints();
    //if(UV.x < 0.4 && UV.y < 0.4){
    //    color1 = texture(lightDepth0, UV*2.5).rrr*10;
    //}
    outColor = vec4(color1, 1);
}