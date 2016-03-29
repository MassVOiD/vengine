#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

out vec4 outColor;

uniform int Voxelize_GridSizeX;
uniform int Voxelize_GridSizeY;
uniform int Voxelize_GridSizeZ;
uniform vec3 Voxelize_BoxSize;
uniform vec3 Voxelize_MapPosition;

vec3 getMapCoord(vec2 UV){
    return (FromCameraSpace(reconstructCameraSpaceDistance(UV, textureMSAAFull(normalsDistancetex, UV).a)) - Voxelize_MapPosition) / Voxelize_BoxSize;
}

vec3 getNormalV(vec3 coord){
    return normalize(textureLod(voxelsNormalsTex, coord, 0).rgb);
}

vec4 sampleCone(vec3 coord, float blurness){
    if(blurness < 0.2){
        float bl = blurness / 0.2;
        vec4 i1 = textureLod(voxelsTex1, coord, 0).rgba;
        vec4 i2 = textureLod(voxelsTex2, coord, 0).rgba;
        return mix(i1, i2, bl);
    } else if(blurness < 0.4){
        float bl = (blurness - 0.2) / 0.2;
        vec4 i1 = textureLod(voxelsTex2, coord, 0).rgba;
        vec4 i2 = textureLod(voxelsTex3, coord, 0).rgba;
        return mix(i1, i2, bl);
    } else if(blurness < 0.6){
        float bl = (blurness - 0.4) / 0.2;
        vec4 i1 = textureLod(voxelsTex3, coord, 0).rgba;
        vec4 i2 = textureLod(voxelsTex4, coord, 0).rgba;
        return mix(i1, i2, bl);
    } else if(blurness < 0.8){
        float bl = (blurness - 0.6) / 0.2;
        vec4 i1 = textureLod(voxelsTex4, coord, 0).rgba;
        vec4 i2 = textureLod(voxelsTex5, coord, 0).rgba;
        return mix(i1, i2, bl);
    } else {
        float bl = (blurness - 0.8) / 0.2;
        vec4 i1 = textureLod(voxelsTex5, coord, 0).rgba;
        vec4 i2 = textureLod(voxelsTex5, coord, 0).rgba;
        return mix(i1, i2, bl);
    } 
}

// Y up in this case, tangent space
// needs normalization
vec3 coneDirections[] = vec3[](
    vec3(1, 0.3, 0),
    vec3(-1, 0.3, 0),
    
    vec3(0, 0.3, 1),
    vec3(0, 0.3, -1),
    
    vec3(1, 0.3, 1),
    vec3(-1, 0.3, 1),
    
    vec3(1, 0.3, -1),
    vec3(-1, 0.3, -1),
    
    
    vec3(0, 1, 0)
    
);

layout(binding = 12) uniform samplerCube cube;
#define MMAL_LOD_REGULATOR 512
vec3 MMALGI(vec3 dir, float roughness){
	//roughness = roughness * roughness;
    float levels = max(0, float(textureQueryLevels(cube)) - 2);
    float mx = log2(roughness*MMAL_LOD_REGULATOR+1)/log2(MMAL_LOD_REGULATOR);
    vec3 result = textureLod(cube, dir, mx * levels).rgb;
	
	return result;
	//return vec3pow(result*1.5, 6.0);
}
vec3 traceConeSpecular(){ 
    //vec3 csp = reconstructCameraSpaceDistance(UV, textureMSAAFull(normalsDistancetex, UV).a) / Voxelize_BoxSize;///Voxelize_BoxSize;
    vec3 csp = getMapCoord(UV);
    csp = clamp(csp * 0.5 + 0.5,0.0, 1.0);
    vec3 center = csp;
    vec3 norm = normalize(textureMSAA(normalsDistancetex, UV, 0).rgb);
	float roughness = max(0.01, textureMSAA(albedoRoughnessTex, UV, 0).a);
    vec3 dir = mix(normalize(reflect(reconstructCameraSpace(UV), textureMSAA(normalsDistancetex, UV, 0).rgb)), normalize(textureMSAA(normalsDistancetex, UV, 0).rgb), roughness * roughness);
    float w = 1.0;
    vec3 res = vec3(0);
    float st = 0.0;
    float blurness = 1.0;
    float rg2 =  sqrt(sqrt(sqrt(roughness)));
    for(int i=0;i < 50;i++){
        vec3 c = center + dir * (st * st + 0.02) * 0.4;
        //float lv = mix(0.0, levels, st * 0.8);
        vec4 rc = sampleCone(clamp(c, 0.0, 1.0), roughness * (1.0 - blurness));
       // rc *= max(0, dot(getNormalV(clamp(c, 0.0, 1.0)), -dir));
        res += w * rc.rgb * mix(1.0, 0.0, rg2);
        w -= min(rc.a, 1.0) * 0.4 * st;
        w = max(0, w);
        if(w == 0.0) break;
        st += 0.02;
        blurness = blurness * 0.9;
    }
    //res += w * MMALGI(dir, roughness);
    return res * 1;
}

vec3 getTangent(vec3 v){
	return normalize(v) == vec3(0,1,0) ? vec3(1,0,0) : normalize(cross(vec3(0,1,0), v));
}


mat3 rotationMatrix(vec3 axis, float angle)
{
	axis = normalize(axis);
	float s = sin(angle);
	float c = cos(angle);
	float oc = 1.0 - c;
	
	return mat3(oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s, 
	oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s, 
	oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c);
}
vec3 traceConeDiffuse(){ 
    //vec3 csp = reconstructCameraSpaceDistance(UV, textureMSAAFull(normalsDistancetex, UV).a) / Voxelize_BoxSize;///Voxelize_BoxSize;
    vec3 csp = getMapCoord(UV);
    csp = clamp(csp * 0.5 + 0.5,0.0, 1.0);
    vec3 center = csp;
    vec3 norm = normalize(textureMSAA(normalsDistancetex, UV, 0).rgb);	
    vec3 reflected = normalize(reflect(reconstructCameraSpace(UV), textureMSAA(normalsDistancetex, UV, 0).rgb));
    vec3 tang = getTangent(norm);
    mat3 TBN = mat3(
		normalize(tang),
		normalize(cross(norm, (tang))),
		normalize(norm)
	);   
	float roughness = textureMSAA(albedoRoughnessTex, UV, 0).a;
    float w = 1.0;
    vec3 res = vec3(0);
    float st = 0.04;
    float blurness = 0.7;
    mat3 rot = rotationMatrix(norm, rand2s(UV) * 2 * 3.1415);
    for(int i=0;i < coneDirections.length();i++){
        vec3 dir = rot * (TBN * normalize(coneDirections[i])) ;
    
        vec3 c = center + dir * (st) * 0.5; 

        st = 0.0;
        float w = 1.0;
        for(int g=0;g<10;g++){
            c = center + dir * (st * st + 0.02) * 0.7;
            vec4 rc = sampleCone(clamp(c, 0.0, 1.0), (1.0 - blurness));
            res += rc.rgb * w;
            w -= min(rc.a, 1.0) * 0.9 * st;
            w = max(0, w);
            blurness = blurness * 0.6;
            st += 0.1;
        }
        
       
    }
    
    return res * 0.4;
}
float traceConeAO(){ 
    //vec3 csp = reconstructCameraSpaceDistance(UV, textureMSAAFull(normalsDistancetex, UV).a) / Voxelize_BoxSize;///Voxelize_BoxSize;
    vec3 csp = getMapCoord(UV);
    csp = clamp(csp * 0.5 + 0.5,0.0, 1.0);
    vec3 center = csp;

    float ao = max(0, 1.0 - (textureLod(voxelsTex2, center, 0).a * 0.1));
    ao += max(0, 1.0 - (textureLod(voxelsTex4, center, 0).a * 0.2));
    ao += max(0, 1.0 - (textureLod(voxelsTex5, center, 0).a * 0.4));
    
    return ao * 0.3;
}

float traceVisDir(vec3 dir){ 
    //vec3 csp = reconstructCameraSpaceDistance(UV, textureMSAAFull(normalsDistancetex, UV).a) / Voxelize_BoxSize;///Voxelize_BoxSize;
    vec3 csp = getMapCoord(UV);
    csp = clamp(csp * 0.5 + 0.5,0.0, 1.0);
    vec3 center = csp;
    
    float vis = 1.0;

    center += dir * 0.04;
    for(int i=0;i < 10;i++){
        center += dir * 0.04;
        vis *= max(0, 1.0 - (textureLod(voxelsTex6, center, 0).a * 0.1));
    }
    
    return vis;
}


void main()
{
    vec3 color = vec3(0);
    
	vec4 albedoRoughnessData = textureMSAA(albedoRoughnessTex, UV, 0);
	vec4 normalsDistanceData = textureMSAA(normalsDistancetex, UV, 0);
	vec4 specularBumpData = textureMSAA(specularBumpTex, UV, 0);
    //color = vec3(1) * traceConeAO();;
    //color = traceConeDiffuse() * texture(aoTex, UV).r;// + traceConeDiffuse() * traceConeAO();;
    //color +=  traceConeDiffuse() * albedoRoughnessData.rgb ;
    color +=  traceConeSpecular() * specularBumpData.rgb  + traceConeDiffuse() * albedoRoughnessData.rgb;
    
    outColor = clamp(vec4(color, 0), 0.0, 10000.0);
}