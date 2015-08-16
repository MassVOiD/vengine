#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include UsefulIncludes.glsl

#define mPI (3.14159265)
#define mPI2 (2.0*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;

layout(binding = 0) uniform sampler2D color;
layout(binding = 1) uniform sampler2D depth;
layout(binding = 2) uniform sampler2D fog;
layout(binding = 3) uniform sampler2D fogDepth;
layout(binding = 4) uniform sampler2D lightpoints;
layout(binding = 6) uniform sampler2D globalIllumination;
layout(binding = 7) uniform sampler2D diffuseColor;
layout(binding = 8) uniform sampler2D normals;
layout(binding = 9) uniform sampler2D worldPos;
layout(binding = 10) uniform sampler2D lastworldPos;
layout(binding = 11) uniform sampler2D meshData;

float centerDepth;
uniform float Brightness;
uniform int UseLightPoints;

const float[7] binarysearch = float[7](0.5, 0.75, 0.875, 0.375, 0.625, 0.01, 0.98);

float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, sampler2D sampler){
        float ret = 0;
        for(float i=0; i<1; i+=0.1)
                ret = max(mix(v1, v2, i) - texture(sampler, mix(p1, p2, i)).r, ret);
        if(abs(ret) > 0.1) return 0;
        return clamp(abs(ret) - 0.0006, 0, 1);
}

bool testVisibilityLowRes(vec2 uv1, vec2 uv2) {
	float d3d1 = texture(depth, uv1).r;
	float d3d2 = texture(depth, uv2).r;

	return 0 == textureMaxFromLine(d3d1, d3d2, uv1, uv2, depth);
}

vec2 getProjNormal(){
	vec3 positionCenter = FromCameraSpace(texture(worldPos, UV).rgb);
	vec3 normalCenter = texture(normals, UV).rgb;
	vec3 dirPosition = positionCenter + normalCenter * 0.05;

	vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4((positionCenter), 1.0);
	vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	clipspace = (ProjectionMatrix * ViewMatrix) * vec4((dirPosition), 1.0);
	vec2 sspace2 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
	return normalize(sspace2 - sspace1);
}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
vec3 DamnReflections()
{
	//float reflectionStrength = texture(meshData, UV).r;
	//if(reflectionStrength < 0.05) return vec3(0);
	vec3 originalColor = (texture(color, UV).rgb * 12 + texture(diffuseColor, UV).rgb * GIDiffuseComponent) * 0.7;
	//if(length(originalColor) > 0.8) discard;
	// Get some basic data for processed pixel, like normal, original color, position in world space
	// distance to camera, and precalculate some used data too.
	vec3 normalCenter = texture(normals, UV).rgb;
	float specSize = texture(normals, UV).a;
	// Good to mix direct light color with diffuse color
	vec3 positionCenter = FromCameraSpace(texture(worldPos, UV).rgb);
	vec2 ssnormaldir = getProjNormal();
	float speccomp = 1;
	float distanceToCamera = distance(CameraPosition, positionCenter);
	vec3 outBuffer = vec3(0);
	vec3 cameraSpace = positionCenter - CameraPosition;
	// We are going to sample the scene 256 times per frame.
	//float seeduv = rand(UV);
	#define samplesCount 15
    int sampls = 0;
	float specmax = -998790;
	vec3 speccolor = vec3(0);
    vec3 closestColor = vec3(0);
    vec2 clouv = UV;
    float iter = 9.0 / (resolution.x);
	for(float g = 0.0; g < 1.0; g += iter )
	{

		vec2 coord = UV + (ssnormaldir * g);
		//coord.x = (((coord.x - 0.5) * 2) * 1.05 / 2) + 0.5;
		if(coord.x < 0 || coord.x > 1 || coord.y < 0 || coord.y > 1) break;
		// Let's test visibility

		vec3 c = texture(color, coord).rgb;
		vec3 worldPosition = FromCameraSpace(texture(worldPos, coord).rgb);
		vec3 lightRelativeToVPos = worldPosition - positionCenter;
		vec3 R = reflect(cameraSpace, normalCenter.xyz);
		float cosAlpha = dot(normalize(R), normalize(lightRelativeToVPos));
        
        float fresn = 1.0 - max(0, dot(normalize(CameraDirection), normalize(normalCenter)));
        fresn = fresn * fresn * fresn * 0.2 + 0.8;
		if(cosAlpha > 0.991 && length(c) > 0.05){
			if(testVisibilityLowRes(coord, UV))
			{
                closestColor = c;
                clouv = coord;
                float cull = min(min(min(smoothstep(0.0, 0.05, coord.x), smoothstep(0.0, 0.05, coord.y)), smoothstep(1.0, 0.95, coord.x)), smoothstep(1.0, 0.95, coord.y));
            // find closer amtch
                float highestCos = cosAlpha;
                /*for(float i=0;i<1.0;i+=0.1){
                    vec2 dir = (normalize(ssnormaldir) * i * 0.01);
                    coord = UV + (ssnormaldir * g) + dir;
                    c = texture(color, coord).rgb;
                    closestColor += c * cull;

                    coord = UV + (ssnormaldir * g) - dir;
                    c = texture(color, coord).rgb;

                    closestColor += c * cull;
                    coord = UV + (ssnormaldir * g) + dir.yx;
                    c = texture(color, coord).rgb;
                    closestColor += c * cull;
                    coord = UV + (ssnormaldir * g) - dir.yx;
                    c = texture(color, coord).rgb;
                    closestColor += c * cull;
                }*/
				//speccolor += blurByUV(color, clouv, distance(worldPosition, positionCenter));
                speccolor += (closestColor / 1) * fresn;
                sampls++;
                break;
			}
		}


	}
	return (sampls == 0 ? closestColor : (speccolor));
}
void main()
{
    vec3 color1 = vec3(0);
    //color1 += DamnReflections();
    outColor = vec4(clamp(color1, 0, 1), 1);
}
