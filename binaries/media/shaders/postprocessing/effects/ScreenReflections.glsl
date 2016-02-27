
vec2 project(vec3 pos){
    vec4 tmp = (VPMatrix * vec4(pos, 1.0));
    return (tmp.xy / tmp.w) * 0.5 + 0.5;
}


vec4 ScreenReflections(FragmentData data){
	if(data.roughness > 0.5) return vec4(0);
    vec2 closuv = vec2(0);
	float closest = 0;
	float closestdst = 0;
	#define SSREFSSTEPS 64
	
	vec2 start = UV;
	vec2 reconstructNorm = project(data.worldPos + data.normal * 0.05);
	float invs = 0.1 / SSREFSSTEPS;
	vec2 dir = normalize(reconstructNorm - start) / SSREFSSTEPS;
	
	vec3 reflected = normalize(reflect(data.cameraPos, data.normal));
		
    for(int i=0;i<SSREFSSTEPS;i++){
		start += dir + rand2s(start + UV) * invs;
		if(start.x > 1.0 || start.y > 1.0 || start.x < 0.0 || start.y < 0.0) break;
		vec3 rec = reconstructCameraSpace(start);
		vec3 dd = normalize(rec - data.cameraPos);
		float dt = max(0, dot(dd, reflected));
		if(dt > closest && dt > 0.9){
			closest = dt;
			closestdst = distance(rec, data.cameraPos);
			closuv = start;
		}
    }
	vec3 res = vec3(0);
	float blurfactor = 0;
	if(closest > 0){
		closuv = clamp(closuv, 0.0, 1.0);
		float dim = 1.0 - distance(closuv, vec2(0.5));
	
		vec3 deferred = texture(deferredTex, closuv).rgb;
		
		vec4 normalsDistanceData = textureMSAAFull(normalsDistancetex, closuv);
		vec3 normal = normalsDistanceData.rgb;
		
		blurfactor = closestdst * 0.1;
	
		res = deferred * dim * step(0, dot(normal, -data.normal));
	}
	float roughMaxed = 1.0 - (data.roughness * 2.0);
	
    return vec4(res * roughMaxed, blurfactor * data.roughness);
}