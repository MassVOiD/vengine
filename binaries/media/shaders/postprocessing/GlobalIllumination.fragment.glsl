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
layout(binding = 6) uniform sampler2D ssnormals;

bool testVisibility(vec2 uv1, vec2 uv2) {
    float d3d1 = texture(depth, uv1).r;
    float d3d2 = texture(depth, uv2).r;
    for(float i=0;i<1.0;i+= 0.1) { 
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

vec3 bruteForceGIBounce1(vec2 centerUV) {
    vec4 normalCenter4 = texture(normals, centerUV).rgba;
    if(normalCenter4.a == 0) return vec3(0); // lighting disabled for that object
    vec3 original = clamp(texture(color, centerUV).rgb + texture(diffuseColor, centerUV).rgb, 0, 1) * 0.2;
    if(length(original) < 0.07) return vec3(0); //  it wont affect any GI so ignore
    vec3 normalCenter = normalCenter4.rgb;
    vec3 positionCenter = texture(worldPos, centerUV).rgb;  
    float distanceToCamera = distance(CameraPosition, positionCenter);
    if(distanceToCamera < 0.2) return vec3(0);
    vec3 outc = vec3(0);
    int counter = 0;
    vec3 CRel = CameraPosition - positionCenter;
    const float quality = 1.0;
    for(float g = 0; g < 1; g += 0.0977 / quality) { 
        for(float g2 = 0; g2 < 1; g2 += 0.0919 / quality) { 
            counter++;
            vec2 coord = vec2(rand(vec2(g, g2) * RandomSeed), rand(vec2(g2*RandomSeed, g-g2)* Time));
            vec3 wpos = texture(worldPos, coord).rgb;
            float worldDistance = distance(positionCenter, wpos);
            if(worldDistance < 0.02) continue;
            if(testVisibility(coord, centerUV)) {
                vec4 normalThere = texture(normals, coord).rgba;    
                float dotdiffuse = dot(normalCenter, normalThere.xyz);
                float diffuseComponent = 1.0 - clamp(dotdiffuse, 0.0, 1.0);
                if(diffuseComponent > 0.0) {
                    vec3 c = (texture(color, coord).rgb  * 350) / worldDistance;
					float lumens = length(c);
					if(lumens > 0.1){
						outc += c * diffuseComponent;
						vec3 lightRelativeToVPos = wpos - positionCenter;
						vec3 R = reflect(lightRelativeToVPos, normalCenter.xyz);
						float cosAlpha = -dot(normalize(CRel), normalize(R));
						if(cosAlpha > 0.7 && lumens > 0.5){
							float s = smoothstep(0.95, 1.79, cosAlpha) * 66.0;
							float specularComponent = s;
							outc += c * specularComponent * 10;
						}
					}
                }
            }
        }
    }
    if(counter == 0) return vec3(0);
    return original * ((outc / counter) / (distanceToCamera)) * 35;
}

#define BUFFER 7.0
#define BUFFER1 (BUFFER+1.0)
void main() {
	vec3 lgi = texture(lastGi, UV).rgb;
	vec3 color1 = vec3(0);
	if(length(lgi) > 0.001 && !(lgi.x == 1.0 && lgi.y == 1.0 && lgi.z == 1.0)){
		color1 = (lgi * BUFFER + vec3(bruteForceGIBounce1(UV))) / BUFFER1;
	} else {
		color1 = vec3(bruteForceGIBounce1(UV));
	}
    gl_FragDepth = texture(depth, UV).r;
    outColor = vec4(color1 * (texture(ssnormals, UV).a), 1);
}