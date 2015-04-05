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
layout(binding = 2) uniform sampler2D diffuseColor;
layout(binding = 3) uniform sampler2D worldPos;
layout(binding = 4) uniform sampler2D normals;
layout(binding = 5) uniform sampler2D lastGi;


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
	float visible = 1.0;
	// raymarch thru
	for(float i=0;i<1.0;i+= 0.1){ 
		vec2 ruv = mix(uv1, uv2, i);
		float rd3d = texture(depth, ruv).r;
		if(rd3d < mix(d3d1, d3d2, i)){
			visible -= 0.1; 
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

vec3 circleLookupGIBounce2(vec2 centerUV, vec3 ocolor){
	vec3 original = ocolor;
	//if(length(original) < 0.02) return vec3(0); //  it wont affect any GI so ignore
	vec3 normalCenter = texture(normals, centerUV).rgb;
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	vec3 outc = vec3(0);
	int counter = 0;
	// 2 pi is about 6.28
			
	for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 1.0; g2 < 6.0; g2+=1.0)
		{ 
			counter++;
			vec2 coord = centerUV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.076)) / (distanceToCamera + 1.0);
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
			
	for(float g = 0; g < mPI2 * 4; g+=GOLDEN_RATIO)
	{ 
		for(float g2 = 2.0; g2 < 21.0; g2+=1.0)
		{ 
			counter++;
			vec2 coord = centerUV + (vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.116)) / (distanceToCamera + 1.0);
			if(coord.x < -0.2 || coord.x > 1.2 || coord.y < -0.2 || coord.y > 1.2) break;
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance > 15.0) continue;
			if(worldDistance < 0.001) continue;
			if(testVisibility(coord, centerUV)) {
				vec3 normalThere = texture(normals, coord).rgb;			
				float dotdiffuse = dot(normalCenter, normalThere);
				float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
				if(diffuseComponent > 0.0 && worldDistance < 5.0){
					vec3 c = texture(color, coord).rgb * 15 / worldDistance * diffuseComponent;
					//outc += circleLookupGIBounce2(coord, c);
					outc += c;
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
	if(counter == 0) return vec3(0);
	vec3 result = outc / counter * 5;
	return result ;
}

vec3 bruteForceGIBounce3(vec2 centerUV, vec3 orgColor){
	float lumens = length(orgColor) + 1.0;
	if(lumens < 1.01) return vec3(0);
	//lumens *= 8.0;
	vec3 original = orgColor;
	vec3 outc = vec3(0);
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	int counter = 0;
	for(float g = 0; g < 1; g += 0.5 / lumens)
	{ 
		for(float g2 = 0; g2 < 1; g2 += 0.5 / lumens)
		{ 
			counter++;
			vec2 coord = vec2(g, g2);
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(testVisibility(coord, centerUV)) {
				vec3 c = texture(color, coord).rgb / worldDistance;
				outc += c;
			}
		}
	}
	if(counter == 0) return vec3(0);
	vec3 result = outc / counter;
	return result ;
}

vec3 bruteForceGIBounce2(vec2 centerUV, vec3 orgColor){
	float lumens = length(orgColor) + 1.0;
	if(lumens < 1.08) return vec3(0);
	lumens = 1.0 + lumens * 0.20;
	vec3 original = orgColor;
	vec3 atCenter = texture(color, centerUV).rgb;
	if(length(atCenter) < 0.07) return vec3(0); //  it wont affect any GI so ignore
	vec3 outc = vec3(0);
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	int counter = 0;
	for(float g = 0; g < 1; g += 0.2709 / lumens)
	{ 
		for(float g2 = 0; g2 < 1; g2 += 0.2187 / lumens)
		{ 
			counter++;
			vec2 coord = vec2(g, g2);
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance > 5.0) continue;
			if(testVisibility(coord, centerUV)) {
				vec3 c = (texture(color, coord).rgb * 150) / worldDistance;
				outc += c;
				//outc += bruteForceGIBounce3(coord, c);
			}
		}
	}
	if(counter == 0) return vec3(0);
	vec3 result = (outc / counter);
	return atCenter * result ;
}

#include noise2d.glsl

vec3 bruteForceGIBounce1(vec2 centerUV){
	vec4 normalCenter4 = texture(normals, centerUV).rgba;
	if(normalCenter4.a == 0) return vec3(0);
	vec3 original = texture(color, centerUV).rgb + texture(diffuseColor, centerUV).rgb * 0.2;
	if(length(original) < 0.07) return vec3(0); //  it wont affect any GI so ignore
	vec3 normalCenter = normalCenter4.rgb;
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	vec3 outc = vec3(0);
	int counter = 0;
	vec3 CRel = CameraPosition - positionCenter;
	const float quality = 1.0;
	for(float g = 0; g < 1; g += 0.0977 / quality)
	{ 
		for(float g2 = 0; g2 < 1; g2 += 0.0919 / quality)
		{ 
			counter++;
			vec2 coord = vec2(rand(vec2(g, g2) * RandomSeed), rand(vec2(g2*RandomSeed, g-g2)* Time));
			//vec2 coord = vec2(g, g2);
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance < 0.02) continue;
			//if(worldDistance > 5.0) continue;
			if(testVisibility(coord, centerUV)) {
				vec4 normalThere = texture(normals, coord).rgba;	
				float dotdiffuse = dot(normalCenter, normalThere.xyz);
				float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
				if(diffuseComponent > 0.0){
					vec3 c = (texture(color, coord).rgb  * 350) / worldDistance;
					outc += c * diffuseComponent;
					
					vec3 lightRelativeToVPos = wpos - positionCenter;
					vec3 R = reflect(lightRelativeToVPos, normalCenter.xyz);
					float cosAlpha = -dot(normalize(CRel), normalize(R));
					float s = smoothstep(0.95, 1.79, cosAlpha) * 66.0;
					float specularComponent = s*s;
					//outc += (c + bruteForceGIBounce2(coord, c));
					outc += c * specularComponent * 10;
					
				}
				//outc += 1.0;
			}
		}
	}
	if(counter == 0) return vec3(0);
	vec3 result = (outc / counter);
	return original * (result / (distanceToCamera)) * 15;
}

vec3 closeRadiosity(vec2 centerUV){
	vec3 original = texture(diffuseColor, centerUV).rgb;
	//if(length(original) < 0.02) return vec3(0); //  it wont affect any GI so ignore
	vec3 normalCenter = texture(normals, centerUV).rgb;
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = -1; g < 1; g += 0.05)
	{ 
		for(float g2 = -1; g2 < 1; g2 += 0.05)
		{ 
			counter++;
			vec2 coord = centerUV + (vec2(g, g2) * 3) / (distanceToCamera + 1.0);
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance < 0.01) continue;
			if(worldDistance > 9.0) continue;
			if(testVisibility(coord, centerUV)) {
				vec3 normalThere = texture(normals, coord).rgb;			
				float dotdiffuse = dot(normalCenter, normalThere);
				float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
				if(diffuseComponent > 0.0){
					vec3 c = texture(diffuseColor, coord).rgb * 5 / worldDistance * diffuseComponent;
					outc += c;
					if(length(outc) > 0.01)break;
				}
				//outc += 1.0;
			}
			if(length(outc) > 0.01)break;
		}
	}
	if(counter == 0) return vec3(0);
	vec3 result = outc / counter;
	return original * result;
}

vec3 radiosityAmbient(vec2 centerUV){
	
	//if(length(original) < 0.02) return vec3(0); //  it wont affect any GI so ignore
	vec4 normalCenter = texture(normals, centerUV).rgba;
	if(normalCenter.a == 0) return vec3(0);
	vec3 positionCenter = texture(worldPos, centerUV).rgb;	
	float distanceToCamera = distance(CameraPosition, positionCenter);
	if(distanceToCamera < 0.2) return vec3(0);
	vec3 outc = vec3(0);
	int counter = 0;
	for(float g = -1; g < 1; g += 0.07)
	{ 
		for(float g2 = -1; g2 < 1; g2 += 0.07)
		{ 
			counter++;
			vec2 coord = centerUV + (vec2(g, g2)) / (distanceToCamera + 1.0);
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance < 0.01) continue;
			if(worldDistance > 3.0) continue;

			outc += testVisibility(coord, centerUV) ? 1:0;

		}
	}
	if(counter == 0) return vec3(0);
	vec3 result = outc / counter;
	return result;
}

#define BUFFER 4.0
#define BUFFER1 (BUFFER+1.0)

void main()
{

	vec3 color1 = (texture(lastGi, UV).rgb * BUFFER + vec3(bruteForceGIBounce1(UV))) / BUFFER1;
	//vec3 color1 = vec3(0);
	
	gl_FragDepth = texture(depth, UV).r;
	
    outColor = vec4(color1, 1);
}