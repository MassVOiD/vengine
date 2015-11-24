#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

layout (std430, binding = 6) buffer RandomsBuffer
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand2s(seeder)) ;
}

int randsPointer = 0;
uniform int RandomsCount;
float getRand(){
    float r = Randoms[randsPointer];
    randsPointer++;
    if(randsPointer >= RandomsCount) randsPointer = 0;
    return r;
}

uniform int UseRSM;

float meshRoughness;
float meshSpecular;
float meshDiffuse;

uniform float VDAOGlobalMultiplier;


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

/********************/

struct AABox
{
    vec4 Minimum;
    vec4 Maximum;
    vec4 Color;
    int Container;
    int StupidAlignment1;
    int StupidAlignment2;
    int StupidAlignment3;
};
uniform int AABoxesCount;

layout (std430, binding = 7) buffer BoxesBuffer
{
    AABox AABoxes[]; 
}; 

/********************/

struct AAContainerBox
{
    vec4 Minimum;
    vec4 Maximum;
    int StartIndex;
    int InnerCount;
    int StupidAlignment5;
    int StupidAlignment6;
};
uniform int AABoxesContainersCount;

layout (std430, binding = 8) buffer Boxes2Buffer
{
    AAContainerBox AAContainerBoxes[]; 
}; 

/********************/

struct SimplePointLight
{
    vec4 Position;
    vec4 Color;
};
uniform int SimpleLightsCount;

layout (std430, binding = 5) buffer SimplePointLightBuffer
{
    SimplePointLight simplePointLights[]; 
}; 


// well.. yeah
// we gonna pathtrace


float NearHitPos = 0.0;
bool tryIntersectBox(vec3 origin, vec3 direction,
vec3 bMin,
vec3 bMax)
{
    vec3 OMIN = ( bMin - origin ) / direction;    
    vec3 OMAX = ( bMax - origin ) / direction;    
    vec3 MAX = max ( OMAX, OMIN );    
    vec3 MIN = min ( OMAX, OMIN );  
    float final = min ( MAX.x, min ( MAX.y, MAX.z ) );
    float start = max ( max ( MIN.x, 0.0 ), max ( MIN.y, MIN.z ) );    
    NearHitPos = start;
    return final > start;
}

float LastCombinedDistance = 9999999;
vec3 getIntersectInContainer(int start, int count, int container, vec3 origin, vec3 direction){
    float lastDistance = 9999999;
    vec3 color = vec3(0);
    AAContainerBox abc = AAContainerBoxes[container];
    for(int i=start;i<start + count;i++){
       // if(AABoxes[i].Container != container) continue;
        if(tryIntersectBox(origin, direction, AABoxes[i].Minimum.xyz, AABoxes[i].Maximum.xyz) &&
                NearHitPos < lastDistance){
            lastDistance = NearHitPos;
            color = AABoxes[i].Color.a * AABoxes[i].Color.rgb; 
        }
    }
    LastCombinedDistance = lastDistance;
    return color;
}

vec3 getIntersect(vec3 originalColor, vec3 origin, vec3 direction){

    vec3 color = originalColor;
    float lastDistance = 9999999;
    
    int indices[16] = int[16](0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    int hit = 0;
    for(int i=0;i<AABoxesContainersCount;i++){
        if(tryIntersectBox(origin, direction, AAContainerBoxes[i].Minimum.xyz, AAContainerBoxes[i].Maximum.xyz)){
            indices[hit] = i;
            hit++;
            //color = getIntersectInContainer(i, origin, direction);
        }
        
        // color += getIntersectInContainer(i, origin, direction);
    }
    lastDistance = 9999999;
    for(int i=0;i<hit;i++){
        
        vec3 a = getIntersectInContainer(AAContainerBoxes[indices[i]].StartIndex, AAContainerBoxes[indices[i]].InnerCount, indices[i], origin, direction);
        if(LastCombinedDistance < lastDistance){
            lastDistance = LastCombinedDistance;
            color = a;
        }
        NearHitPos = 0.0;
    }
    
    
    LastCombinedDistance = 999999;
    return color;
}
bool getIntersectInContainerEarlyEndShadows(int start, int count, int container, vec3 origin, vec3 direction, float maxdistance){
    float lastDistance = 9999999;
    AAContainerBox abc = AAContainerBoxes[container];
    for(int i=start;i<start + count;i++){
        if(tryIntersectBox(origin, direction, AABoxes[i].Minimum.xyz, AABoxes[i].Maximum.xyz)){
            if(NearHitPos < maxdistance) return true;
        }
    }
    LastCombinedDistance = lastDistance;
    return false;
}

float getIntersectShadows(vec3 origin, vec3 direction, float maxdistance){

    float lastDistance = 9999999;
    
    int indices[16] = int[16](0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    int hit = 0;
    for(int i=0;i<AABoxesContainersCount;i++){
        if(tryIntersectBox(origin, direction, AAContainerBoxes[i].Minimum.xyz, AAContainerBoxes[i].Maximum.xyz) && NearHitPos < maxdistance){
            indices[hit] = i;
            hit++;
        }
    }
    lastDistance = 9999999;
    for(int i=0;i<hit;i++){
        
        if(getIntersectInContainerEarlyEndShadows(AAContainerBoxes[indices[i]].StartIndex, AAContainerBoxes[indices[i]].InnerCount, indices[i], origin, direction, maxdistance)){
            return 0;
        }
    }
    LastCombinedDistance = 999999;
    return 1;
}

float traceVisibility(vec3 p1, vec3 p2){
    return getIntersectShadows(p1, normalize(p2 - p1), distance(p1, p2));
}

vec3 random3dSample(){
    return normalize(vec3(
    getRand() * 2 - 1, 
    getRand() * 2 - 1, 
    getRand() * 2 - 1
    ));
}

vec2 getTexel(sampler2D t){
 return 1.0 / vec2(textureSize(t, 0));
}

vec3 reconstructTangentWorldSpace(vec2 uv){
    vec3 bc = texture(worldPosTex, uv).rgb;
    vec2 dsp = getTexel(worldPosTex);
    vec3 bdx = bc - texture(worldPosTex, uv+vec2(dsp.x, 0)).rgb;
    vec3 bdy = bc - texture(worldPosTex, uv+vec2(0, dsp.y)).rgb;
    
    return vec3(0, 0, 1);
}

// using this brdf makes cosine diffuse automatically correct
vec3 BRDF(vec3 reflectdir, vec3 norm, float roughness){
    vec3 displace = random3dSample();
    displace = displace * sign(dot(norm, displace));
    return mix(displace, reflectdir, roughness);
}
vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}
vec3 adjustGamma(vec3 c, float gamma){
    return vec3pow(c, 1.0/gamma)/gamma;
}

vec2 HitPos = vec2(-2);
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(int i=0; i<5; i++){
        float ix = getRand();
        vec2 muv = mix(p1, p2, ix);
        float expr = min(muv.x, muv.y) * -(max(muv.x, muv.y)-1.0);
        float tmp = min(0, sign(expr)) * 
        ret + 
        min(0, -sign(expr)) * 
        max(
        mix(v1, v2, ix) - texture(sampler, muv).r,
        ret);
        if(tmp > ret) HitPos = muv;
        ret = tmp;
    }
    //if(abs(ret) > 0.1) return 0;
    return abs(ret) - 0.0006;
}
vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}
float testVisibility3d(vec3 w1, vec3 w2) {
    HitPos = vec2(-2);
    vec4 clipspace = (PV) * vec4((w1), 1.0);
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) * vec4((w2), 1.0);
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    float d3d1 = toLogDepth(length(ToCameraSpace(w1)));
    float d3d2 = toLogDepth(length(ToCameraSpace(w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, depthTex));

    return mx;
}

vec3 lookupCubeMap(vec3 displace){
    vec3 c = texture(cubeMapTex, displace).rgb;
    return vec3pow(c*2.5, 1.7);
}


uniform int UseVDAO;
uniform int UseHBAO;
void main()
{   

    //  float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
    // if(alpha < 0.99){
    //nUV = refractUV();
    // }
    Seed(UV*88);
    randsPointer = int(randomizer * 123.86786 ) % RandomsCount;
    vec3 colorOriginal = texture(diffuseColorTex, nUV).rgb;    
    vec4 normal = texture(normalsTex, nUV);
    meshDiffuse = normal.a;
    meshSpecular = texture(worldPosTex, nUV).a;
    meshRoughness = texture(meshDataTex, nUV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    float color1 = 0;
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        IgnoreLightingFragment = true;
    } else {
        // color1 = colorOriginal * 0.01;
    }
    
    //vec3 color1 = colorOriginal * 0.2;
    //if(texture(texColor, UV).a < 0.99){
    //    color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    //}
    gl_FragDepth = texture(depthTex, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = normalize(-fragmentPosWorld3d.xyz);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    // int foundSun = 0;
    if(!IgnoreLightingFragment) for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(fragmentPosWorld3d.xyz, 1.0);
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

        //if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0){ 
            float percent = 0;//
            
            if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) 
                continue;
            else {
                
                //percent += traceVisibility(LightsPos[i] + random3dSample() * 0.3, fragmentPosWorld3d.xyz);
                //percent = traceVisibility(LightsPos[i] + random3dSample() * 0.3, //fragmentPosWorld3d.xyz);
              //  percent += traceVisibility(LightsPos[i] + random3dSample() * 0.17, fragmentPosWorld3d.xyz);
                //percent *= 0.5;
             // //  
            }
                
            vec3 radiance = shadeUV(UV, LightsPos[i], LightsColors[i]);
            color1 += percent;
        //}
    }
    
    outColor = clamp(vec4(color1, texture(depthTex, UV).r, 0.0, 1.0), 0.0, 1.0);
    
    
}