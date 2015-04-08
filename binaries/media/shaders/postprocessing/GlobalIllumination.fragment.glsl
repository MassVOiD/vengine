#version 430 core

#include Lighting.glsl

in vec2 UV;
out vec4 outColor;

layout(binding = 0) uniform sampler2D color;
layout(binding = 1) uniform sampler2D depth;
layout(binding = 2) uniform sampler2D diffuseColor;
layout(binding = 3) uniform sampler2D worldPos;
layout(binding = 4) uniform sampler2D normals;
layout(binding = 5) uniform sampler2D lastGi;
layout(binding = 6) uniform sampler2D lastGiDepth;
layout(binding = 7) uniform sampler2D ssnormals;
layout(binding = 8) uniform sampler2D backFacesColor;
layout(binding = 9) uniform sampler2D backFacesDepth;
//layout(binding = 9) uniform sampler2D backNormals;

bool testVisibilityHiRes(vec2 uv1, vec2 uv2) {
    float d3d1 = texture(depth, uv1).r;
    float d3d2 = texture(depth, uv2).r;
	float d = (distance(uv1, uv2) + 1.0) * 3 * 0.2;
    for(float i=0;i<1.0;i+= d) { 
        vec2 ruv = mix(uv1, uv2, i);
        float rd3d = texture(depth, ruv).r;
        float bd = texture(backFacesDepth, ruv).r;
        if(rd3d < mix(d3d1, d3d2, i) && rd3d + (rd3d - bd) < mix(d3d1, d3d2, i)) {
            return false;
        }
    }
    return true;
}
bool testVisibility(vec2 uv1, vec2 uv2) {
    float d3d1 = texture(depth, uv1).r;
    float d3d2 = texture(depth, uv2).r;
	float d = (distance(uv1, uv2) + 1.0) * 3 * 0.1;
    for(float i=0;i<1.0;i+= d) { 
        vec2 ruv = mix(uv1, uv2, i);
        float rd3d = texture(depth, ruv).r;
        if(rd3d < mix(d3d1, d3d2, i)) {
            return false;
        }
    }
    return true;
}

mediump float rand(vec2 co) {
    mediump float a = 12.9898; mediump float b = 78.233; mediump float c = 43758.5453; 
	mediump float dt= dot(co.xy ,vec2(a,b)); mediump float sn= mod(dt,3.14);
    return fract(sin(sn) * c);
}

#include noise2D.glsl
float centerDepth;

vec3 bruteForceGIBounce1(vec2 centerUV) {
    vec4 normalCenter4 = texture(normals, centerUV).rgba;
    //if(normalCenter4.a == 0) return vec3(0); // lighting disabled for that object
    vec3 original = (texture(color, centerUV).rgb * 3 + texture(diffuseColor, centerUV).rgb) * 0.7;
    //if(length(original) < 0.07) return vec3(0); //  it wont affect any GI so ignore
    vec3 normalCenter = normalCenter4.rgb;
    vec3 positionCenter = texture(worldPos, centerUV).rgb;  
    float distanceToCamera = distance(CameraPosition, positionCenter);
    //if(distanceToCamera < 0.2) return vec3(0);
    vec3 outc = vec3(0);
    int counter = 0;
    vec3 CRel = CameraPosition - positionCenter;
    #define samples 53.54331
    for(float g = 0; g < samples; g += 1.112313) { 			
		float rd = rand(vec2(RandomSeed3 * 0.063123 * g * UV.x, RandomSeed1 * g * UV.y));
		vec2 coord = vec2((rd), fract(rd*12.545));
        //for(float g2 = 0; g2 < 1; g2 += qualityy) { 
            counter++;
            //vec2 coord = vec2(rand(vec2(g, rseed) * RandomSeed1), rand(vec2(RandomSeed1, g)* UV));
			//vec2 coord = vec2(rand(vec2(g, rseed) * RandomSeed1 * barycentric.z), rand(vec2(RandomSeed1, g) * barycentric.xy));
			
			//float rd = rand(vec2(rseed * g));
			//vec2 coord = vec2(cos(rd), sin(rd*12.545));
            //vec2 coord = vec2(rand(vec2(g, rseed)), rand(vec2(g, RandomSeed1)));
			
			//float rd = rand(vec2(rseed * g * UV.x, RandomSeed1 * g * UV.y));
			//vec2 coord = vec2((rd), fract(rd*12.545));
		
			vec3 wpos = texture(worldPos, coord).rgb;
			float worldDistance = distance(positionCenter, wpos);
			if(worldDistance < 0.02) continue;
			float att = 1.0 / pow(((worldDistance/1.0) + 1.0), 2.0) * 90.0;
			vec3 ga = texture(lastGi, coord).rgb;
			vec3 c = ((texture(color, coord).rgb + texture(diffuseColor, coord).rgb)  * 7 + ga * 1.7) * att;
            if(testVisibility(coord, centerUV)) {
                vec4 normalThere = texture(normals, coord).rgba;    
                float dotdiffuse = dot(normalCenter, normalThere.xyz);
                float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
                if(diffuseComponent > 0.1) {
					outc += c * diffuseComponent;
                }
				vec3 lightRelativeToVPos = wpos - positionCenter;
				vec3 R = reflect(lightRelativeToVPos, normalCenter.xyz);
				float cosAlpha = -dot(normalize(CRel), normalize(R));
				if(cosAlpha > 0.99){
					outc += c * 6;
				}
            }
        //}
    }	
	vec3 res = original * ((outc / samples)) * (distanceToCamera / 16 * (texture(ssnormals, centerUV).a));
	/*if(abs(texture(lastGiDepth, centerUV).r - centerDepth) < 0.0001){
		vec3 g = texture(lastGi, centerUV).rgb;
		if(length(g) > length(res) + 0.05) return g;
		else return res;
	} else {	
		return res;
	}*/
	return res;
}
vec3 lowquality(vec2 centerUV) {
    vec4 normalCenter4 = texture(normals, centerUV).rgba;
    //if(normalCenter4.a == 0) return vec3(0); // lighting disabled for that object
    vec3 original = (texture(color, centerUV).rgb * 3 + texture(diffuseColor, centerUV).rgb) * 0.7;
    vec3 positionCenter = texture(worldPos, centerUV).rgb;  
    float distanceToCamera = distance(CameraPosition, positionCenter);
    vec3 normalCenter = normalCenter4.rgb;
    //if(distanceToCamera < 0.2) return vec3(0);
    vec3 outc = vec3(0);
    int counter = 0;
    vec3 CRel = CameraPosition - positionCenter;
    #define qualityx 0.0977 / (111.8)
	const float rseed = snoise(15.2345824 * centerUV);
    for(float g = 0; g < 1; g += qualityx) { 
		counter++;
		vec2 coord = vec2(fract(centerUV.x * RandomSeed3 * 1.654642 * g), fract(centerUV.y * RandomSeed1 * 1.36854642  * g));
			
		if(testVisibility(coord, centerUV)) {
			vec3 c = (texture(color, coord).rgb * 3 + texture(diffuseColor, coord).rgb) * 0.7;
			outc += (c);
			
		}
    }
	vec3 res = vec3(0);
	if(counter != 0) res = original * ((outc / counter)) * (distanceToCamera / 16 * (texture(ssnormals, centerUV).a));
	return res;

}


float testVisibility3d(vec2 uv1, vec3 displaced) {
	vec3 wpos = texture(worldPos, uv1).xyz;
	vec3 dis = displaced;
	
	const mat4 vpmat = (ProjectionMatrix * ViewMatrix); 
	
    float d3d1 = distance(CameraPosition, wpos);
    float d3d2 = distance(CameraPosition, dis);
	float ou = 1;
    for(float i=0.1;i<1.0;i+= 0.02) { 
        vec3 ruv = mix(wpos, dis, i);
		vec4 clipspace = vpmat * vec4(ruv, 1.0);
		if(clipspace.z < 0.0) {ou -= 0.02; continue;}
		vec2 sspace = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
        float rd3d = distance(CameraPosition, texture(worldPos, sspace).xyz);
		float m = mix(d3d1, d3d2, i);
        if(rd3d < m && m - rd3d < 0.5) {
            ou -= 0.09;
        }
    }
    return ou;
}
vec3 ambientRadiosity(vec2 centerUV) {
    vec4 normalCenter4 = texture(normals, centerUV).rgba;
    //if(normalCenter4.a == 0) return vec3(0); // lighting disabled for that object
    vec3 original = (texture(color, centerUV).rgb * 3 + texture(diffuseColor, centerUV).rgb) * 0.7;
    vec3 positionCenter = texture(worldPos, centerUV).rgb;  
    float distanceToCamera = distance(CameraPosition, positionCenter);
    vec3 normalCenter = normalCenter4.rgb;
    //if(distanceToCamera < 0.2) return vec3(0);
    vec3 outc = vec3(0);
    int counter = 0;
    vec3 CRel = CameraPosition - positionCenter;
	mat4 PV = (ProjectionMatrix * ViewMatrix);
	#define samples 53.54331
    for(float g = 0; g < samples; g += 1.112313) { 			
		counter++;
		vec3 coord = vec3(rand(centerUV * g), rand(centerUV * RandomSeed2 + g * 10 * g), rand(centerUV / RandomSeed1 + g * 10 / g));
	
		//float rd = rand(vec2(rseed * g));
		//vec3 coord = vec3(sin(rd*5.213576), cos(rd*2.4535), cos(rd*52.1212543));
		
		coord = ((coord * 2.0) - 1.0) * 5.0;
		vec4 clipspace = PV * vec4(positionCenter + coord, 1.0);
		//if(clipspace.z < 0.0) continue;
		vec2 sspace = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
			
		if(testVisibility(centerUV, sspace)) {
			outc += original;
		}
    }
	return  original * ((outc / samples)) * ((texture(ssnormals, centerUV).a)) * 6.;
}


vec3 visibilityOnly() {
    vec3 original = texture(color, UV).rgb + texture(diffuseColor, UV).rgb;
	vec3 outc = vec3(0);
    #define samples 53.54331
    for(float g = 0; g < samples; g += 1.112313) { 			
		float rd = rand(vec2(RandomSeed3 * 0.063123 * g * UV.x, RandomSeed1 * g * UV.y));
		vec2 coord = vec2((rd), fract(rd*12.545));
		if(testVisibility(UV, coord)) {
			outc += original + texture(lastGi, coord).rgb * 2;
		}
    }
	return (outc/samples/2);
}

vec3 directional() {
    vec3 original = texture(color, UV).rgb + texture(diffuseColor, UV).rgb;
	vec3 outc = vec3(0);
	vec3 worldLightDir = vec3(1, 1, 1) * 3;
	vec3 positionCenter = texture(worldPos, UV).rgb;  
	vec3 displaced = positionCenter + worldLightDir;  
	mat4 PV = (ProjectionMatrix * ViewMatrix);
    #define samples 53.54331
    for(float g = 0.2; g < samples; g += 1.112313) { 			
		float rd = (rand(vec2(RandomSeed3 * 0.063123 * g * UV.x, RandomSeed1 * g * UV.y)) - 0.5) * 2;
		vec3 coord = mix(positionCenter, displaced, g / samples) + (vec3(1) * rd * 0.2);

		coord = ((coord * 2.0) - 1.0);
		vec4 clipspace = PV * vec4(coord, 1.0);
		//if(clipspace.z < 0.0) continue;
		vec2 sspace = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		
		outc += original * testVisibility3d(UV, coord);
		
    }
	return (outc/samples);
}

#define BUFFER 1.0
#define BUFFER1 (1.99)
void main() {
	vec3 color1 = vec3(0);
	//color1 = visibilityOnly();
	color1 = directional();
	//color1 = bruteForceGIBounce1(UV);
	//color1 *= ambientRadiosity(UV);
    centerDepth = texture(depth, UV).r;
	//vec3 lgi = texture(lastGi, UV).rgb;
	
	//if(length(lgi) > 0.001 && !(lgi.x == 1.0 && lgi.y == 1.0 && lgi.z == 1.0)){
		//color1 = (lgi * BUFFER + color1) / BUFFER1;
	//}
	gl_FragDepth = centerDepth;
    outColor = vec4(color1, 1);
}