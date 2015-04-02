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
	//vec3 wpos1 = texture(worldPos, uv1).rgb;
	//vec3 wpos2 = texture(worldPos, uv2).rgb;
	float d3d1 = texture(depth, uv1).r;
	float d3d2 = texture(depth, uv2).r;
	bool visible = true;
	// raymarch thru
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

float calculateSaturation(vec3 color){
	return max(max(color.r, color.g), color.b) - min(min(color.r, color.g), color.b);
}

vec3 visibilityExperiement(){
	vec3 original = texture(color, UV).rgb;
	//vec3 normalCenter = texture(normals, UV).rgb;
	vec3 positionCenter = texture(worldPos, UV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = 0; g < mPI2 * 3; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 2.0; g2 < 18.0; g2+=1.0)
		{ 
			vec2 coord = UV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.276)) / (distanceToCamera + 1.0);
			if(coord.x < 0 || coord.x > 1.0 || coord.y < 0 || coord.y > 1) break;
			if(testVisibility(coord, UV)) {
				//vec3 normalThere = texture(normals, coord).rgb;			
				//float dotdiffuse = dot(normalCenter, normalThere);
				//float diffuseComponent = clamp(dotdiffuse, 0.0, 1.0);
				outc += texture(color, coord).rgb;
				//outc += 1.0;
			}
			counter++;
		}
	}
	//return (original + outc / counter * 9) / 2;
	vec3 result = outc / counter;
	return result ;
}


void main()
{

	vec3 color1 = vec3(visibilityExperiement());
	
	gl_FragDepth = texture(depth, UV).r;
	
    outColor = vec4(color1, 1);
}