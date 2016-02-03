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
vec3 MMALNoPrcDiffuse(vec3 visdis, float dist, vec3 normal, float roughness, int i){
	
    float levels = float(textureQueryLevels(cube)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	result +=textureLod(cube, vec3(1, 0, 0), levels).rgb;
	result +=textureLod(cube, vec3(0, 1, 0), levels).rgb;
	result +=textureLod(cube, vec3(0, 0, 1), levels).rgb;
	result += textureLod(cube, vec3(-1, 0, 0), levels).rgb;
	result += textureLod(cube, vec3(0, -1, 0), levels).rgb;
	result += textureLod(cube, vec3(0, 0, -1), levels).rgb;	
	result /= 6;
    //return vec3pow(result * 2.0, 1.7)*0.5;
    return precentage * result;
}
#define rlg(a) reverseLogEx(a,1000)
vec3 MMAL(vec3 visdis, float dist, vec3 normal, vec3 reflected, float roughness, int i){
	
    float levels = float(textureQueryLevels(cube)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	dist = toLogDepthEx(dist, 1000);
	float counter = 0.01;
	float aaprc = 0;
	float aafw = 0;
	
	float fw = CubeMapsFalloffs[i].x * CalculateFallof(length((currentFragment.worldPos - CubeMapsPositions[i].xyz)));
	
	for(int x = 0; x < 5; x++){
		//rd=rd.wxyz;
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) * 0.1;
		float dst = texture(cube, visdis + rd*0.6).a;
		float prc = max(0.04, 1.0 - step(0.0001, (dist) - (dst))) * smoothstep(0.0, 0.3, rlg(dst)); 
		aaprc += prc;
		counter += 1.0;
	}
	precentage = aaprc / counter;
		result += (precentage * fw * texture(cube, mix(reflected, normal, roughness)).rgb);
		aafw += fw;
	
    //return vec3pow(result * 2.0, 1.7)*0.5;
	falloff = fw;
    return (result / counter) * (1.0 - ncos(roughness));
}


uniform int CurrentlyRenderedCubeMap;
vec3 EnvironmentLight(FragmentData data)
{       
	if(DisablePostEffects == 1) return vec3(0);
    vec3 dir = normalize(reflect(data.cameraPos, data.normal));
    vec3 vdir = normalize(data.cameraPos);
	vec3 reflected = vec3(0);
	vec3 diffused = vec3(0);
	for(int i=0;i<CubeMapsCount;i++){
		#define CUT 5.0
		if(distance(data.worldPos, CubeMapsPositions[i].xyz) < CUT) {
			float fv = 1.0 - smoothstep(0.0, CUT, distance(data.worldPos, CubeMapsPositions[i].xyz));
			cube = samplerCube(CubeMapsAddrs[i]);
			vec3 dirvis = -normalize(CubeMapsPositions[i].xyz - data.worldPos);

			reflected += fv * MMAL(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), data.normal, dir, data.roughness, i) * data.specularColor ;
			//reflected = vec3(0);
			diffused += fv * MMALNoPrcDiffuse(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), normalize(data.normal), 1.0, i) * data.diffuseColor;
		}
	}
	    
    return reflected + diffused;
}
