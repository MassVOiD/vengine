#version 430 core

in vec2 UV;
#include Lighting.glsl

layout(binding = 8) uniform sampler2D texColor;
layout(binding = 9) uniform sampler2D texDepth;


out vec4 outColor;

uniform vec2 resolution;


vec2 hash2x2(vec2 co) {
	return vec2(
	fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453),
	fract(sin(dot(co.yx ,vec2(12.9898,78.233))) * 43758.5453));
}

	float ratio = resolution.y/resolution.x;
	
vec3 ball(vec3 colour, float sizec, float xc, float yc){
	float xdist = (abs(UV.x - xc));
	float ydist = (abs(UV.y - yc)) * ratio;
	
	//float d = (xdist * ydist);
	float d = sizec / length(vec2(xdist, ydist));
	
	return colour * (d);
}

void main()
{

	vec3 color1 = texture(texColor, UV).rgb;
	
	//hdr but disabled
	/*
	float maxbrightness = 1.0;
	float tmplen = 0.0;
	for(float i = 0; i < 1.0; i+=0.2){
		for(float g = 0; g < 1.0; g+=0.2){
			vec2 hash = vec2(0.5) + (vec2(i,g) - vec2(0.5)) * 0.1;
			tmplen = length(texture(texColor, hash).rgb);
			if(maxbrightness < tmplen) maxbrightness = tmplen;
		}
	}
	if(maxbrightness < 1.0) maxbrightness = 1.0;
	maxbrightness = 1.0 / log(maxbrightness * 2.0 + 1.0);
	*/

	for(int i=0;i<LightsCount;i++){
		vec4 clipspace = (ProjectionMatrix * ViewMatrix) * vec4(LightsPos[i], 1.0);
		vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		if(clipspace.z < 0.0) continue;
		
		vec4 clipspace2 = (LightsPs[i] * LightsVs[i]) * vec4(CameraPosition, 1.0);
		if(clipspace2.z < 0.0) continue;
		vec2 sspace = ((clipspace2.xyz / clipspace2.w).xy + 1.0) / 2.0;
		float dist = distance(CameraPosition, LightsPos[i]);
		dist = log(LogEnchacer*dist + 1.0) / log(LogEnchacer*LightsFarPlane[i] + 1.0);
		float percent = lookupDepthFromLight(i, sspace);
		dist = 1.0f - (dist - percent);
		if(dist > 1) {
			color1 += ball(vec3(1),3.0 / distance(CameraPosition, LightsPos[i]), sspace1.x, sspace1.y);
			color1 += ball(vec3(1),250.0 / distance(CameraPosition, LightsPos[i]), sspace1.x, sspace1.y) * 0.1f;
		}
	}

	if(UV.x > 0.49 && UV.x < 0.51 && abs(UV.y - 0.5) < 0.0003) color1 = vec3(0);
	if(UV.y > 0.48 && UV.y < 0.52 && abs(UV.x - 0.5) < 0.0003) color1 = vec3(0);
	
	//color1 *= 1.0 - (pow(distance(UV, vec2(0.5, 0.5)) * 2.0, 2));
		
	color1.x = log(color1.x + 1.0);
	color1.y = log(color1.y + 1.0);
	color1.z = log(color1.z + 1.0);
	
	
		
    outColor = vec4(color1, 1);
	
}