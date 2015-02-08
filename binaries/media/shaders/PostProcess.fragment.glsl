#version 430 core

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 2) uniform sampler2D light1C;
layout(binding = 3) uniform sampler2D light1D;

in vec2 UV;

#include Mesh3dUniforms.glsl

out vec4 outColor;

in vec2 resolution;


vec2 hash2x2(vec2 co) {
	return vec2(
	fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453),
	fract(sin(dot(co.yx ,vec2(12.9898,78.233))) * 43758.5453));
}

vec3 ball(vec3 colour, float sizec, float xc, float yc){
	return colour * (sizec / distance(UV, vec2(xc, yc)));
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
		vec4 clipspace = ProjectionMatrix * ViewMatrix * vec4(LightsPos[i], 1.0);
		vec2 sspace = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
		color1 += ball(vec3(1),6.0/ distance(LightsPos[i], CameraPosition), sspace.x, sspace.y);
	}

	if(UV.x > 0.49 && UV.x < 0.51 && abs(UV.y - 0.5) < 0.0003) color1 = vec3(0);
	if(UV.y > 0.48 && UV.y < 0.52 && abs(UV.x - 0.5) < 0.0003) color1 = vec3(0);
	
	//color1 *= 1.0 - (pow(distance(UV, vec2(0.5, 0.5)) * 2.0, 2));
		
	color1.x = log(color1.x + 1.0);
	color1.y = log(color1.y + 1.0);
	color1.z = log(color1.z + 1.0);
	
	
		
    outColor = vec4(color1, 1);
	
}