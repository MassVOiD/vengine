#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl
#define mPI (3.14159265)
#define mPI2 (2*3.14159265)
#define GOLDEN_RATIO (1.6180339)
out vec4 outColor;

layout(binding = 0) uniform sampler2D color;
layout(binding = 1) uniform sampler2D depth;
layout(binding = 2) uniform sampler2D worldPos;
layout(binding = 3) uniform sampler2D normals;


bool testVisibility(vec2 uv1, vec2 uv2){
	float d3d1 = texture(depth, uv1).r;
	float d3d2 = texture(depth, uv2).r;
	bool visible = true;
	for(float i=0;i<1.0;i+= 0.1){ 
		vec2 ruv = mix(uv1, uv2, i);
		float rd3d = texture(depth, ruv).r;
		if(rd3d < mix(d3d1, d3d2, i)){
			visible = false; 
			break; 
		}
	}
	return visible;
}
bool testVisibilityLowRes(vec2 uv1, vec2 uv2){
	float d3d1 = texture(depth, uv1).r;
	float d3d2 = texture(depth, uv2).r;
	bool visible = true;
	for(float i=0;i<1.0;i+= 0.2){ 
		vec2 ruv = mix(uv1, uv2, i);
		float rd3d = texture(depth, ruv).r;
		if(rd3d < mix(d3d1, d3d2, i)){
			visible = false; 
			break; 
		}
	}
	return visible;
}
float visibilityValue(vec2 uv1, vec2 uv2){
	//vec3 wpos1 = texture(worldPos, uv1).rgb;
	//vec3 wpos2 = texture(worldPos, uv2).rgb;
	float d3d1 = texture(depth, uv1).r;
	float d3d2 = texture(depth, uv2).r;
	float distanceStep = distance(uv1, uv2) / 3;
	float visible = 1.0;
	// raymarch thru
	for(float i=0;i<1.0;i+= distanceStep){ 
		vec2 ruv = mix(uv1, uv2, i);
		float rd3d = texture(depth, ruv).r;
		if(rd3d < mix(d3d1, d3d2, i)){
			visible -= distanceStep * 2; 
			if(visible < 0) return 0;
		}
	}
	return visible;
}

float calculateSaturation(vec3 color){
	return max(max(color.r, color.g), color.b) - min(min(color.r, color.g), color.b);
}

mediump float rand(vec2 co)
{
    mediump float a = 12.9898;
    mediump float b = 78.233;
    mediump float c = 43758.5453;
    mediump float dt= dot(co.xy ,vec2(a,b));
    mediump float sn= mod(dt,3.14);
    return fract(sin(sn) * c);
}

vec3 circleLookupGIBounce2(vec2 centerUV){
	vec3 original = texture(color, centerUV).rgb;
	//if(length(original) < 0.02) return vec3(0); //  it wont affect any GI so ignore
	vec3 normalCenter = texture(normals, centerUV).rgb;
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	vec3 outc = vec3(0);
	int counter = 0;
	// 2 pi is about 6.28
			
	for(float g = 0; g < mPI2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 1.0; g2 < 6.0; g2+=1.0)
		{ 
			counter++;
			vec2 coord = centerUV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.176)) / (distanceToCamera + 1.0);
			if(coord.x < -0.2 || coord.x > 1.2 || coord.y < -0.2 || coord.y > 1.2) break;
			if(testVisibilityLowRes(coord, centerUV)) {
				outc += texture(color, coord).rgb;
			}
		}
	}
	//return (original + outc / counter * 9) / 2;
	if(counter == 0) return vec3(0);
	vec3 result = outc / counter * 5;
	//return result * (calculateSaturation(result) + 0.01) * 420;
	return result;
}

vec3 circleLookupGIBounce1(vec2 centerUV){
	vec3 original = texture(color, centerUV).rgb;
	//if(length(original) < 0.02) return vec3(0); //  it wont affect any GI so ignore
	vec3 normalCenter = texture(normals, centerUV).rgb;
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	vec3 outc = vec3(0);
	int counter = 0;
	// 2 pi is about 6.28
			
	for(float g = 0; g < mPI2 * 3; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 2.0; g2 < 21.0; g2+=1.0)
		{ 
			counter++;
			vec2 coord = centerUV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.276)) / (distanceToCamera + 1.0);
			if(coord.x < -0.2 || coord.x > 1.2 || coord.y < -0.2 || coord.y > 1.2) break;
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance > 15.0) continue;
			if(testVisibility(coord, centerUV)) {
				vec3 normalThere = texture(normals, coord).rgb;			
				float dotdiffuse = dot(normalCenter, normalThere);
				float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
				if(diffuseComponent > 0.0 && worldDistance < 5.0){
					outc += texture(color, coord).rgb * 15 / worldDistance * diffuseComponent;
					outc += circleLookupGIBounce2(coord);
				}
				//outc += 1.0;
			}
		}
	}
	//return (original + outc / counter * 9) / 2;
	if(counter == 0) return vec3(0);
	vec3 result = outc / counter * 5;
	//return result * (calculateSaturation(result) + 0.01) * 420;
	return result;
}
vec3 circleLookupGIPrecentage(){
	vec3 original = texture(color, UV).rgb;
	//vec3 normalCenter = texture(normals, UV).rgb;
	vec3 positionCenter = texture(worldPos, UV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	vec3 outc = vec3(0);
	int counter = 0;
	// 2 pi is about 6.28
	for(float g = 0; g < mPI2 * 2; g+=6.28 / 7.0)
	{ 
		for(float g2 = 1.0; g2 < 88.0; g2+=5.0)
		{ 
			vec2 coord = UV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.176)) / (distanceToCamera + 1.0);
			if(coord.x < -0.2 || coord.x > 1.2 || coord.y < -0.2 || coord.y > 1.2) break;
			outc += texture(color, coord).rgb * visibilityValue(UV, coord);
			counter++;
		}
	}
	vec3 result = outc / counter * 5;
	return result ;
}


vec3 bruteForceGI(){
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = 0; g < 1; g += 0.05)
	{ 
		for(float g2 = 0; g2 < 1; g2 += 0.05)
		{ 
			vec2 coord = vec2(g, g2);
			outc += texture(color, coord).rgb * visibilityValue(UV, coord);
			counter++;
		}
	}
	return outc / counter * 5;
}


void main()
{

	vec3 color1 = vec3(circleLookupGIBounce1(UV));
	//vec3 color1 = vec3(0);
	
	gl_FragDepth = texture(depth, UV).r;
	
    outColor = vec4(color1, 1);
}