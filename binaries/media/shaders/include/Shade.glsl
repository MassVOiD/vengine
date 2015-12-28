
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
    float t = mix(1.0, max(0.02, max(NdotL, NdotV)), step(0.0, s));

    float sigma2 = roughness * roughness;
    float A = 1.0 + sigma2 * (1.0 / (sigma2 + 0.13) + 0.5 / (sigma2 + 0.33));
    float B = 0.45 * sigma2 / (sigma2 + 0.09);

    return max(0.0, NdotL) * (A + B * s / t) / PI;
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

    float VdotN = max(dot(viewDirection, surfacenormal), 0.0001);
    float LdotN = max(dot(lightDirection, surfacenormal), 0.0001);

    //Half angle vector
    vec3 H = normalize(lightDirection + viewDirection);

    //Geometric term
    float NdotH = max(abs(dot(surfacenormal, H)), 0.0001);
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
    //return dist == 0 ? 1 : (1) / (PI*dist*dist+1);
    
   float constantAttenuation=1.0;

   float linearAttenuation=1.0;

   float quadraticAttenuation=0.02;

   float att = 1.0/(constantAttenuation + (linearAttenuation * dist) + (quadraticAttenuation * dist * dist));
   return att;
}

float fresnelSchlick(float VdotH)
{
	return  pow(1.0 - VdotH, 5.0);
}
vec3 makeFresnel(float V2Ncos, vec3 reflected)
{
	return reflected + 0.5 * reflected * pow(1.0 - V2Ncos, 5.0);
}

float G1V(float dotNV, float k)
{
	return 1.0f/(dotNV*(1.0f-k)+k);
}

vec3 LightingFuncGGX_REF(vec3 N, vec3 V, vec3 L, float roughness, vec3 F0)
{
	float alpha = roughness*roughness;

	vec3 H = normalize(V+L);

	float dotNL = max(0, dot(N,L));
	float dotNV = max(0, dot(N,V));
	float dotNH = max(0, dot(N,H));
	float dotLH = max(0, dot(L,H));

	vec3 F;
	float D, vis;

	// D
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;
	D = alphaSqr/(pi * denom * denom);

	// F
	float dotLH5 = pow(1.0f-dotLH,5);
	F = F0 + (1.0-F0)*(dotLH5);

	// V
	float k = alpha/2.0f;
	vis = G1V(dotNL,k)*G1V(dotNV,k);

	vec3 specular = dotNL * D * F * vis;
	return specular;
}


vec2 LightingFuncGGX_FV(float dotLH, float roughness)
{
	float alpha = roughness*roughness;

	// F
	float F_a, F_b;
	float dotLH5 = pow(1.0f-dotLH,5);
	F_a = 1.0f;
	F_b = dotLH5;

	// V
	float vis;
	float k = alpha/2.0f;
	float k2 = k*k;
	float invK2 = 1.0f-k2;
	vis = rcp(dotLH*dotLH*invK2 + k2);

	return vec2(F_a*vis,F_b*vis);
}

float LightingFuncGGX_D(float dotNH, float roughness)
{
	float alpha = roughness*roughness;
	float alphaSqr = alpha*alpha;
	float pi = 3.14159f;
	float denom = dotNH * dotNH *(alphaSqr-1.0) + 1.0f;

	float D = alphaSqr/(pi * denom * denom);
	return D;
}

vec3 LightingFuncGGX_OPT3(vec3 N, vec3 V, vec3 L, float roughness, vec3 F0)
{
	vec3 H = normalize(V+L);

	float dotNL = max(0, dot(N,L));
	float dotNH = max(0, dot(N,H));
	float dotLH = max(0, dot(L,H));

	float D = LightingFuncGGX_D(dotNH,roughness);
	vec2 FV_helper = LightingFuncGGX_FV(dotLH,roughness);
	vec3 FV = F0*FV_helper.x + (1.0f-F0)*FV_helper.y;
	vec3 specular = dotNL * D * FV;

	return specular;
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
    bool ignoreAtt
){
	lightColor.rgb *= lightColor.a;
    vec3 lightRelativeToVPos =normalize( lightPosition - fragmentPosition);
    
    vec3 cameraRelativeToVPos = -normalize(fragmentPosition - camera);
    
    float distanceToLight = distance(fragmentPosition, lightPosition);
    float att = ignoreAtt ? 1 : (CalculateFallof(distanceToLight));
   // if(att < 0.002) return vec3(0);
    
    /*float specularComponent = (1.0 - roughness) * cookTorranceSpecular(
        lightRelativeToVPos,
        cameraRelativeToVPos,
        normal,
        roughness + 0.01
        );*/
    vec3 specularComponent = mix(1.0, att, roughness * roughness) * LightingFuncGGX_REF(
        normal,
        cameraRelativeToVPos,
        lightRelativeToVPos,
        clamp(roughness, 0.005, 0.99),
		mix(albedo * lightColor.rgb, albedo, metalness)
        );
    
    float diffuseComponent = 0*max(0, dot(lightRelativeToVPos, normal));

    vec3 cc = lightColor.rgb*albedo;
    vec3 difcolor = cc * diffuseComponent * att;
    return mix((specularComponent + difcolor) * 0.5, specularComponent, metalness);
}

vec3 shadePhoton(vec2 uv, vec3 color){
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    return color*albedo;
}
vec3 shadePhotonSpecular(vec2 uv, vec3 color){
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    float metalness =  texture(normalsTex, uv).a;
    return mix(color * albedo, color, metalness);
}

vec3 shadeUV(vec2 uv,
    vec3 lightPosition, 
    vec4 lightColor
){
    vec3 position = FromCameraSpace(reconstructCameraSpace(uv));
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    vec3 normal = normalize(texture(normalsTex, uv).rgb);
      
    float roughness = texture(diffuseColorTex, uv).a;
    float metalness =  texture(normalsTex, uv).a;
    return shade(CameraPosition, albedo, normal, position, lightPosition, lightColor, roughness, metalness, false);
}

vec3 shadeUVNoAtt(vec2 uv,
    vec3 lightPosition, 
    vec4 lightColor
){
    vec3 position = FromCameraSpace(reconstructCameraSpace(uv));
    vec3 albedo = texture(diffuseColorTex, uv).rgb;
    vec3 normal = normalize(texture(normalsTex, uv).rgb);
      
    float roughness = texture(diffuseColorTex, uv).a;
    float metalness =  texture(normalsTex, uv).a;
    return shade(CameraPosition, albedo, normal, position, lightPosition, lightColor, roughness, metalness, true);
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