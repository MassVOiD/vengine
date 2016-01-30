vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}

float CalculateFalloff( float dist){
    //return 1.0 / pow(((dist) + 1.0), 2.0);
    //return dist == 0 ? 1 : (1) / (PI*dist*dist+1);
    
   float constantAttenuation=1.0;

   float linearAttenuation=1.0;

   float quadraticAttenuation=0.02;

   float att = 1.0/(constantAttenuation + (linearAttenuation * dist) + (quadraticAttenuation * dist * dist));
   return att;
}
#extension GL_ARB_bindless_texture : require
#define MMAL_LOD_REGULATOR 512
vec3 MMAL(vec3 normal, vec3 reflected, float roughness, int i){
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
    float levels = float(textureQueryLevels(cube)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
	vec3 result = vec3(0);
	for(float x = 0.0; x < 10.0; x+= 1.0){
		vec3 rd = vec3(
			rand2s(x+currentFragment.worldPos.xy), 
			rand2s(x+currentFragment.worldPos.zx), 
			rand2s(x+currentFragment.worldPos.yz)
		) * 0.7;
		result += textureLod(cube, mix(reflected, normal + rd, roughness), mx * levels).rgb;
	}
    //return vec3pow(result * 2.0, 1.7)*0.5;
    return result * 0.1;
}

vec3 EnvironmentLight(FragmentData data)
{       
    //vec3 dir = normalize(reflect(data.cameraPos, data.normal));
    vec3 vdir = normalize(data.cameraPos);
	vec3 reflected = vec3(0);
	vec3 diffused = vec3(0);
	for(int i=0;i<CubeMapsCount;i++){
		vec3 dir = -normalize(CubeMapsPositions[i].xyz - data.worldPos);
		float falloff = 1 * CalculateFalloff(max(1.0, distance(data.worldPos, CubeMapsPositions[i].xyz) * CubeMapsFalloffs[i].x));
		
		
		float dt = smoothstep(0.0, 0.34, max(0, dot(data.normal, -normalize(data.worldPos - CubeMapsPositions[i].xyz))));
	//	falloff *= dt;
		reflected += MMAL(data.normal, dir, data.roughness, i) * data.specularColor * falloff;
		diffused += MMAL(data.normal, dir, 1.0, i) * data.diffuseColor * falloff;
	}
	    
    return makeFresnel(1.0 - max(0, dot(data.normal, vdir)), reflected + diffused);
}
