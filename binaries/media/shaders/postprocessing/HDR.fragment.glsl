#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include FXAA.glsl

uniform int Numbers[12];
uniform int NumbersCount;


uniform float Brightness;

out vec4 outColor;




uniform float LensBlurAmount;
uniform float CameraCurrentDepth;

	
float centerDepth;
#define mPI (3.14159265)

float ngonsides = 5;
float sideLength = sqrt(1+1-2*cos(mPI2 / ngonsides));

float PIOverSides = mPI2/ngonsides;
float PIOverSidesOver2 = PIOverSides/2;
float triangleHeight = 0.85;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
	vec3 finalColor = vec3(0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    float radius = max_radius;  
	float centerDepthDistance = abs((centerDepth) - (depthfocus));
	//float centerDepth = texture(texDepth, UV).r;
    float focus = length(texture(worldPosTex, vec2(0.5)).rgb);
    for(float x = 0; x < mPI2; x+=0.5){ 
        for(float y=0.1;y<1.0;y+= 0.1){  
			
			//ngon
		
			vec2 crd = vec2(sin(x + y) * ratio, cos(x + y)) * (rand(UV + vec2(x, y)) * 0.125);
			//if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.01 * amount;  
			//coord.x = clamp(abs(coord.x), 0.0, 1.0);
			//coord.y = clamp(abs(coord.y), 0.0, 1.0);
            float depth = length(texture(worldPosTex, coord).xyz);
            if(distance(coord, UV.xy) < max_radius){  
                //if((depth - focus) > 0.005) continue;     
                vec3 texel = texture(currentTex, coord).rgb;
                float w = length(texel) + 0.2;
                float dpf = abs(focus - (depth))*0.2+0.8;
                w*=dpf;
                weight+=w;
                finalColor += texel * w;
            
            }
        }
    }
	return weight == 0.0 ? vec3(0.0) : finalColor/weight;
}

uniform int UseBloom;
vec3 lookupBloomBlurred(vec2 buv, float radius){
	vec3 outc = vec3(0);
	float counter = 0;
	for(float g = 0; g < mPI2*2; g+=0.3)
	{ 
		for(float g2 = 0; g2 < 3.14; g2+=0.3)
		{ 
            float h = rand(UV+vec2(g, g2));
			vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (h * radius);
			vec4 color = texture(bloomTex, buv + gauss).rgba * (log(1.0-h + 1)*2);
            float w = max(0, (length(color) - 1.3) ) * 1.1;
            counter += max(w, 0.1);
			outc += (color.rgb * color.a) * w ;
			//counter++;
		}
	}
	return outc / counter;
}
#include noise3D.glsl
vec3 lookupSelected(vec2 buv, float radius){
	vec3 outc = vec3(0);
	float counter = 0;
    float s = texture(meshDataTex, buv ).r;
    if(s == 1) return vec3(0);
	for(float g = 0; g < mPI2*2; g+=0.3)
	{ 
		for(float g2 = 0; g2 < 3.14; g2+=0.3)
		{ 
            float h = rand(UV+vec2(g, g2));
			vec2 gauss =buv +  vec2(sin(g + g2)*ratio, cos(g + g2)) * (h * radius);
			vec3 color = vec3(0.5, 0.6, 1.0) * (log(1.0-h + 1)*2);
			float selected = texture(meshDataTex, gauss).r;
            if(selected == 1){
                counter += 1;
                outc += color ;
            }
			//counter++;
		}
	}
	return counter == 0 ? vec3(0) : (outc / counter) * (snoise(vec3(buv.x*400, buv.y*400, Time*55.4))*0.5+0.5);
}

float avgdepth(vec2 buv){
	float outc = float(0);
    float fDepth = length(texture(worldPosTex, vec2(0.5, 0.5)).rgb);

    vec3 color = texture(worldPosTex, buv).xyz;
    float adepth = length(color);
    //float avdepth = clamp(pow(abs(depth - focus), 0.9) * 53.0 * LensBlurAmount, 0.0, 4.5 * LensBlurAmount);		
    float f = (LensBlurAmount)*1; //focal length in mm
    float d = fDepth*1000.0; //focal plane in mm
    float o = adepth*1000.0; //depth in mm
    
    float fstop = 16.0;
    float CoC = 0.03;
    float a = (o*f)/(o-f); 
    float b = (d*f)/(d-f); 
    float c = (d-f)/(d*fstop*CoC); 
    
    float blur = abs(a-b)*c;
    outc += clamp(blur * 50,0.0,4.0) * 10;

	return (outc);
}
void main()
{
	vec4 color1 = fxaa(currentTex, UV);
    //color1.rgb += lookupSelected(UV, 0.02);
    //vec4 color1 = vec4(0,0,0,1);
	float depth = texture(depthTex, UV).r;
	centerDepth = depth;
	if(LensBlurAmount > 0.001){
		float focus = CameraCurrentDepth;
		float adepth = length(texture(worldPosTex, UV).xyz);
		//float fDepth = reverseLog(CameraCurrentDepth);

		color1.xyz = lensblur(avgdepth(UV), 1, 0.09, 7.0);
	}
    float letterpixels = 10;
    float maxx = NumbersCount * (0.5 / letterpixels);
    if(UV.x < maxx && UV.y < 0.05){
        vec2 nuv = vec2(UV.x / maxx, UV.y / 0.05);
        float letterx = 1.0 / letterpixels;
        vec2 nuv2 = vec2(mod(UV.x / maxx, letterx), 1.0 - UV.y / 0.05);
        for(int i=0;i<NumbersCount;i++){
            vec2 numbUVOffset = vec2(i*letterx, 0);
            if(nuv.x > numbUVOffset.x && nuv.x < numbUVOffset.x + letterx){
                vec2 numbUV = vec2(Numbers[i]*letterx, 0) + nuv2;
                float data = texture(numbersTex, numbUV).a;
                color1 += data;
            }
        }
    }
				
	if(UseBloom == 1) color1.xyz += lookupBloomBlurred(UV, 0.1).rgb * BloomContribution;  
    color1.a = texture(depthTex, UV).r;
    outColor = clamp(color1, 0.0, 1.0);
}