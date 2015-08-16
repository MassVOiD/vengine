#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;
layout(binding = 32) uniform sampler2D backDepth;
layout(binding = 33) uniform sampler2D meshDataTex;

//layout (binding = 11, r32ui) volatile uniform uimage3D full3dScene;

const int MAX_SIMPLE_LIGHTS = 20;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

float meshRoughness;
float meshSpecular;
float meshDiffuse;


out vec4 outColor;
bool IgnoreLightingFragment = false;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 0;
    for(float i=0;i<1;i+=0.1)
        ret = max(mix(v1, v2, i) - texture(sampler, mix(p1, p2, i)).r, ret);

    return ret;
}
float textureMaxFromLineNegate(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
    float ret = 9999;
    for(float i=0;i<1;i+=0.33)
        ret = min(mix(v1, v2, i) - texture(sampler, mix(p1, p2, i)).r, ret);

    return ret;
}

vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}

mat4 PV = (ProjectionMatrix * ViewMatrix);
float testVisibility3d(vec2 cuv, vec3 w1, vec3 w2) {
    vec4 clipspace = (PV) * vec4((w1), 1.0);
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) * vec4((w2), 1.0);
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    float d3d1 = toLogDepth(length(ToCameraSpace(w1)));
    float d3d2 = toLogDepth(length(ToCameraSpace(w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));
    //float mx2 = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));

    return mx;
}

vec3 LightingPhysical(
    vec3 lightColor,
    float albedo,
    float gloss,
    vec3 normal,    
    vec3 lightDir,
    vec3 viewDir,
    float atten )
    {
        // calculate diffuse term
        float n_dot_l = clamp( dot( normal, lightDir ) * atten, 0.0, 1.0);
        vec3 diffuse = n_dot_l * lightColor;

        // calculate specular term
        vec3 h = normalize( lightDir + viewDir );

        float n_dot_h = clamp( dot( normal, h ), 0.0, 1.0);
        float normalization_term = ( ( meshSpecular * meshRoughness ) + 2.0 ) / 8.0;
        float blinn_phong = pow( n_dot_h, meshSpecular * meshRoughness );
        float specular_term = blinn_phong * normalization_term;
        float cosine_term = n_dot_l;

        float h_dot_l = dot( h, lightDir );
        float base = 1.0 - h_dot_l;
        float exponential =	pow( base, 5.0 );

        vec3 specColor = vec3(1) * gloss;
        vec3 fresnel_term = specColor + ( 1.0 - specColor ) * exponential;

        vec3 specular = specular_term * cosine_term * fresnel_term * lightColor;

        vec3 final_output = diffuse * ( 1 - fresnel_term );
        return final_output;
    }

float step2(float a, float b, float x){
    return step(a, x) * (1.0-step(b, x));
}
    
void main()
{

    float alpha = texture(texColor, UV).a;
    vec2 nUV = UV;
    if(alpha < 0.99){
        //nUV = refractUV();
    }
    vec3 colorOriginal = texture(texColor, nUV).rgb;
    vec4 normal = texture(normalsTex, nUV);
    meshDiffuse = normal.a;
    meshSpecular = texture(worldPosTex, nUV).a;
    meshRoughness = texture(meshDataTex, nUV).a;
    vec3 color1 = vec3(0);
    if(normal.x == 0.0 && normal.y == 0.0 && normal.z == 0.0){
        color1 = colorOriginal;
        IgnoreLightingFragment = true;
    } else {
        color1 = colorOriginal * 0.01;
    }

    //vec3 color1 = colorOriginal * 0.2;
    if(texture(texColor, UV).a < 0.99){
        color1 += texture(texColor, UV).rgb * texture(texColor, UV).a;
    }
    gl_FragDepth = texture(texDepth, nUV).r;
    vec4 fragmentPosWorld3d = texture(worldPosTex, nUV);
    vec3 cameraRelativeToVPos = -vec3(fragmentPosWorld3d.xyz);
    fragmentPosWorld3d.xyz = FromCameraSpace(fragmentPosWorld3d.xyz);


    //vec3 cameraRelativeToVPos = normalize( CameraPosition - fragmentPosWorld3d.xyz);
    float len = length(cameraRelativeToVPos);
    int foundSun = 0;

    #define RSMSamples 7
    for(int i=0;i<LightsCount;i++){
        //break;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE && foundSun == 1) continue;
        if(LightsMixModes[i] == LIGHT_MIX_MODE_SUN_CASCADE) foundSun = 1;
        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        mat4 invlightPV = inverse(LightsPs[i] * LightsVs[i]);
        vec3 centerpos = LightsPos[i];
        for(int x=0;x<RSMSamples;x++){
            for(int y=0;y<RSMSamples;y++){
                vec2 scruv = vec2(float(x) / RSMSamples, float(y) /RSMSamples);
                float ldep = lookupDepthFromLight(i, scruv);
                scruv.y = 1.0 - scruv.y;
                scruv = scruv * 2 - 1;
                vec4 reconstructDir = invlightPV * vec4(scruv, 0.01, 1.0);
                reconstructDir.xyz /= reconstructDir.w;
                vec3 dir = normalize(
                reconstructDir.xyz - centerpos
                );

                // not optimizable
                vec3 newpos = dir * reverseLog(ldep) + LightsPos[i] + (-dir * 0.1);
                float distanceToLight = distance(fragmentPosWorld3d.xyz, newpos);
                vec3 lightRelativeToVPos = normalize(newpos - fragmentPosWorld3d.xyz);
                float att = min(1.0 / pow(((distanceToLight * 0.6) + 1.0), 2.0) * 0.9, 0.007);
                float vi = testVisibility3d(nUV, fragmentPosWorld3d.xyz + lightRelativeToVPos*3.5, fragmentPosWorld3d.xyz);
                vi = (step(0.0, -vi))+smoothstep(0.0, 0.91, vi);
                //    vi = 1.0-vi;
                float fresnel = 1.0 - max(0, dot(normalize(cameraRelativeToVPos), normalize(normal.xyz)));
                fresnel = fresnel * fresnel * fresnel + 1.0;
               
                    color1 += vi* fresnel * LightingPhysical(
                        LightsColors[i].rgb*colorOriginal*LightsColors[i].a,
                        meshDiffuse,
                        meshSpecular,
                        normal.xyz,
                        (lightRelativeToVPos),
                        normalize(cameraRelativeToVPos),
                        att);
                
            }
        }
    }
 //   outColor = vec4(clamp(color1 / (RSMSamples*RSMSamples), 0, 1), 1);
    outColor = vec4(0,0,0, 1);
}
