#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;
layout(binding = 32) uniform sampler2D worldPosTexBack;
layout(binding = 33) uniform sampler2D meshDataTex;

//layout (binding = 11, r32ui) volatile uniform uimage3D full3dScene;

const int MAX_SIMPLE_LIGHTS = 20;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

float meshRoughness;
float meshSpecular;
float meshDiffuse;


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
vec2 projectPoint(vec3 point){
    //vec3 dirPosition = start + end;
    
    vec4 clipspace = (PV) * vec4((point), 1.0);
    vec2 sspace1 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
    return sspace1;
}

struct SampleData
{
    bool isInSurface;
    vec2 sampleUV;
    vec3 sampleNormal;
    vec3 sampleWorldPos;
    vec3 sampleColor;
};

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
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
    vec4 clipspace = (PV) * vec4((w1), 1.0);
    vec2 sspace1 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
    vec4 clipspace2 = (PV) * vec4((w2), 1.0);
    vec2 sspace2 = (clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5;
    float d3d1 = length(ToCameraSpace(w1));
    float d3d2 = length(ToCameraSpace(w2));
    for(float ix=0;ix<1.0;ix+= 0.05) { 
        float i = fract(rand(UV) * (RandomSeed2 * 123.54234234 * ix));
        vec2 ruv = mix(sspace1, sspace2, i);
        float zcheck = mix(clipspace.z, clipspace2.z, i);
        if(ruv.x<0 || ruv.x > 1 || ruv.y < 0 || ruv.y > 1 || zcheck < 0) continue;
        vec3 wd = texture(worldPosTex, ruv).rgb; 
        float rd3d = length(wd) + 0.01;
        wd = texture(worldPosTexBack, ruv).rgb; 
        float othick = length(wd) + 0.01;
        if(othick - rd3d < 1.2 || othick < rd3d) othick = rd3d + 1.2;
        float inter = distance(CameraPosition, mix(w1, w2, i));
        if(rd3d < inter && (othick > inter)) {
            return false;
        }
    }
    return true;
    /*float res =0;
    for(float ix=0;ix<1.0;ix+= 0.05) { 
        vec3 inter = mix(w1, w2, ix);    
        vec3 normalized = (inter) *3;
        normalized = clamp(normalized, -32, 32);
        normalized = normalized + 32;
        ivec3 imgcoord = ivec3(int(normalized.x), int(normalized.y), int(normalized.z));
        uint val = imageLoad(full3dScene, imgcoord).r;
        if(val == FrameINT) res +=1;
    }
    return res < 20;*/
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
        
        /*vec3 displace = vec3(
            fract(rd) * 2 - 1, 
            fract(rd*12.2562), 
            fract(rd*7.121214) * 2 - 1
        ) * clamp(length(posCenter), 0.1, 2.0);*/
        vec3 displace = vec3(0, 5, 0);
        float dotdiffuse =  max(0, dot(normalize(displace),  (normalCenter)));
        for(int div = 0;div < octaves; div++)
        {
            if(testVisibility3d(UV, posCenter, posCenter + displace))
            {
                ambient += ambientColor * dotdiffuse;
            }
            //displace = displace * 0.8;
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
vec3 LightingPhysical(
    vec3 lightColor, 
    float albedo, 
    float gloss, 
    vec3 normal, 
    vec3 lightDir, 
    vec3 viewDir, 
    float atten )
    {
        // calculate diffuse term
        float n_dot_l = clamp( dot( normal, lightDir ) * atten, 0.0, 1.0);
        vec3 diffuse = n_dot_l * lightColor;

        // calculate specular term
        vec3 h = normalize( lightDir + viewDir );

        float n_dot_h = clamp( dot( normal, h ), 0.0, 1.0);
        float normalization_term = ( ( meshSpecular * meshRoughness ) + 2.0 ) / 8.0;
        float blinn_phong = pow( n_dot_h, meshSpecular * meshRoughness );
        float specular_term = blinn_phong * normalization_term;
        float cosine_term = n_dot_l;

        float h_dot_l = dot( h, lightDir );
        float base = 1.0 - h_dot_l;
        float exponential =	pow( base, 5.0 );

        vec3 specColor = vec3(1) * gloss;
        vec3 fresnel_term = specColor + ( 1.0 - specColor ) * exponential;

        vec3 specular = specular_term * cosine_term * fresnel_term * lightColor;

        vec3 final_output = texture(texColor, UV).rgb * diffuse * ( 1 - fresnel_term ) + specular;
        return final_output;
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
    float NdotH = max(x, 0.001);
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
    float NdotH = max(abs(dot(surfaceNormal, H)), 0.0);
    float VdotH = max(abs(dot(viewDirection, H)), 0.0001);
    float LdotH = max(abs(dot(lightDirection, H)), 0.0001);
    float G1 = (2.0 * NdotH * VdotN) / VdotH;
    float G2 = (2.0 * NdotH * LdotN) / LdotH;
    float G = min(1.0, min(G1, G2));

    //Distribution term
    float D = beckmannDistribution(NdotH, roughness);

    //Fresnel term
    float F = 1;

    //Multiply terms and done
    return  G * F * D / max(3.14159265 * VdotN, 0.001);
}

const float EPSILON  = 1e-6;
const float INFINITY = 1e+4;
float textautoshadow(vec2 p1, vec2 p2, vec3 ldir, vec3 newpos){
        float ret = 999;
        float dt1 = length(CameraPosition - ldir);
        float dt2 = length(CameraPosition - newpos);
        
        for(float i=0.0001; i<1; i+=0.1){
            vec3 w = (texture(worldPosTex, mix(p1, p2, i)).rgb);
            ret = min( length(w) - mix(dt1, dt2, i) -0.001, ret );
        }
        return  ret<-0.0003?0:1;
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
    meshDiffuse = normal.a;
    meshSpecular = texture(worldPosTex, nUV).a;
    meshRoughness = texture(meshDataTex, nUV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    vec3 color1 = vec3(0);
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        color1 = colorOriginal;
        IgnoreLightingFragment = true;
    } else {
       // color1 = colorOriginal * 0.01;
    }
    
    //vec3 color1 = colorOriginal * 0.2;
    if(texture(texColor, UV).a < 0.99){
        color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    }
    gl_FragDepth = texture(texDepth, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = -vec3(fragmentPosWorld3d.xyz);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
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
            vec3 abc = LightsPos[i];
            
            vec3 lightRelativeToVPos =normalize( abc - fragmentPosWorld3d.xyz);
            /*vec2 pointCenter = projectPoint(fragmentPosWorld3d.xyz);
            vec3 rdir = normalize(abc - fragmentPosWorld3d.xyz);
            vec3 flatdir = cross(cross(normal.xyz, rdir), normal.xyz);
            vec3 newposl = fragmentPosWorld3d.xyz + lightRelativeToVPos*(0.7);
            vec2 pointedDir = clamp(projectPoint(newposl), 0.0, 1.0);
            vec2 d2dir = (pointedDir - pointCenter);
            float dotmax = textautoshadow(pointCenter, pointedDir, fragmentPosWorld3d.xyz, newposl);
           // if(dotmax == 0){
                percent *= dotmax;
            //}*/
        
            float distanceToLight = distance(fragmentPosWorld3d.xyz, abc);
            float att = 1.0 / pow(((distanceToLight/1.0) + 1.0), 2.0) * LightsColors[i].a*4;
            //att = 1;
            if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE)att = 1;
            if(att < 0.002) continue;
            
            
            /*
            vec3 R = reflect(lightRelativeToVPos, normal.xyz);
            float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
            float specularComponent = clamp(pow(cosAlpha, 160.0 / normal.a), 0.0, 1.0) * fragmentPosWorld3d.a;*/
            /*
            float specularComponent = cookTorranceSpecular(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            meshRoughness, 0.1
            ) * meshSpecular;

            
            float diffuseComponent = orenNayarDiffuse(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            meshRoughness, 0.1
            ) * meshDiffuse  ;
            //diffuseComponent = max(0, log(3*dot(normalize(lightRelativeToVPos),  normal.xyz)))* 0.017 ;
            //float diffuseComponent = 0;
            
            float culler = LightsMixModes[i] == LIGHT_MIX_MODE_ADDITIVE ? clamp((1.0 - distance(lightScreenSpace, vec2(0.5)) * 2.0), 0.0, 1.0) : 1.0;

            color1 += (((colorOriginal * (diffuseComponent * LightsColors[i].rgb)) 
            + (LightsColors[i].rgb * specularComponent))
            * att * max(0, percent)) * LightsColors[i].a;*/
           /* color1 += LightingPhysical(
                LightsColors[i].rgb*LightsColors[i].a, 
                meshDiffuse, 
                meshSpecular, 
                normal.xyz, 
                normalize(lightRelativeToVPos),
                normalize(cameraRelativeToVPos), 
                att) * clamp(percent, 0, 1);*/
            float specularComponent = clamp(cookTorranceSpecular(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            max(0.02, meshRoughness), 1
            ), 0.0, 1.0);

            
            float diffuseComponent = clamp(orenNayarDiffuse(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            meshRoughness, 1
            ), 0.0, 1.0);   
            float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
            fresnel = fresnel * fresnel * fresnel + 1.0;
            
            vec3 illumalbedo = vec3((LightsColors[i].r+LightsColors[i].g+LightsColors[i].b)*0.333);
            vec3 cc = mix(LightsColors[i].rgb*colorOriginal, LightsColors[i].rgb, meshMetalness);
            
            vec3 difcolor = cc * diffuseComponent;
            vec3 difcolor2 = LightsColors[i].rgb*colorOriginal * diffuseComponent;
            vec3 specolor = cc * specularComponent;
            
            vec3 radiance = mix(difcolor2 + specolor, difcolor*meshRoughness + specolor, meshMetalness);
            
            color1 += (radiance) * att * percent;
            
            
         //   if(percent < 0){
                //is in shadow! lets try subsufrace scattering
                float subsc =  min(0.1, abs(LastProbeDistance));
                float amount = 1.0/(pow((subsc *500)+1, 2));
                amount = abs(amount);
                  
               // color1 += colorOriginal *  max(0.1, amount);
        //    } 
            if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) foundSun = 1;
        
        }
        
    }
    
    for(int i=0;i<SimpleLightsCount;i++){
        
            vec3 abc = SimpleLightsPos[i];
            
            vec3 lightRelativeToVPos =normalize( abc - fragmentPosWorld3d.xyz);
            float distanceToLight = distance(fragmentPosWorld3d.xyz, abc);
            float att = 1.0 / pow(((distanceToLight/SimpleLightsColors[i].a) + 1.0), 2.0);
            //att = 1;;
            if(att < 0.002) continue;
            
            if(SimpleLightsColors[i].r < 0){
                color1 += SimpleLightsColors[i].rgb * att;
                continue;
            }
            
            float specularComponent = clamp(cookTorranceSpecular(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            meshRoughness, 0
            ) * meshSpecular, 0.0, 1.0);

            float diffuseComponent = clamp(orenNayarDiffuse(
            normalize(lightRelativeToVPos),
            normalize(cameraRelativeToVPos),
            normal.xyz,
            meshRoughness, 0
            ) * meshDiffuse, 0.0, 1.0);   
            float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
            fresnel = fresnel * fresnel * fresnel + 1.0;
            
            color1 +=  (((colorOriginal * (diffuseComponent * SimpleLightsColors[i].rgb)) * fresnel            + (mix(SimpleLightsColors[i].rgb, colorOriginal, meshMetalness) * specularComponent))
            * att * fresnel);      
            
    }
    
        
    
    //color1 += lightPoints();
    //if(UV.x < 0.4 && UV.y < 0.4){
    //    color1 = vec3(length(texture(worldPosTex, UV*2.5).rgb - texture(//worldPosTexBack, UV*2.5).rgb) * 0.1);
    //}        
    //vec3 inter = mix(w1, w2, ix);    
       /* vec3 normalized = (fragmentPosWorld3d.xyz) *3;
        normalized = clamp(normalized, -32, 32);
        normalized = normalized + 32;
        ivec3 imgcoord = ivec3(int(normalized.x), int(normalized.y), int(normalized.z));
        uint val = imageLoad(full3dScene, imgcoord).r;*/
       // color1.b += float(val) / 512;
    outColor = vec4(color1, 1);
}