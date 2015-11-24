#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

uniform int UseRSM;

float meshRoughness;
float meshSpecular;
float meshDiffuse;

uniform float VDAOGlobalMultiplier;


out vec4 outColor;
bool IgnoreLightingFragment = false;


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

uniform int UseVDAO;
uniform int UseHBAO;
vec3 Radiosity()
{
    if(texture(diffuseColorTex, UV).r >= 999){ 
        return texture(cubeMapTex, normalize(texture(worldPosTex, UV).rgb)).rgb;
    }
    uint idvals = texture(meshIdTex, UV).b;
    vec3 posCenter = texture(worldPosTex, UV).rgb;
    vec3 normalCenter = normalize(texture(normalsTex, UV).rgb);
    float metalness =  texture(meshDataTex, UV).z;
    vec4 vals = unpackUnorm4x8(idvals);
    float vdaomult = vals.x * 4 + 0.1;
    float vdaorefract = vals.z;
	
    vec3 dir = normalize(reflect(posCenter, normalCenter));
    vec3 vdir = normalize(posCenter);
    vec3 dir2 = normalize(refract(posCenter, normalCenter, 0.8));
	
    vec3 vdaoMain = shadePhotonSpecular(UV, MMAL(normalCenter, dir, texture(meshDataTex, UV).a)) * vdaomult;
	
    if(metalness < 1.0){
        vec3 vdaoFullDiffuse = (shadePhoton(UV, MMAL(normalCenter, dir, 1.0))) * vdaomult;
        vdaoMain = mix((vdaoMain + vdaoFullDiffuse)*0.5, vdaoMain, metalness);
    }
	vec3 vdaoRefract = vec3(0);
    if(vdaorefract > 0){
        vdaoRefract = (shadePhoton(UV, MMAL(-normalCenter, dir2, 1.0))) * vdaorefract;
    }
    return makeFresnel(1.0 - max(0, dot(normalCenter, vdir)), (vdaoMain + vdaoRefract) * VDAOGlobalMultiplier);
}


float lookupShadow(vec2 fuv){
    float outc = 0;
    int counter = 0;
    float depthCenter = texture(depthTex, fuv).r;
    for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
    {
        for(float g2 = 0; g2 < 6.0; g2+=1.0)
        {
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.01);
            float color = texture(indirectTex, fuv + gauss).r;
            float depthThere = texture(indirectTex, fuv + gauss).g;
            if(abs(depthThere - depthCenter) < 0.01){
                outc += color;
                counter++;
            }
        }
    }
    return counter == 0 ? texture(indirectTex, fuv).r : outc / counter;
}

void main()
{   

    //  float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
    // if(alpha < 0.99){
    //nUV = refractUV();
    // }
    vec3 colorOriginal = texture(diffuseColorTex, nUV).rgb;    
    vec4 normal = texture(normalsTex, nUV);
    meshDiffuse = normal.a;
    meshSpecular = texture(worldPosTex, nUV).a;
    meshRoughness = texture(meshDataTex, nUV).a;
    float meshMetalness =  texture(meshDataTex, UV).z;
    vec3 color1 = vec3(0);
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        color1 = colorOriginal;
        IgnoreLightingFragment = true;
    } else {
        // color1 = colorOriginal * 0.01;
    }
    
    //vec3 color1 = colorOriginal * 0.2;
    //if(texture(texColor, UV).a < 0.99){
    //    color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    //}
    gl_FragDepth = texture(depthTex, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = normalize(-fragmentPosWorld3d.xyz);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    // int foundSun = 0;
    if(!IgnoreLightingFragment) for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(fragmentPosWorld3d.xyz, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

        float percent = 0;
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) {
            percent = getShadowPercent(lightScreenSpace, fragmentPosWorld3d.xyz, i);

        } else percent = lookupShadow(UV);
        vec3 radiance = shadeUV(UV, LightsPos[i], LightsColors[i]);
        color1 += (radiance) * percent;
    }
  //  if(!IgnoreLightingFragment) for(int i=0;i<SimpleLightsCount;i++){
  //      color1 += shadeUV(UV, simplePointLights[i].Position.xyz, simplePointLights[i].Color);
  //  }

    if(UseVDAO == 1 && UseHBAO == 0) color1 += Radiosity();
    if(UseVDAO == 1 && UseHBAO == 1) color1 += Radiosity() * texture(HBAOTex, UV).a;
    //   if(UseVDAO == 0 && UseHBAO == 1) color1 += texture(HBAOTex, UV).rrr;
    
    // experiment
    
    
    outColor = vec4(clamp(color1, 0.0, 9999.0), 1.0);
    
    
}