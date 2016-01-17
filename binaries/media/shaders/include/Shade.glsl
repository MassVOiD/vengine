
#define PI 3.14159265

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
    vec3 lightColor, 
    float roughness, 
    bool ignoreAtt
){
    vec3 lightRelativeToVPos =normalize( lightPosition - fragmentPosition);
    
    vec3 cameraRelativeToVPos = -normalize(fragmentPosition - camera);
    
    float distanceToLight = distance(fragmentPosition, lightPosition);
    float att = ignoreAtt ? 1 : (CalculateFallof(distanceToLight));
	att = mix(1.0, att, roughness);
  
    vec3 specularComponent = mix(1.0, att, roughness * roughness) * LightingFuncGGX_REF(
        normal,
        cameraRelativeToVPos,
        lightRelativeToVPos,
        clamp(roughness, 0.005, 0.99),
        lightColor
        );
    
    float diffuseComponent = 0*max(0, dot(lightRelativeToVPos, normal));

    vec3 cc = lightColor*albedo;
    vec3 difcolor = cc * diffuseComponent * att;
    return specularComponent * albedo;
}
