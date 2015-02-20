#version 430 core

in vec2 UV;
#include Lighting.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;


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

float getNearDiff(float originalDepth){
	float diff = 0.0;
	int counter = 0;
	for(float i=-2.0;i<2.0;i+=0.5){
		for(float g=-2.0;g<2.0;g+=0.5){
			float depth = texture(texDepth, UV + vec2(i/460.0,g/460.0)).r;
			if(abs(depth - originalDepth) > 0.002)diff += 1.0;
			counter++;
		}
	}
	return pow(diff/counter, 1.0);
}
float getAveragedDepth(){
	float diff = 0.0;
	int counter = 0;
	for(float i=-2.0;i<2.0;i+=0.3){
		for(float g=-2.0;g<2.0;g+=0.3){
			float depth = texture(texDepth, UV + vec2(i*0.005*ratio,g*0.005)).r;
			diff += depth;
			counter++;
		}
	}
	return diff/counter;
}
float getNearDiffByColor(vec3 originalColor){
	float diff = 0.0;
	for(int i=-2;i<2;i++){
		for(int g=-2;g<2;g++){
			vec3 color = texture(texColor, UV + vec2(i/400.0,g/400.0)).rgb;
			diff += distance(originalColor, color);
		}
	}
	return diff;
}

vec3 blur(float amount){
	vec3 outc = vec3(0);
	for(int g = 0; g < 14; g++){ 
		for(int g2 = 0; g2 < 14; g2++){ 
			vec2 gauss = vec2(getGaussianKernel(g) * amount, getGaussianKernel(g2) * amount);
			vec3 color = texture(texColor, UV + gauss).rgb;
			outc += color;
		}
	}
	return outc / (14.0*14.0);
}

vec3 lensblur(float amount, float max_radius, int samples){
	vec3 finalColor = vec3(0.0,0.0,0.0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	//float centerDepth = texture(texDepth, UV).r;
    for(int x=samples*-1;x<samples;x++) {  
        for(int y=samples*-1;y<samples;y++){  
            vec2 coord = UV+(vec2(float(samples) / x * ratio, float(samples) / y) * 0.001 * amount);  
			coord.x = clamp(coord.x, 0.0, 1.0);
			coord.y = clamp(coord.y, 0.0, 1.0);
            if(distance(coord, UV.xy) < float(max_radius)){  
                //float depth = texture(texDepth, coord).r;
				//if(depth - centerDepth < 0.01){
				vec3 texel = texture(texColor, coord).rgb;
				float w = length(texel)+0.1;  
				weight+=w;  
				finalColor += texel*w;  
				//}
            }  
        }  
    } 
	return finalColor/weight;	
}

void main()
{

	vec3 color1 = texture(texColor, UV).rgb;
	float depth = texture(texDepth, UV).r;
	
	//FXAA
	//float edge = getNearDiff(depth);
	//if(edge > 0.002)color1 = blur(edge * 0.1);
	//color1 = vec3(edge);
	float focus = 0.09;
	float avdepth = getAveragedDepth();
	color1 = lensblur(clamp(abs(avdepth - focus) * 7.0, 0.0, 4.5), 0.009, 8);
	
	//color1 -= pow(distance(UV, vec2(0.5, 0.5)), 8);
	
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
			color1 += ball(vec3(1),1.0 / distance(CameraPosition, LightsPos[i]), sspace1.x, sspace1.y);
			color1 += ball(vec3(1),250.0 / distance(CameraPosition, LightsPos[i]), sspace1.x, sspace1.y) * 0.1f;
		} else {
			//color1 += ball(vec3(dist),3.0 / distance(CameraPosition, LightsPos[i]), sspace1.x, sspace1.y);
			//color1 += ball(vec3(dist),250.0 / distance(CameraPosition, LightsPos[i]), sspace1.x, sspace1.y) * 0.1f;
		}
		// now the radial blur goes on
		/*float pxDistance = distance(UV, sspace1);
		vec2 direction = (sspace1 - UV) / pxDistance;
		vec3 colorSum = vec3(0);
		for(int g=0;g<10;g++){
			colorSum += texture(texColor, UV + (direction / 200.0) * pxDistance).rgb;
		}
		colorSum /= 10.0;
		//colorSum = colorSum.x + colorSum.y + colorSum.z > 0.9*3 ? (colorSum - 0.9) * 10.0 : vec3(0); // clip to 
		color1 += colorSum;
		//color1 = colorSum;
		*/
	}
	
	//color1 = edge > 0.01 ? vec3(1) : vec3(0);

	if(UV.x > 0.49 && UV.x < 0.51 && abs(UV.y - 0.5) < 0.0003) color1 = vec3(0);
	if(UV.y > 0.48 && UV.y < 0.52 && abs(UV.x - 0.5) < 0.0006) color1 = vec3(0);
	
	//color1 *= 1.0 - (pow(distance(UV, vec2(0.5, 0.5)) * 2.0, 2));
		
	//if(UV.x > 0.5){
	//	color1.x = log(color1.x + 1.0);
	//	color1.y = log(color1.y + 1.0);
	//	color1.z = log(color1.z + 1.0);
	//}
		
    outColor = vec4(color1, 1);
	
}