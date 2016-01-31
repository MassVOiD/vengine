vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}

#extension GL_ARB_bindless_texture : require
#define MMAL_LOD_REGULATOR 512
float precentage = 0;
float falloff = 0;
vec3 MMALNoPrcDiffuse(vec3 visdis, float dist, vec3 normal, vec3 reflected, float roughness, int i){
	samplerCube cube = cubeMapTex1;
	if(i == 1) cube = cubeMapTex2;
	else if(i == 2) cube = cubeMapTex3;
	else if(i == 3) cube = cubeMapTex4;
	else if(i == 4) cube = cubeMapTex5;
	else if(i == 5) cube = cubeMapTex6;
	else if(i == 6) cube = cubeMapTex7;
	else if(i == 7) cube = cubeMapTex8;
	else if(i == 8) cube = cubeMapTex9;
	else if(i == 9) cube = cubeMapTex10;
	else if(i == 10) cube = cubeMapTex11;
	else if(i == 11) cube = cubeMapTex12;
	else if(i == 12) cube = cubeMapTex13;
	else if(i == 13) cube = cubeMapTex14;
	else if(i == 14) cube = cubeMapTex15;
	else if(i == 15) cube = cubeMapTex16;
	else if(i == 16) cube = cubeMapTex17;
	else if(i == 17) cube = cubeMapTex18;
	else if(i == 18) cube = cubeMapTex19;
	else if(i == 19) cube = cubeMapTex20;
	else if(i == 20) cube = cubeMapTex21;
	else if(i == 21) cube = cubeMapTex22;
	else if(i == 22) cube = cubeMapTex23;
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
	samplerCube cube = cubeMapTex1;
	if(i == 1) cube = cubeMapTex2;
	else if(i == 2) cube = cubeMapTex3;
	else if(i == 3) cube = cubeMapTex4;
	else if(i == 4) cube = cubeMapTex5;
	else if(i == 5) cube = cubeMapTex6;
	else if(i == 6) cube = cubeMapTex7;
	else if(i == 7) cube = cubeMapTex8;
	else if(i == 8) cube = cubeMapTex9;
	else if(i == 9) cube = cubeMapTex10;
	else if(i == 10) cube = cubeMapTex11;
	else if(i == 11) cube = cubeMapTex12;
	else if(i == 12) cube = cubeMapTex13;
	else if(i == 13) cube = cubeMapTex14;
	else if(i == 14) cube = cubeMapTex15;
	else if(i == 15) cube = cubeMapTex16;
	else if(i == 16) cube = cubeMapTex17;
	else if(i == 17) cube = cubeMapTex18;
	else if(i == 18) cube = cubeMapTex19;
	else if(i == 19) cube = cubeMapTex20;
	else if(i == 20) cube = cubeMapTex21;
	else if(i == 21) cube = cubeMapTex22;
	else if(i == 22) cube = cubeMapTex23;
    float levels = float(textureQueryLevels(cube)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	dist = toLogDepthEx(dist, 1000.0);
	float counter = 0.001;
	float aaprc = 0;
	float aafw = 0;
	for(float x = 0.0; x < 10.0; x+= 1.2){
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) * 0.1;
		float dst = texture(cube, visdis + rd*0.3).a;
		float prc = max(0.001, 1.0 - step(0.0001, dist - dst)); 
		float fw = CalculateFallof((reverseLogEx(dst, 1000.0) + reverseLogEx(dist, 1000.0)));
		float fw2 = CalculateFallof((reverseLogEx(dst, 1000.0)));
		result += prc * mix(fw2, fw, roughness) * textureLod(cube, mix(reflected, normal + rd, roughness), mx * levels).rgb;
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
		//vec3 dir = -normalize(CubeMapsPositions[i].xyz - data.worldPos);
		vec3 dirvis = -normalize(CubeMapsPositions[i].xyz - data.worldPos);
		//float falloff = CalculateFalloff(distance(data.worldPos, CubeMapsPositions[i].xyz));
		//falloff *= 1.0 - smoothstep(0.0, 4.0, distance(data.worldPos, CubeMapsPositions[i].xyz));
		//float falloff = cosfalloff(distance(data.worldPos, CubeMapsPositions[i].xyz), CubeMapsFalloffs[i].x);
		//if(falloff < 0.003) continue;
		
		float dt = max(0, dot(data.normal, -normalize(data.worldPos - CubeMapsPositions[i].xyz)));
		//falloff *= dt;
		reflected += MMAL(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), data.normal, dir, data.roughness, i) * data.specularColor * dt;
		//reflected = vec3(0);
		diffused += falloff * MMALNoPrcDiffuse(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), data.normal, -dirvis, 1.0, i) * data.diffuseColor * dt;
	}
	    
    return makeFresnel(1.0 - max(0, dot(data.normal, vdir)), reflected + diffused) * 2.0;
}
