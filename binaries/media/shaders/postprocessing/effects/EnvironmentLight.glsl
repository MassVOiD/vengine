vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}
#define MMAL_LOD_REGULATOR 512
vec3 MMAL(vec3 normal, vec3 reflected, float roughness){
    float levels = float(textureQueryLevels(cubeMapTex)) - 1;
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
    vec3 result = textureLod(cubeMapTex, mix(reflected, normal, roughness), mx * levels).rgb;
    //return vec3pow(result * 2.0, 1.7)*0.5;
    return result;
}

vec3 EnvironmentLight(FragmentData data)
{       
    vec3 dir = normalize(reflect(data.cameraPos, data.normal));
    vec3 vdir = normalize(data.cameraPos);
	
    vec3 reflected = MMAL(data.normal, dir, data.roughness) * data.specularColor;
    vec3 diffused = MMAL(data.normal, dir, 1.0) * data.diffuseColor;
	    
    return makeFresnel(1.0 - max(0, dot(data.normal, vdir)), reflected + diffused);
}
