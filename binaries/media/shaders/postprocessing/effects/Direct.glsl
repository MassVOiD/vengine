
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
        float rd3d = length(wd);
        float inter = mix(d3d1, d3d2, i);
        if(rd3d < inter) {
            outs -= 0.07 * (1.0-i);
        }
    }
    return max(0, outs);
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
	
	float parallax = step(1.0, metalness);
	metalness = fract(metalness);
	
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
		float dx = parallax == 1 ? 1.0 : testVisibility3d(UV, position, position + normalize(LightsPos[i] - position) * 0.06);
		if(UV.y > 0.5)dx = 1;
        color1 += (radiance) * percent * dx;
    }
	
	return color1;
}