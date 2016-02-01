vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}

#define MMAL_LOD_REGULATOR 512
float precentage = 0;
float falloff = 0;
samplerCube cube;
vec3 MMALNoPrcDiffuse(vec3 visdis, float dist, vec3 normal, vec3 reflected, float roughness, int i){
	
    float levels = float(textureQueryLevels(cube)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	result += textureLod(cube, vec3(1, 0, 0), levels).rgb;
	result += textureLod(cube, vec3(0, 1, 0), levels).rgb;
	result += textureLod(cube, vec3(0, 0, 1), levels).rgb;
	result += textureLod(cube, vec3(-1, 0, 0), levels).rgb;
	result += textureLod(cube, vec3(0, -1, 0), levels).rgb;
	result += textureLod(cube, vec3(0, 0, -1), levels).rgb;
	result /= 6;
    //return vec3pow(result * 2.0, 1.7)*0.5;
    return precentage * result;
}
vec3 MMAL(vec3 visdis, float dist, vec3 normal, vec3 reflected, float roughness, int i){
	
    float levels = float(textureQueryLevels(cube)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	dist = toLogDepthEx(dist, 1000.0);
	float counter = 0.01;
	float aaprc = 0;
	float aafw = 0;
	for(float x = 0.0; x < 10.0; x+= 3.2){
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) * 0.1;
		float dst = texture(cube, visdis + rd*0.3).a;
		float prc = max(0.001, 1.0 - step(0.0001, dist - dst)); 
		float fw = CalculateFallof((reverseLogEx(dst, 1000.0) + reverseLogEx(dist, 1000.0)));
		float fw2 = CalculateFallof((reverseLogEx(dst, 1000.0)));
		result += (prc <= 0.01 ? AOValue : 1.0) * (prc * mix(fw2, fw, roughness) * textureLod(cube, mix(reflected, normal + rd*0.5, roughness), mx * levels).rgb);
		aafw += fw;
		aaprc += prc;
		counter += 1.0;
	}
    //return vec3pow(result * 2.0, 1.7)*0.5;
	precentage = aaprc / counter;
	falloff = aafw / counter;
    return result / counter;
}

uniform int CurrentlyRenderedCubeMap;
vec3 EnvironmentLight(FragmentData data)
{       
    vec3 dir = normalize(reflect(data.cameraPos, data.normal));
    vec3 vdir = normalize(data.cameraPos);
	vec3 reflected = vec3(0);
	vec3 diffused = vec3(0);
	for(int i=0;i<CubeMapsCount;i++){
		if(i == CurrentlyRenderedCubeMap) continue;
		cube = samplerCube(CubeMapsAddrs[i]);
		//vec3 dir = -normalize(CubeMapsPositions[i].xyz - data.worldPos);
		vec3 dirvis = -normalize(CubeMapsPositions[i].xyz - data.worldPos);
		//float falloff = CalculateFalloff(distance(data.worldPos, CubeMapsPositions[i].xyz));
		//falloff *= 1.0 - smoothstep(0.0, 4.0, distance(data.worldPos, CubeMapsPositions[i].xyz));
		//float falloff = cosfalloff(distance(data.worldPos, CubeMapsPositions[i].xyz), CubeMapsFalloffs[i].x);
		//if(falloff < 0.003) continue;
		
		float dt = max(0, dot(data.normal, -normalize(data.worldPos - CubeMapsPositions[i].xyz)));
		//falloff *= dt;
		reflected += MMAL(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), data.normal, dir, data.roughness, i) * data.specularColor ;
		//reflected = vec3(0);
		diffused += falloff * MMALNoPrcDiffuse(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), data.normal, -dirvis, 1.0, i) * data.diffuseColor;
	}
	    
    return makeFresnel(1.0 - max(0, dot(data.normal, vdir)), reflected + diffused) * 2.0;
}
