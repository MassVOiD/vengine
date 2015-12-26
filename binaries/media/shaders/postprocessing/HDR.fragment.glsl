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
    //if(amount < 0.02) return texture(currentTex, UV).rgb;
    //amount -= 0.02;
	//amount = max(0, amount - 0.1);
	//return textureLod(currentTex, UV, amount*2).rgb;
    float radius = max_radius;  
    float centerDepthDistance = abs((centerDepth) - (depthfocus));
    //float centerDepth = texture(texDepth, UV).r;
    float focus = length(reconstructCameraSpace(vec2(0.5)));
    float cc = length(reconstructCameraSpace(UV).rgb);
    for(float x = 0; x < mPI2; x+=0.1){ 
        for(float y=0.1;y<1.0;y+= 0.04){  
            
            //ngon
            
            vec2 crd = vec2(sin(x + y*5) * ratio, cos(x + y*5)) * y;
            //float alpha = texture(alphaMaskTex, crd*1.41421).r;
            //if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.02 * amount;  
            //coord.x = clamp(abs(coord.x), 0.0, 1.0);
            //coord.y = clamp(abs(coord.y), 0.0, 1.0);
            float depth = reverseLog(texture(depthTex, coord).r);
            vec3 texel = texture(currentTex, coord).rgb;
            float w = length(texel) + 0.1;
            float dd = length(crd * 0.1 * amount)/0.125;
            w *= dd;
            
			w += (smoothstep(0.1, 0.0, abs(y - 0.9)));
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


uniform int UseBloom;
vec3 lookupBloomBlurred(vec2 buv, float radius){
    vec3 outc = vec3(0);
    float counter = 0;
	

	//outc += textureLod(currentTex, buv, 7).rgb;
	outc += textureLod(currentTex, buv, 8).rgb;
	outc += textureLod(currentTex, buv, 9).rgb;
	outc += textureLod(currentTex, buv, 10).rgb;
	outc += textureLod(currentTex, buv, 11).rgb;
	//outc *= max(0.0, length(outc) - 1.0) * 0.4;
	return vec3pow(outc * 1.1, 1.3) * 0.11;
	
}
vec3 funnybloom(vec2 buv){
    vec3 outc = vec3(0);

	outc += textureLod(currentTex, buv, 0).rgb;
	outc += textureLod(currentTex, buv, 1).rgb;
	outc += textureLod(currentTex, buv, 2).rgb;
	outc += textureLod(currentTex, buv, 3).rgb;
	outc += textureLod(currentTex, buv, 4).rgb;
	outc += textureLod(currentTex, buv, 5).rgb;
	outc += textureLod(currentTex, buv, 6).rgb;
	outc += textureLod(currentTex, buv, 7).rgb;
	outc += textureLod(currentTex, buv, 8).rgb;
	outc += textureLod(currentTex, buv, 9).rgb;
	outc += textureLod(currentTex, buv, 10).rgb;
	outc += textureLod(currentTex, buv, 11).rgb;
	return outc / 12.0;
	
}
#include noise3D.glsl

uniform float InputFocalLength;
float avgdepth(vec2 buv){
    float outc = float(0);
    float counter = 0;
    float fDepth = length(reconstructCameraSpace(vec2(0.5, 0.5)).rgb);
    for(float g = 0; g < mPI2; g+=0.4)
    { 
        for(float g2 = 0; g2 < 1.0; g2+=0.11)
        { 
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.05);
            float adepth = (reverseLog(texture(depthTex, buv + gauss).r));
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
        }
    }
    return min(abs(outc / counter), 14.0);
}



vec3 ExecutePostProcessing(vec3 color, vec2 uv){

	return vec3pow(color.rgb, 1.9);
}

vec3 hdr(vec3 color, vec2 uv){
	float levels = float(textureQueryLevels(currentTex)) - 1;
	vec3 refbright = textureLod(currentTex, uv,levels).rgb;
	float reflen = length(refbright);
	float mult = reflen > mPI2 ? 0 : cos(reflen);
	return color * max(0.5, mult) * 2.0;
}

void main()
{
    vec4 color1 = texture(currentTex, UV);
	//color1.rgb = funnybloom(UV);
    //vec3 avg = getAverageOfAdjacent(UV);
   // if(distance(getAverageOfAdjacent(UV), texture(currentTex, UV).rgb) > 0.6) color1.rgb = avg;
    //vec4 color1 = vec4(edgeDetect(UV), 1.0);
    //if(ShowSelected == 1) color1.rgb += lookupSelected(UV, 0.02);
    //vec4 color1 = vec4(0,0,0,1);
    float depth = texture(depthTex, UV).r;
    centerDepth = depth;
    if(LensBlurAmount > 0.001 && DisablePostEffects == 0){
        float focus = CameraCurrentDepth;
        float adepth = length(reconstructCameraSpace(vec2(0.5)).xyz);
        //float fDepth = reverseLog(CameraCurrentDepth);

        color1.xyz = lensblur(avgdepth(UV), adepth, 0.99, 7.0);
    }
    float letterpixels = 10;
    float maxx = NumbersCount * (1.0 / letterpixels);
    if(DisablePostEffects == 0 && UV.x < maxx && UV.y < 0.05){
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
    
    if(UseBloom == 1 && DisablePostEffects == 0) color1.xyz += lookupBloomBlurred(UV, 0.1).rgb;  
	//if(DisablePostEffects == 0)color1.xyz = hdr(color1.xyz, UV);
	//if(DisablePostEffects == 0)color1.rgb = ExecutePostProcessing(color1.rgb, UV);
    color1.a = texture(depthTex, UV).r;
    
    vec3 last = texture(lastIndirectTex, UV).rgb;
    float f1 = length(last) / length(vec3(1));
    float f2 = length(color1.rgb);
    
	vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2);
	color1.rgb = vec3(pow(color1.r, gamma.r),
	pow(color1.g, gamma.g),
	pow(color1.b, gamma.b));
    vec3 additiveMix = mix(last, color1.rgb, UnbiasedIntegrateRenderMode == 1 ? 0.04538 : 1.0);
    if(UnbiasedIntegrateRenderMode == 1){
        //additiveMix *= texture(HBAOTex, UV).a;
       // if(abs(texture(lastIndirectTex, UV).a - depth) > 0.0003) additiveMix = color1.rgb;
    }
    //additiveMix = texture(diffuseColorTex, UV).rgb;
	//additiveMix = vec3(1);
    outColor = clamp(vec4(additiveMix, depth), 0.0, 10000.0);
}