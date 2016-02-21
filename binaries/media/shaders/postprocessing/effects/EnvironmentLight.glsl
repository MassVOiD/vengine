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

#define rlg(a) reverseLogEx(a,1000)
#define tld(a) toLogDepthEx(a,1000)

float MMALGetShadowforFuckSake(float dists, vec3 wpos, vec3 dir){
	float dist = toLogDepthEx(dists, 1000);
	/*float blocker = 0.0;
	for(int x = 0; x < 9; x++){
		//rd=rd.wxyz;
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) * 0.1;
		float dst = texture(cube, dir + rd*0.5).a;
		blocker = max(blocker, dists - rlg(dst));
	}*/
	float aaprc = 0.0;
	for(int x = 0; x < 3; x++){
		//rd=rd.wxyz;
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) ;
		float dst = texture(cube, dir + rd*0.1).a;
		float prc = max(0.0, 1.0 - step(0.001, dist - dst)); 
		aaprc += prc;
	}
	return aaprc / 3;
}

vec3 MMAL(vec3 visdis, float dist, vec3 normal, vec3 reflected, float roughness, int i){
	
    float levels = float(textureQueryLevels(cube))*0.5;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	float counter = 0.01;
	float aaprc = 0;
	float aafw = 0;
	
	float fw = 0.2;//CubeMapsFalloffs[i].x * CalculateFallof(length((currentFragment.worldPos - CubeMapsPositions[i].xyz)));
	precentage = MMALGetShadowforFuckSake(dist, currentFragment.worldPos, visdis);
	for(int x = 0; x < 9; x++){
		//rd=rd.wxyz;
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) * 0.6;
		result += precentage * ( textureLod(cube, mix(reflected, normal, roughness) + rd*0.9*roughness, mx * levels).rgb);
		counter += 1.0;
	}
	aafw += fw;
	
    //return vec3pow(result * 2.0, 1.7)*0.5;
	falloff = fw;
    return (result / counter) * (1.0 - ncos(roughness));
}


uniform int CurrentlyRenderedCubeMap;
vec3 EnvironmentLight(FragmentData data)
{       
	
    vec3 dir = normalize(reflect(data.cameraPos, data.normal));
    vec3 vdir = normalize(data.cameraPos);
	vec3 reflected = vec3(0);
	vec3 diffused = vec3(0);
	for(int i=0;i<CubeMapsCount;i++){
		#define CUT 7.0
		if(distance(data.worldPos, CubeMapsPositions[i].xyz) < CUT) {
			float fv = 1.0 - smoothstep(0.0, CUT, distance(data.worldPos, CubeMapsPositions[i].xyz));
			cube = samplerCube(CubeMapsAddrs[i]);
			vec3 dirvis = -normalize(CubeMapsPositions[i].xyz - data.worldPos);

			precentage = MMALGetShadowforFuckSake(distance(data.worldPos, CubeMapsPositions[i].xyz), currentFragment.worldPos, dirvis);
			reflected += fv * MMAL(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), data.normal, dir, data.roughness, i) * data.specularColor ;
			//reflected = vec3(0);
			diffused += fv * MMALNoPrcDiffuse(dirvis, distance(data.worldPos, CubeMapsPositions[i].xyz), normalize(data.normal), 1.0, i) * data.diffuseColor;
		}
	}
	//if(DisablePostEffects == 1){reflected *= 0.6;diffused *= 0.6;}
    return (reflected + diffused) * 0.5;
	//return vec3(vdir);
}
