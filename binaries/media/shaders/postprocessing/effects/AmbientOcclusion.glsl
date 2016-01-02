vec2 HBAO_projectOnScreen(vec3 worldcoord){
    vec4 clipspace = (VPMatrix) * vec4(worldcoord, 1.0);
    vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy )* 0.5 + 0.5;
    return sspace1;
}

float run = 0;
float AmbientOcclusionSingle(
vec3 position,
vec3 normal,
float roughness,
float hemisphereSize
){
	run += 1.1234;
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
    float samples = mix(52, 132, roughness);
    float stepsize = PI*2 / samples;
    float ringsize = min(length(posc), hemisphereSize);
    float iringsize = 1.0/min(length(posc), hemisphereSize);
    vec2 uv = HBAO_projectOnScreen(position);
    roughness = 1.0 - roughness;
    for(float g = 0.0; g < PI*2; g+=stepsize)
    {
        vec3 zx = vec3(rand2s(UV + g*0.2 + 123 + run), rand2s(UV - g - 432 + run), rand2s(UV + g*1.2 + 155 + run)) * 2 - 1;
		zx *= sign(dot(zx, normal));
        
        vec3 displace = mix(normalize(zx), dir, roughness * 0.7 + 0.3) * ringsize;
        
        vec2 gauss = mix(uv, HBAO_projectOnScreen(position + displace), rand2s(UV + g));
        //if(gauss.x < 0.0 || gauss.x > 1.0 || gauss.y < 0.0 || gauss.y > 1.0) continue;
        vec3 pos = reconstructCameraSpace(gauss, 0);
        float dt = max(0, dot(normal, normalize(pos - posc)));
        
        buf += dt * ((ringsize - min(length(pos - posc), ringsize))*iringsize);
    }
    return pow(clamp(1.0 - (buf/samples), 0.0, 1.0), 1.2);
}

float AmbientOcclusion(

vec3 position,
vec3 normal,
float roughness,
float metalness

){
    float ao = 0;//AmbientOcclusionSingle(position, normal, roughness, 0.1);
   // ao = AmbientOcclusionSingle(position, normal, roughness, 0.1);
    //float ao = FastApprox(position, normal, roughness, 1.1);
    //float ao = VeryFastAO(position, normal, roughness);
    ao = AmbientOcclusionSingle(position, normal, roughness, 0.1);
    ao = AmbientOcclusionSingle(position, normal, roughness, 0.525);
    ao = ao * AmbientOcclusionSingle(position, normal, roughness, 1.325);
    //ao *= 0.50;
    //ao *= AmbientOcclusionSingle(position, normal, tangent, roughness, 0.35);
    //ao = AmbientOcclusionSingle(position, normal, tangent, roughness);
    //if(metalness < 1.0) ao = AmbientOcclusionSingle(position, normal, tangent, 1.0)) * 0.2;
    return ao;// * 0.3333;
}