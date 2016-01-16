

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
    return vec3pow(result * 2.0, 1.7)*0.5;
}

struct Ray{
    vec3 origin;
    vec3 direction;
};

float intersectPlane(Ray ray,vec3 point,vec3 normal)
{
    return dot(point-ray.origin,normal)/dot(ray.direction,normal);
}
vec3 EnvironmentLight(

    vec3 albedo,
    vec3 position, 
    vec3 normal, 
    float roughness, 
    float IOR)

{   
    vec3 cameraspace = ToCameraSpace(position);
    
    vec3 dir = normalize(reflect(cameraspace, normal));
    vec3 vdir = normalize(cameraspace);
     
    //vec3 reflected = textureLod(cubeMapTex, normal, 6).rgb;
    vec3 reflected = MMAL(normal, dir, roughness);
    reflected = mix(albedo * reflected, reflected, 1.0 - roughness);
    
    vec3 diffused = MMAL(normal, dir, 1.0) * albedo;
    reflected = mix((reflected + diffused)*0.5, reflected, 1.0 - roughness);
    
    //IOR = 1.0;
    vec3 refracted = vec3(0);
    /*if(IOR > 0){
        vec3 dir2 = normalize(refract(cameraspace, normal, 0.1));
        refracted = MMAL(-normal, dir2, roughness) * IOR;
        refracted = mix(refracted*albedo, refracted, 1.0 - roughness);
    }*/
    
    return makeFresnel(1.0 - max(0, dot(normal, vdir)), (reflected + refracted));
}
