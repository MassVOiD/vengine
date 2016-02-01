#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

uniform int Numbers[12];
uniform int NumbersCount;


uniform float Brightness;

out vec4 outColor;




uniform float LensBlurAmount;
uniform float CameraCurrentDepth;

uniform int DisablePostEffects;
float centerDepth;
#define mPI (3.14159265)

float ngonsides = 5;
float sideLength = sqrt(1+1-2*cos(mPI2 / ngonsides));

float PIOverSides = mPI2/ngonsides;
float PIOverSidesOver2 = PIOverSides/2;
float triangleHeight = 0.85;
uniform int ShowSelected;
uniform int UnbiasedIntegrateRenderMode;


float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
vec3 lensblur(float amount, float depthfocus, float max_radius, float samples){
    vec3 finalColor = vec3(0);  
    float weight = 0.0;//vec4(0.,0.,0.,0.);  
    if(amount < 0.05) amount = 0.05;
    amount -= 0.05;
    //amount = max(0, amount - 0.1);
    //return textureLod(currentTex, UV, amount*2).rgb;
    float radius = max_radius;  
    float centerDepthDistance = abs((centerDepth) - (depthfocus));
    //float centerDepth = texture(texDepth, UV).r;
    float focus = length(reconstructCameraSpace(vec2(0.5), 0));
    float cc = texture(distanceTex, UV).r;
    for(float x = 0; x < mPI2; x+=0.2){ 
        for(float y=0.1;y<1.0;y+= 0.08){  
            
            //ngon
            
            vec2 crd = vec2(sin(x + y*5) * ratio, cos(x + y*5)) * y;
            //float alpha = texture(alphaMaskTex, crd*1.41421).r;
            //if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.02 * amount;  
            coord = clamp(coord, 0.0, 1.0);
            //coord.x = clamp(abs(coord.x), 0.0, 1.0);
            //coord.y = clamp(abs(coord.y), 0.0, 1.0);
            float depth = texture(distanceTex, coord).r;
            vec3 texel = textureMSAAFull(forwardOutputTex, coord).rgb;
            float w = length(texel) + 0.1;
            float dd = length(crd * 0.1 * amount)/0.125;
            
            w += (smoothstep(0.1, 0.0, abs(y - 0.9)));
			w *= clamp(1.0 - smoothstep(0.0, 6.7 * amount, abs(depth - cc)) + step(amount, 2.0) * step(0, depth - cc) + 
			(1.0 - ( step(0.1, focus - depth) * 
			(step(0.1, focus - cc)))), 0.0, 1.0);
            weight+=w;
            finalColor += texel * w;
            
        }
    }
    return weight == 0.0 ? vec3(0.0) : finalColor/weight;
}


vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}

#include noise3D.glsl

uniform float InputFocalLength;
float avgdepth(vec2 buv){
    float outc = float(0);
    float counter = 0;
    float fDepth = length(reconstructCameraSpace(vec2(0.5, 0.5), 0).rgb);
    //
            //vec2 gauss = buv + vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.05);
            //gauss = clamp(gauss, 0.0, 0.90);
            float adepth = texture(distanceTex, buv).r;
            //if(adepth < fDepth) adepth = fDepth + (fDepth - adepth);
            //float avdepth = clamp(pow(abs(depth - focus), 0.9) * 53.0 * LensBlurAmount, 0.0, 4.5 * LensBlurAmount);        
            float f = InputFocalLength;
            //float f = 715.0; //focal length in mm
            float d = fDepth*1000.0; //focal plane in mm
            float o = adepth*1000.0; //depth in mm
            
            float fstop = 64.0 / LensBlurAmount;
            float CoC = 1.0;
            float a = (o*f)/(o-f); 
            float b = (d*f)/(d-f); 
            float c = (d-f)/(d*fstop*CoC); 
            
            float blur = abs(a-b)*c;
            outc += blur;
            counter++;
     //   }
   // }
    return min(abs(outc / counter), 2.0);
}



vec3 ExecutePostProcessing(vec3 color, vec2 uv){
	float vignette = distance(vec2(0), vec2(0.5)) - distance(uv, vec2(0.5));
	vignette = 0.1 + 0.9*smoothstep(0.0, 0.3, vignette);
    return vec3pow(color.rgb, 1.0) * vignette * Brightness;
}


void main()
{
    vec4 color1 = textureMSAAFull(forwardOutputTex, UV);
	//color1.rgb = texture(distanceTex, UV).rrr;
    if(texture(distanceTex, UV).r < 0.01)color1.rgb = vec3(0);
    //color1.rgb = funnybloom(UV);
    //vec3 avg = getAverageOfAdjacent(UV);
   // if(distance(getAverageOfAdjacent(UV), texture(currentTex, UV).rgb) > 0.6) color1.rgb = avg;
    //vec4 color1 = vec4(edgeDetect(UV), 1.0);
    //if(ShowSelected == 1) color1.rgb += lookupSelected(UV, 0.02);
    //vec4 color1 = vec4(0,0,0,1);
    if(LensBlurAmount > 0.001 && DisablePostEffects == 0){
        float focus = CameraCurrentDepth;
        float adepth = texture(distanceTex, vec2(0.5)).r;
        //float fDepth = reverseLog(CameraCurrentDepth);

        color1.xyz = lensblur(avgdepth(UV), adepth, 0.99, 7.0);
    }
	
    
    //if(UseBloom == 1 && DisablePostEffects == 0) color1.xyz += lookupBloomBlurred(UV, 0.1).rgb;  
    //if(DisablePostEffects == 0)color1.xyz = hdr(color1.xyz, UV);
    //if(DisablePostEffects == 0)color1.rgb = ExecutePostProcessing(color1.rgb, UV);
        
	//color1.rgb += SSIL();
	
	//color1.rgb += vec3pow(texture(normalsTex, UV).rgb, 2);
	
	//color1.rgb = texture(distanceTex, UV).rrr * 0.1;
	
    vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2);
    color1.rgb = vec3(pow(color1.r, gamma.r),
    pow(color1.g, gamma.g),
    pow(color1.b, gamma.b));

    outColor = clamp(vec4(color1.rgb, toLogDepthEx(texture(distanceTex, UV).r, 1000.0)), 0.0, 10000.0);
}