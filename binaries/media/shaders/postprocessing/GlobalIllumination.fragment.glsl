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
//layout(binding = 7) uniform sampler2D backFacesColor;
//layout(binding = 8) uniform sampler2D backFacesDepth;
//layout(binding = 9) uniform sampler2D backNormals;

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
    #define qualityx1 9.77 / (11.8)
    //float qualityy = 0.0919 / 1.8;
	const float rseed = RandomSeed3 * 144.2345 + Time;
    for(float g = 0; g < 100; g += qualityx1) { 
        //for(float g2 = 0; g2 < 1; g2 += qualityy) { 
            counter++;
			//float rd = rand(vec2(rseed * g));
            //vec2 coord = vec2(rand(vec2(g, rseed) * RandomSeed1), rand(vec2(RandomSeed1, g)* UV));
			//vec2 coord = vec2(rand(vec2(g, rseed) * RandomSeed1 * barycentric.z), rand(vec2(RandomSeed1, g) * barycentric.xy));
			
			//vec2 coord = vec2(fract(rd), fract(rd*12.545));
            //vec2 coord = vec2(rand(vec2(g, rseed)), rand(vec2(g, RandomSeed1)));
			
			float rd = rand(vec2(rseed * g * UV.x, RandomSeed1 * g * UV.y));
			vec2 coord = vec2((rd), fract(rd*12.545));
		
            if(testVisibility(coord, centerUV)) {
                //vec4 normalThere = texture(normals, coord).rgba;    
                //float dotdiffuse = dot(normalCenter, normalThere.xyz);
                //float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
                //if(diffuseComponent > 0.1) {
					vec3 wpos = texture(worldPos, coord).rgb;
					float worldDistance = distance(positionCenter, wpos);
					if(worldDistance < 0.02) continue;
					float att = 1.0 / pow(((worldDistance/1.0) + 1.0), 2.0) * 90.0;
					vec3 g = texture(lastGi, coord).rgb;
                    vec3 c = (texture(color, coord).rgb  * 66 + g * 3.7) * att;
					//float lumens = length(c);
					//if(lumens > 0.1){
						//outc += c * diffuseComponent;
						vec3 lightRelativeToVPos = wpos - positionCenter;
						vec3 R = reflect(lightRelativeToVPos, normalCenter.xyz);
						float cosAlpha = -dot(normalize(CRel), normalize(R));
						float limit = 0.991 + 0.0099 / att / att;
						if(cosAlpha > limit){
							float s = smoothstep(limit, 1.0, cosAlpha);
							outc += c * s * 11;
						}
					//}
               // }
				/*// backpart
                normalThere = texture(backNormals, coord).rgba;  
                dotdiffuse = dot(normalCenter, normalThere.xyz);
                diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
                if(diffuseComponent > 0.0) {
                    vec3 c = (texture(backFacesColor, coord).rgb  * 350) / worldDistance;
					float lumens = length(c);
					if(lumens > 0.1){
						outc += c * diffuseComponent;
					}
                }*/
            }
        //}
    }	
	if(counter == 0) return vec3(0);
	vec3 res = vec3(0);
	if(counter != 0) res = original * ((outc / counter)) * (distanceToCamera / 16 * (texture(ssnormals, centerUV).a));
	if(abs(texture(lastGiDepth, centerUV).r - centerDepth) < 0.0001){
		vec3 g = texture(lastGi, centerUV).rgb;
		if(length(g) > length(res) + 0.05) return g;
		else return res;
	} else {	
		return res;
	}
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
    #define qualityx 9.77 / (11.8)
	const float rseed = RandomSeed3 * 15.2345824;
    for(float g = 0; g < 100; g += qualityx) { 
		counter++;
		float rd = rand(vec2(rseed * g * UV.x, RandomSeed1 * g * UV.y));
		vec2 coord = vec2(fract(rd), fract(rd*12.545));
			
		// fastest
		//vec2 coord = vec2(rand(vec2(g, rseed)), rand(vec2(g, RandomSeed1)));
		
		vec3 normalThere = texture(normals, coord).rgb;    
		float dotdiffuse = abs(dot(normalCenter, normalThere));
		float diffuseComponent = 1.0 - clamp(dotdiffuse, 0, 1);
		if(testVisibility(coord, centerUV)) {
			vec3 c = (texture(color, coord).rgb * 3 + texture(diffuseColor, coord).rgb) * 0.7;
			outc += (c) * diffuseComponent;
			
		}
    }
	vec3 res = vec3(0);
	if(counter != 0) res = original * ((outc / counter)) * (distanceToCamera / 16 * (texture(ssnormals, centerUV).a));
	if(abs(texture(lastGiDepth, centerUV).r - centerDepth) < 0.00001){
		vec3 g = texture(lastGi, centerUV).rgb;
		if(length(g) > length(res) + 0.05) return (res + g) / 2;
		else return res;
	} else {	
		return res;
	}
}

#define BUFFER 1.0
#define BUFFER1 (1.11)
void main() {
    centerDepth = texture(depth, UV).r;
	//vec3 lgi = texture(lastGi, UV).rgb;
	mediump vec3 color1 = vec3(0);
	//vec3 gi = bruteForceGIBounce1(UV);
	vec3 gi =  lowquality(UV);
	//if(length(lgi) > 0.001 && !(lgi.x == 1.0 && lgi.y == 1.0 && lgi.z == 1.0)){
	//	color1 = (lgi * BUFFER + vec3(gi)) / BUFFER1;
	//} else {
		color1 = gi;
	//}
	gl_FragDepth = centerDepth;
    outColor = vec4(color1, 1);
}