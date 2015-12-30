vec2 HBAO_projectOnScreen(vec3 worldcoord){
    vec4 clipspace = (VPMatrix) * vec4(worldcoord, 1.0);
    vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy )* 0.5 + 0.5;
    return sspace1;
}

float AmbientOcclusionSingle(
vec3 position,
vec3 normal,
float roughness,
float hemisphereSize
){
    //vec2 pixelSize = vec2(length(dFdx(position)), length(dFdy(position)));
    vec3 posc = ToCameraSpace(position);
    vec3 vdir = normalize(posc);
    vec3 tangent = getTangentPlane(normal);
    //normal = normalize(cross((dFdx(position) - position), (dFdy(position) - position))); 
    
    mat3 TBN = inverse(transpose(mat3(
    tangent,
    cross(normal, tangent),
    normal
    )));
    
    float buf = 0.0;
    vec3 dir = normalize(reflect(posc, normal));
    float samples = mix(51, 112, roughness);
    float stepsize = PI*2 / samples;
    float ringsize = min(length(posc), hemisphereSize);
    vec2 uv = HBAO_projectOnScreen(position);
    roughness = 1.0 - roughness;
    for(float g = 0.0; g < PI*2; g+=stepsize)
    {
        vec3 zx = vec3(sin(g), cos(g), 0);
        
        vec3 displace = mix(TBN * normalize(zx), dir, roughness) * ringsize;
        
        vec2 gauss = mix(uv, HBAO_projectOnScreen(position + displace), rand2s(UV + g));
        if(gauss.x < 0.0 || gauss.x > 1.0 || gauss.y < 0.0 || gauss.y > 1.0) continue;
        vec3 pos = reconstructCameraSpace(gauss);
        float dt = max(0, dot(normal, normalize(pos - posc)));
        
        buf += dt * ((ringsize - min(length(pos - posc), ringsize))/ringsize);
    }
    return clamp(pow(1.0 - (buf/samples), 6), 0.0, 1.0);
}

float FastApprox(
vec3 position,
vec3 normal,
float roughness,
float metalness
){
	float d = distance(CameraPosition, position);
	float i = 0;
	float c = 0.0;
	for(float g = 0; g < mPI2 * 2; g+=0.2)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.2)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * g2 * 0.01);
			i += (textureLod(depthTex, UV + gauss, 4).r);
			i += (textureLod(depthTex, UV + gauss, 5).r);
			i += (textureLod(depthTex, UV + gauss, 6).r);
			c += 3.0;
			//i += reverseLog(textureLod(depthTex, UV, 5).r);
			//i += reverseLog(textureLod(depthTex, UV, 6).r);
			//i += reverseLog(textureLod(depthTex, UV, 7).r);
		}
	}
	 i /= c;
	
	float ring = 0.5;
	float amplify = 3.0;
	float diff = max(0, d - i) * amplify;
	float x = clamp(diff * 15, 0.0, 1.0);
	diff *= max(0, ring - diff);
	return pow(1.0 - diff, 12);

}
float AmbientOcclusion(

vec3 position,
vec3 normal,
float roughness,
float metalness

){
    float ao = AmbientOcclusionSingle(position, normal, roughness, 1.1);
    //float ao = FastApprox(position, normal, roughness, 1.1);
    //float ao = VeryFastAO(position, normal, roughness);
    //ao = ao + AmbientOcclusionSingle(position, normal, roughness, 0.525);
   // ao *= 0.5;
    //ao *= AmbientOcclusionSingle(position, normal, tangent, roughness, 0.35);
    //ao = AmbientOcclusionSingle(position, normal, tangent, roughness);
    //if(metalness < 1.0) ao = AmbientOcclusionSingle(position, normal, tangent, 1.0)) * 0.2;
    return ao;// * 0.3333;
}