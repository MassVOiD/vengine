#include LightingSamplers.glsl
#include Mesh3dUniforms.glsl
/*
Instane lighting
Part of: https://github.com/achlubek/vengine
@author Adrian Chlubek
*/

vec2 LightScreenSpaceFromGeo[MAX_LIGHTS];
smooth in vec3 positionModelSpace;
smooth in vec3 positionWorldSpace;
smooth in vec3 normal;
smooth in vec3 barycentric;
flat in int instanceId;
uniform int UseNormalMap;

highp float rand(vec2 co)
{
    highp float a = 12.9898;
    highp float b = 78.233;
    highp float c = 43758.5453;
    highp float dt= dot(co.xy ,vec2(a,b));
    highp float sn= mod(dt,3.14);
    return fract(sin(sn) * c);
}
vec4 quat_from_axis_angle(vec3 axis, float angle)
{ 
	vec4 qr;
	float half_angle = (angle * 0.5) * 3.14159 / 180.0;
	qr.x = axis.x * sin(half_angle);
	qr.y = axis.y * sin(half_angle);
	qr.z = axis.z * sin(half_angle);
	qr.w = cos(half_angle);
	return qr;
}
vec3 rotate_vector_by_quat( vec4 quat, vec3 vec )
{
	return vec + 2.0 * cross( cross( vec, quat.xyz ) + quat.w * vec, quat.xyz );
}

vec3 rotate_vector_by_vector( vec3 vec_first, vec3 vec_sec )
{
	vec3 zeros = vec3(0.0, 1.0, 0.0);
	vec3 cr = normalize(cross(vec_first, vec_sec));
	float angle = dot(zeros, normalize(vec_sec));
	return rotate_vector_by_quat(quat_from_axis_angle(cr, angle), vec_first);
}

float specular(vec3 normalin, uint index){
	vec3 lightRelativeToVPos = LightsPos[index] - positionWorldSpace.xyz;
	vec3 cameraRelativeToVPos = CameraPosition - positionWorldSpace.xyz;
	vec3 R = reflect(lightRelativeToVPos, normalin);
	float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
	return clamp(pow(cosAlpha, 80.0 / SpecularSize), 0.0, 1.0);
}

float diffuse(vec3 normalin, uint index){
	vec3 lightRelativeToVPos = LightsPos[index] - positionWorldSpace.xyz;
	float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (normalin));
	float angle = clamp(dotdiffuse, 0.0, 1.0);
	return (angle);
}

const float gaussKernel[14] = float[14](-0.028, -0.024,-0.020,-0.016,-0.012,-0.008,-0.004,.004,.008,.012,0.016,0.020,0.024,0.028); 
float getGaussianKernel(int i){
	return gaussKernel[i];
}

float lookupDepthFromLight(uint i, vec2 uv){
	float distance1 = 0.0;
	if(i==0)distance1 = texture(lightDepth0, uv).r;
	else if(i==1)distance1 = texture(lightDepth1, uv).r;
	else if(i==2)distance1 = texture(lightDepth2, uv).r;
	else if(i==3)distance1 = texture(lightDepth3, uv).r;
	else if(i==4)distance1 = texture(lightDepth4, uv).r;
	else if(i==5)distance1 = texture(lightDepth5, uv).r;
	else if(i==6)distance1 = texture(lightDepth6, uv).r;
	else if(i==7)distance1 = texture(lightDepth7, uv).r;
	else if(i==8)distance1 = texture(lightDepth8, uv).r;
	else if(i==9)distance1 = texture(lightDepth9, uv).r;
	else if(i==10)distance1 = texture(lightDepth10, uv).r;
	else if(i==11)distance1 = texture(lightDepth11, uv).r;
	else if(i==12)distance1 = texture(lightDepth12, uv).r;
	else if(i==13)distance1 = texture(lightDepth13, uv).r;
	else if(i==14)distance1 = texture(lightDepth14, uv).r;
	else if(i==15)distance1 = texture(lightDepth15, uv).r;
	else if(i==16)distance1 = texture(lightDepth16, uv).r;
	else if(i==17)distance1 = texture(lightDepth17, uv).r;
	else if(i==18)distance1 = texture(lightDepth18, uv).r;
	else if(i==19)distance1 = texture(lightDepth19, uv).r;
	else if(i==20)distance1 = texture(lightDepth20, uv).r;
	else if(i==21)distance1 = texture(lightDepth21, uv).r;
	else if(i==22)distance1 = texture(lightDepth22, uv).r;
	else if(i==23)distance1 = texture(lightDepth23, uv).r;
	else if(i==24)distance1 = texture(lightDepth24, uv).r;
	else if(i==25)distance1 = texture(lightDepth25, uv).r;
	else if(i==26)distance1 = texture(lightDepth26, uv).r;
	else if(i==27)distance1 = texture(lightDepth27, uv).r;
	else if(i==28)distance1 = texture(lightDepth28, uv).r;
	return distance1;
}

float getBlurAmount(vec2 uv, uint i){
	float distance1 = lookupDepthFromLight(i, uv + vec2(0.004, 0.004)) + lookupDepthFromLight(i, uv + vec2(0.004, -0.004)) 
	+ lookupDepthFromLight(i, uv + vec2(-0.004, 0.004)) + lookupDepthFromLight(i, uv + vec2(-0.004, -0.004));
	return abs(distance1/4.0 - lookupDepthFromLight(i, uv) );
}
float getShadowPercent(vec2 uv, vec3 pos, uint i){
	float accum = 1.0;
	float distance2 = distance(pos, LightsPos[i]);
	//float distanceCam = distance(positionWorldSpace.xyz, CameraPosition);
	float distance1 = 0.0;
	vec2 fakeUV = vec2(0.0);
	vec2 offsetDistance = vec2(0.0);
	float badass_depth = log(LogEnchacer*distance2 + 1.0) / log(LogEnchacer*FarPlane + 1.0f);
	//float centerDiff = abs(badass_depth - lookupDepthFromLight(i, uv)) * 10000.0;
	
	//float blurAmount = getBlurAmount(uv, i);
	float blurAmount = 1;
	if(blurAmount > 0.0001){
		//float gaussKernel[14] = float[14](-0.028, -0.024,-0.020,-0.016,-0.012,-0.008,-0.004,.004,.008,.012,0.016,0.020,0.024,0.028); 
		
		
		for(int g = 0; g < 14; g+=2){ 
			vec2 gauss = vec2(0, gaussKernel[g]);
			offsetDistance = gauss * (distance2 / LightsFarPlane[i] /15.0 + 0.03f);
			fakeUV = uv + offsetDistance;
			distance1 = lookupDepthFromLight(i, fakeUV);
			float diff = abs(distance1 -  badass_depth);
			if(diff > 0.0003) accum -= 1.0/14.0;
		}
		for(int g = 0; g < 14; g+=2){ 
			vec2 gauss = vec2(gaussKernel[g], 0);
			offsetDistance = gauss * (distance2 / LightsFarPlane[i] / 15.0 + 0.03f);
			fakeUV = uv + offsetDistance;
			distance1 = lookupDepthFromLight(i, fakeUV);
			float diff = abs(distance1 -  badass_depth);
			if(diff > 0.0003) accum -= 1.0/14.0;
		}
	} else {
		distance1 = lookupDepthFromLight(i, uv);
		float diff = abs(distance1 -  badass_depth);
		if(diff > 0.0003) accum -= 1.0;
	}
	return accum;
}

vec3 processLighting(vec3 color){
	for(uint x = 0; x < LightsCount; x++){
		vec4 clipspace = vec4(0);
		clipspace = ((LightsPs[x] * LightsVs[x] * ModelMatrixes[instanceId]) * vec4(positionModelSpace, 1.0));

		vec3 tmp = clipspace.xyz / clipspace.w;
		LightScreenSpaceFromGeo[x] = clipspace.z > 0 ? (tmp.xy + 1.0) / 2.0 : vec2(10, 10);
	}
	bool shadow = false;
	int lightsIlluminating = 0;
	for(uint i = 0; i < LightsCount; i++){
		if(LightScreenSpaceFromGeo[i].x > 0.0 && LightScreenSpaceFromGeo[i].x < 1.0 &&
		LightScreenSpaceFromGeo[i].y > 0.0 && LightScreenSpaceFromGeo[i].y < 1.0) {
			shadow = true;
			lightsIlluminating++;
		}
	}
	
	vec3 normalNew  = normal;
	if(UseNormalMap == 1){
		vec3 nmap = texture(normalMap, UV).rbg * 2.0 - 1.0;
		nmap *= 1.5f; // to be sure

		normalNew = (vec4(normalize(rotate_vector_by_vector(normal, nmap)), 1)).xyz;

		//normalNew = nmap;
	}
	
	float multiplier = 0.0;
	if(DiffuseComponent < 100.0){
		vec3 specularComponent = vec3(0.0);
		vec3 diffuseComponent = vec3(0.0);
		if(shadow) {
			for(uint i = 0; i < LightsCount; i++)
			{
				float percent = clamp(getShadowPercent(LightScreenSpaceFromGeo[i], positionWorldSpace, i), 0.0, 1.0);
				multiplier += (percent);
				float culler = clamp(1.0 - distance(LightScreenSpaceFromGeo[i], vec2(0.5)) * 2.0, 0.0, 1.0);
				//float culler = clamp(1.0 - distance(LightScreenSpaceFromGeo[i], vec2(0.5)) * 2.0, 0.0, 1.0);
				specularComponent += specular(normalNew, i) * SpecularComponent * LightsColors[i].xyz * LightsColors[i].a * culler;
				diffuseComponent += diffuse(normalNew, i) * DiffuseComponent * LightsColors[i].xyz * LightsColors[i].a * culler;
			} 
		}else {
			/*for(uint i = 0; i < LightsCount; i++)
			{
				float culler = clamp(1.0 - distance(LightScreenSpaceFromGeo[i], vec2(0.5)) * 2.0, 0.0, 1.0);
				//float culler = clamp(1.0 - distance(LightScreenSpaceFromGeo[i], vec2(0.5)) * 2.0, 0.0, 1.0);
				specularComponent += specular(normalNew, i) * SpecularComponent * LightsColors[i].xyz * LightsColors[i].a * culler;
				diffuseComponent += diffuse(normalNew, i) * DiffuseComponent * LightsColors[i].xyz * LightsColors[i].a * culler;
			}*/
		}
		specularComponent = clamp(specularComponent, 0.0, 1.0);
		diffuseComponent = clamp(diffuseComponent, 0.0, 1.0);
		vec3 ambient = color * 0.03; // this is place for global ambient occlusion
		if(shadow) {
			//multiplier /= lightsIlluminating; 
			color = (color *multiplier * diffuseComponent + (specularComponent*multiplier)).xyz;
		}else {
			color = (color * diffuseComponent + (specularComponent)).xyz;
		}
		color = color + ambient;
		
		//color = vec3(diff);
	}
	//float diffuse = 1.0;
	return color.xyz;
}