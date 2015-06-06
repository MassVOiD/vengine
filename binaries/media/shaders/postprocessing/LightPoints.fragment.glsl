#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

out vec4 outColor; 

vec3 ball(vec3 colour, float sizec, float xc, float yc){
	float xdist = (abs(UV.x - xc));
	float ydist = (abs(UV.y - yc)) * ratio;
	
	float d = sizec / length(vec2(xdist, ydist));
	return colour * (d);
}

const int MAX_SIMPLE_LIGHTS = 2000;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

void main()
{
	vec3 color = vec3(0);
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);

		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4((LightsPos[i]), 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;
		
        float badass_depth = (clipspace.z / clipspace.w) * 0.5 + 0.5;	
        float logg = texture(texDepth, UV).r;
        
        if(logg > badass_depth) {
            color += ball(vec3(LightsColors[i].rgb*2.0),LightPointSize / ( badass_depth) * 0.001, sspace1.x, sspace1.y);
            //color += ball(vec3(LightsColors[i]*2.0 * overall),12.0 / dist, sspace1.x, sspace1.y) * 0.03f;
        }
    
	}
	
	for(int i=0;i<SimpleLightsCount;i++){
	
		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(SimpleLightsPos[i], 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;
		float dist = distance(CameraPosition, SimpleLightsPos[i]);
		float revlog = reverseLog(texture(texDepth, sspace1).r);
		if(dist > revlog)continue;
		dist += 1.0;
		color += ball(vec3(SimpleLightsColors[i]*2.0 * SimpleLightsColors[i].a),SimpleLightPointSize * (0.8/ dist), sspace1.x, sspace1.y);

	
	}
    outColor = vec4(color, 1);
}