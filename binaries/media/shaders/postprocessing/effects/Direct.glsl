
float closestEdge(vec2 point, vec2 size, vec2 uv)
{
    vec2 ledg = point - vec2(size.x, 0);
    vec2 redg = point + vec2(size.x, 0);
    vec2 tedg = point - vec2(0, size.y);
    vec2 bedg = point + vec2(0, size.y);
    float xmissl = max(0, ledg.x - uv.x);
    float xmissr = max(0, uv.x - redg.x);
    float ymissl = max(0, tedg.y - uv.y);
    float ymissr = max(0, uv.y - bedg.y);
    float miss = xmissl;
    miss = max(miss, xmissr);
    miss = max(miss, ymissl);
    miss = max(miss, ymissr);
    return miss;
}
#define AERA_ROUGH_LOOKUP 512
vec3 LODAERA(sampler2D sampler, vec2 uv, float roughness){
    float levels = float(textureQueryLevels(sampler)) - 1;
    float mx = log2(roughness*AERA_ROUGH_LOOKUP+1)/log2(AERA_ROUGH_LOOKUP);
    vec3 result = textureLod(sampler, uv, mx * levels).rgb;
    return result;
}

struct AreaLight
{
    vec3 Position;
    vec3 Normal;
    vec2 Size;
};

vec3 areaExperiment(
    vec3 position, 
    vec3 normal, 
    float roughness){
    
    vec3 cameraspace = ToCameraSpace(position);
    
    vec3 dir = normalize(reflect(cameraspace, normal));
    vec3 vdir = normalize(cameraspace);
    
    vec3 projdir = mix(dir, vec3(0, 0, -1), roughness);
    float iexp = intersectPlane(Ray(position, projdir), vec3(0, 5, 0), vec3(0, 0, 1));
    if(iexp < 0) return vec3(0);
    vec3 p = position + projdir * iexp;
    vec2 asize = vec2(3, 3);
    float miss = closestEdge(vec2(0, 5), asize, p.xy);
    float attdiff = CalculateFallof(distance(position, p) + miss) * dot(normalize(p-position), normal);
    float irgh = 1.0 - roughness;
    float co =  (1.0 / (mix(miss * 15.0, 0, 1.0 - (irgh*irgh*irgh)) + 1));
    float l = smoothstep(0.0, 1.0, co);
    
    vec3 ill = LODAERA(aoTex, clamp((vec2(p.x, p.y - 5) / asize) * 0.5 + 0.5, 0.0, 1.0), roughness);
    
    vec3 col = ill * l * mix(1.0, attdiff, roughness);
    return col;
}

struct SimpleLight
{
    vec4 Position;
    vec4 Direction;
    vec4 Color;
    vec4 alignment;
};

layout (std430, binding = 6) buffer SLBf
{
    SimpleLight simpleLights[]; 
}; 


vec3 raymarchFog(vec3 start, vec3 end){
    vec3 color1 = vec3(0);

    //vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;    
    
    bool foundSun = false;
    for(int i=0;i<LightsCount;i++){
    
        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(end, 1.0);
        
        
        float fogDensity = 0.0;
        float fogMultiplier = 5.4;
        vec2 fuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
        vec3 lastPos = start - mix(start, end, 0.01);
        const float stepr = 0.1;
        float samples = 1.0 / stepr;
        float stepsize = distance(start, end) / samples;
        for(float m = 0.0; m< 1.0;m+= stepr){
            vec3 pos = mix(start, end, m/* + rand2s(UV * 3 + m * 5)*/ * stepsize);
            float distanceMult = stepsize;
            //float distanceMult = 5;
            lastPos = pos;
            float att = CalculateFallof(distance(pos, LightsPos[i]));
            //att = 1;
            lightClipSpace = lightPV * vec4(pos, 1.0);
            
            float fogNoise = 1.0;
    
            vec2 frfuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
            float frfuvz = (lightClipSpace.z / lightClipSpace.w) * 0.5 + 0.5;
            //float idle = 1.0 / 1000.0 * fogNoise * fogMultiplier * distanceMult;
            float idle = 0.0;
            if(lightClipSpace.z < 0.0 || frfuv.x < 0.0 || frfuv.x > 1.0 || frfuv.y < 0.0 || frfuv.y > 1.0){ 
                fogDensity += idle;
                continue;
            }
            float diff = (lookupDepthFromLight(i, frfuv, frfuvz));
            if(diff > 0) {
                float culler = 1;//clamp(1.0 - distance(frfuv, vec2(0.5)) * 2.0, 0.0, 1.0);
                //float fogNoise = 1.0;
                fogDensity += idle + 1.0 / 20.0 * culler * fogNoise * fogMultiplier * att * distanceMult;
            } else {
                fogDensity += idle;
            }
        }
        color1 += LightsColors[i].xyz * fogDensity;
        
    }
    return color1;
}

vec3 makeFog(){
    vec3 cspaceEnd = ToCameraSpace(Input.WorldPos);
    if(length(cspaceEnd) > 80) cspaceEnd = normalize(cspaceEnd) * 80;
    vec3 fragmentPosWorld3d = Input.WorldPos;
    return vec3(raymarchFog(CameraPosition, fragmentPosWorld3d));
}

vec3 DirectLight(
    vec3 camera,
    vec3 albedo, 
    vec3 normal,
    vec3 position, 
    float roughness
){
    vec3 color1 = vec3(0);
    
    //float parallax = step(100.0, metalness);
    //metalness = fract(metalness);
    
    for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(position, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

        float percent = 0;
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) {
            percent = getShadowPercent(lightScreenSpace, position, LightsShadowMapsLayer[i]);
			percent *= 1.0 - smoothstep(0.4, 0.5, distance(lightScreenSpace, vec2(0.5)));
        }
        vec3 radiance = shade(camera, albedo, normal, position, LightsPos[i], LightsColors[i].rgb, roughness, false);
        //float dx = parallax == 1 ? 1.0 : testVisibility3d(UV, position, position + normalize(LightsPos[i] - position) * 0.06);
        color1 += (radiance) * percent;
    }/*
    for(int i=0;i<SimpleLightsCount;i++){
        vec3 pos = simpleLights[i].Position.xyz;
        vec3 n = simpleLights[i].Direction.xyz;
        float angle = cos(simpleLights[i].Color.a);
        float dt = dot(normalize(position - pos), n);
        float factor = smoothstep(angle, 1.0, dt);
        //float cosangle = 
        vec3 col = simpleLights[i].Color.rgb;
        color1 += shade(camera, albedo, normal, position, pos, col, roughness, false) * AOValue;
    }*/
	
	if(UseFog == 1) color1 += makeFog();
    
    return color1;
}