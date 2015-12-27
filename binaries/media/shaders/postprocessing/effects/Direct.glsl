
float testVisibility3d(vec2 cuv, vec3 w1, vec3 w2) {
	vec2 sspace1 = cuv;
	vec4 clipspace2 = (VPMatrix) * vec4((w2), 1.0);
	vec2 sspace2 = (clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5;
    float d3d1 = length(ToCameraSpace(w1));
    float d3d2 = length(ToCameraSpace(w2));
	float outs = 1.0;
    for(float i=0;i<1.0;i+= 0.012) { 
        vec2 ruv = mix(sspace1, sspace2, i);
        if(ruv.x<0 || ruv.x > 1 || ruv.y < 0 || ruv.y > 1) continue;
        vec3 wd = reconstructCameraSpace(ruv);
		float m = texture(normalsTex, ruv).a;
		if(m > 1.0) continue;
        float rd3d = length(wd);
        float inter = mix(d3d1, d3d2, i);
        if(rd3d < inter) {
            outs -= 0.07 * (1.0-i);
        }
    }
    return max(0, outs);
}

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
vec3 areaExperiment(
	vec3 position, 
	vec3 normal, 
	float roughness){
	
	vec3 cameraspace = ToCameraSpace(position);
	
    vec3 dir = normalize(reflect(cameraspace, normal));
    vec3 vdir = normalize(cameraspace);
	
	vec3 projdir = mix(dir, normal, roughness);
	float iexp = intersectPlane(Ray(position, projdir), vec3(0, 5, 0), vec3(0, 0, 1));
	if(iexp < 0) return vec3(0);
	vec3 p = position + projdir * iexp;
	float attdiff = CalculateFallof(distance(position, p)) * dot(normalize(p-position), normal);
	vec2 asize = vec2(3, 3);
	float miss = closestEdge(vec2(0, 5), asize, p.xy);
	float irgh = 1.0 - roughness;
	float co =  (1.0 / (mix(miss * 15.0, 0, 1.0 - (irgh*irgh*irgh)) + 1));
	float l = smoothstep(0.0, 1.0, co);
	
	vec3 ill = LODAERA(aoTex, (vec2(p.x, p.y - 5) / asize) * 0.5 + 0.5, roughness);
	
	vec3 col = ill * l * mix(1.0, attdiff, roughness);
	return col;
}

vec3 DirectLight(
	vec3 camera,
	vec3 albedo, 
	vec3 normal,
	vec3 position, 
	float roughness, 
	float metalness
){
    vec3 color1 = vec3(0);
	
	float parallax = step(100.0, metalness);
	//metalness = fract(metalness);
	
    for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(position, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

        float percent = 0;
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) {
            percent = getShadowPercent(lightScreenSpace, position, i);

        }
        vec3 radiance = shade(camera, albedo, normal, position, LightsPos[i], LightsColors[i], roughness, metalness, false);
		//float dx = parallax == 1 ? 1.0 : testVisibility3d(UV, position, position + normalize(LightsPos[i] - position) * 0.06);
        color1 += (radiance) * percent;
    }
	
	return color1 + areaExperiment(position, normal, roughness);
}