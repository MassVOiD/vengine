
#define PI 3.14159265

float orenNayarDiffuse(
vec3 lightDirection,
vec3 viewDirection,
vec3 surfacenormal,
float roughness) {

   // return max(0.0, dot(lightDirection, surfacenormal));
    float LdotV = dot(lightDirection, viewDirection);
    float NdotL = dot(lightDirection, surfacenormal);
    float NdotV = dot(surfacenormal, viewDirection);

    float s = LdotV - NdotL * NdotV;
    float t = mix(1.0, max(NdotL, NdotV), step(0.0, s));

    float sigma2 = roughness * roughness;
    float A = 1.0 + sigma2 * (1.0 / (sigma2 + 0.13) + 0.5 / (sigma2 + 0.33));
    float B = 0.45 * sigma2 / (sigma2 + 0.09);

    return max(0.1, NdotL) * (A + B * s / t) / PI;
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
vec3 surfacenormal,
float roughness) {
    return beckmannDistribution(dot(surfacenormal, normalize(lightDirection + viewDirection)), roughness);
}

float cookTorranceSpecular(
vec3 lightDirection,
vec3 viewDirection,
vec3 surfacenormal,
float roughness) {

    float VdotN = max(dot(viewDirection, surfacenormal), 0.0);
    float LdotN = max(dot(lightDirection, surfacenormal), 0.0);

    //Half angle vector
    vec3 H = normalize(lightDirection + viewDirection);

    //Geometric term
    float NdotH = max(abs(dot(surfacenormal, H)), 0.0);
    float VdotH = max(abs(dot(viewDirection, H)), 0.0001);
    float LdotH = max(abs(dot(lightDirection, H)), 0.0001);
    float G1 = (2.0 * NdotH * VdotN) / VdotH;
    float G2 = (2.0 * NdotH * LdotN) / LdotH;
    float G = min(1.0, min(G1, G2));

    //Distribution term
    float D = beckmannDistribution(NdotH, roughness);

    //Multiply terms and done
    return  G * D / max(3.14159265 * VdotN, 0.001);
}

float CalculateFallof( float dist){
    //return 1.0 / pow(((dist) + 1.0), 2.0);
    return dist == 0 ? 1 : (1) / (PI*dist*dist+1);
    
}

float fresnelSchlick(float VdotH)
{
	return  pow(1.0 - VdotH, 5.0);
}
vec3 makeFresnel(float V2Ncos, vec3 reflected)
{
	return reflected + 0.5 * reflected * pow(1.0 - V2Ncos, 5.0);
}

#define MaterialTypeSolid 0
#define MaterialTypeRandomlyDisplaced 1
#define MaterialTypeWater 2
#define MaterialTypeSky 3
#define MaterialTypeWetDrops 4
#define MaterialTypeGrass 5
#define MaterialTypePlanetSurface 6
#define MaterialTypeTessellatedTerrain 7
vec3 shade(
    vec3 camera,
    vec3 albedo, 
    vec3 normal,
    vec3 fragmentPosition, 
    vec3 lightPosition, 
    vec4 lightColor, 
    float roughness, 
    float metalness, 
    float specular,
    bool ignoreAtt
){
    vec3 lightRelativeToVPos =normalize( lightPosition - fragmentPosition);
    
    vec3 cameraRelativeToVPos = -normalize(fragmentPosition - camera);
    
    float distanceToLight = distance(fragmentPosition, lightPosition);
    float att = ignoreAtt ? 1 : (CalculateFallof(distanceToLight)* lightColor.a);
   // if(att < 0.002) return vec3(0);
    
    float specularComponent = cookTorranceSpecular(
        lightRelativeToVPos,
        cameraRelativeToVPos,
        normal,
        max(0.022, (roughness) + 0.01)
        );

    
    float diffuseComponent = (1.0 - metalness) * orenNayarDiffuse(
        lightRelativeToVPos,
        cameraRelativeToVPos,
        normal,
        max(0.022, (roughness) + 0.01)
        );   

    vec3 cc = lightColor.rgb*albedo;
    
    float fresnel = fresnelSchlick(dot(cameraRelativeToVPos, normal));
    
    vec3 difcolor = cc * diffuseComponent * att;
    vec3 difcolor2 = lightColor.rgb * albedo * diffuseComponent * att;
    vec3 specolor = mix(cc * specularComponent, lightColor.rgb * specularComponent, specular);
    specolor = makeFresnel(dot(cameraRelativeToVPos, normal), specolor);
    
    return (difcolor2 + makeFresnel(dot(cameraRelativeToVPos, normal), specolor));
}

vec3 shadePhoton(vec2 uv, vec3 color){
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    return color*albedo;
}
vec3 shadePhotonSpecular(vec2 uv, vec3 color){
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    float spec = texture(worldPosTex, uv).a;
    return mix(color*albedo, color, spec);
}

vec3 shadeUV(vec2 uv,
    vec3 lightPosition, 
    vec4 lightColor
){
    vec3 position = FromCameraSpace(texture(worldPosTex, uv).rgb);
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    float specular = texture(worldPosTex, uv).a;
    vec3 normal = normalize(texture(normalsTex, uv).rgb);
      
    float roughness = texture(meshDataTex, uv).a;
    float metalness =  texture(meshDataTex, uv).z;
    return shade(CameraPosition, albedo, normal, position, lightPosition, lightColor, roughness, metalness, specular, false);
}

vec3 shadeUVNoAtt(vec2 uv,
    vec3 lightPosition, 
    vec4 lightColor
){
    vec3 position = FromCameraSpace(texture(worldPosTex, uv).rgb);
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    float specular = texture(worldPosTex, uv).a;
    vec3 normal = normalize(texture(normalsTex, uv).rgb);
      
    float roughness = texture(meshDataTex, uv).a;
    float metalness =  texture(meshDataTex, uv).z;
    return shade(CameraPosition, albedo, normal, position, lightPosition, lightColor, roughness, metalness, specular, true);
}


 vec3 hemisphereSample_uniform(float u, float v) {
     float phi = v * 2.0 * PI;
     float cosTheta = 1.0 - u;
     float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
     return vec3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
 }
 
    
 vec3 hemisphereSample_cos(float u, float v) {
     float phi = v * 2.0 * PI;
     float cosTheta = sqrt(1.0 - u);
     float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
     return vec3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
 }