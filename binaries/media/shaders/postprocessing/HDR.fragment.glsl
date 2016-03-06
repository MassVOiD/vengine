#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl
#include FXAA.glsl

uniform int Numbers[12];
uniform int NumbersCount;


//uniform float Brightness;

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
    float focus = length(reconstructCameraSpace(vec2(0.5)));
    float cc = textureMSAA(normalsDistancetex, UV, 0).a;
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
            float depth = textureMSAA(normalsDistancetex, coord, 0).a;
            vec3 texel = texture(deferredTex, coord).rgb;
            texel += texture(ssRefTex, coord).rgb;
			if(depth < 0.001) texel = vec3(1);
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
    float fDepth = length(reconstructCameraSpace(vec2(0.5, 0.5)).rgb);
    //
            //vec2 gauss = buv + vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.05);
            //gauss = clamp(gauss, 0.0, 0.90);
            float adepth = textureMSAA(normalsDistancetex, buv, 0).a;
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

vec3 lookupSSR(vec2 fuv, float radius){
    vec3 outc = vec3(0);
    int counter = 0;
    float depthCenter = textureMSAA(normalsDistancetex, fuv, 0).a;
	vec3 normalcenter = textureMSAAFull(normalsDistancetex, fuv).rgb;
    for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO*0.5)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.1)
        {
            vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.0001 * radius);
            vec3 color = texture(ssRefTex, fuv + gauss).rgb;
            float depthThere = textureMSAA(normalsDistancetex, fuv + gauss, 0).a;
			vec3 normalthere = textureMSAAFull(normalsDistancetex, fuv).rgb;
            if(abs(depthThere - depthCenter) < 0.1 && dot(normalthere, normalcenter) > 0.9){
                outc += color;
                counter++;
            }
        }
    }
    return counter == 0 ? texture(ssRefTex, fuv).rgb : outc / counter;
}

vec3 lookupSSRLowPass(vec2 fuv, float radius){
    vec3 outc = vec3(0);
    int counter = 0;
    float depthCenter = textureMSAA(normalsDistancetex, fuv, 0).a;
	vec3 normalcenter = textureMSAAFull(normalsDistancetex, fuv).rgb;
	float lastLum = 0;
    for(float g = 0; g < mPI2 * 2; g+=GOLDEN_RATIO*0.5)
    {
        for(float g2 = 0; g2 < 1.0; g2+=0.1)
        {
            vec2 gauss = vec2(sin(g + g2*6)*ratio, cos(g + g2*6)) * (g2 * 0.007 * radius);
            vec3 color = texture(ssRefTex, fuv + gauss).rgb;
            float depthThere = textureMSAA(normalsDistancetex, fuv + gauss, 0).a;
			vec3 normalthere = textureMSAAFull(normalsDistancetex, fuv).rgb;
            if(abs(depthThere - depthCenter) < 0.05){
				float difference = abs(lastLum - length(color))*12;
				lastLum = length(color);
                outc += (1.0 / (difference + 1)) * color;
                counter++;
            }
        }
    }
    return counter == 0 ? texture(ssRefTex, fuv).rgb : outc / counter;
}

// THATS FROM PANDA 3d! Thanks tobspr
const float SRGB_ALPHA = 0.055;
float linear_to_srgb(float channel) {
    if(channel <= 0.0031308)
        return 12.92 * channel;
    else
        return (1.0 + SRGB_ALPHA) * pow(channel, 1.0/2.4) - SRGB_ALPHA;
}
vec3 rgb_to_srgb(vec3 rgb) {
    return vec3(
        linear_to_srgb(rgb.r),
        linear_to_srgb(rgb.g),
        linear_to_srgb(rgb.b)
    );
}

uniform mat4 CurrentViewMatrix;
uniform mat4 LastViewMatrix;
uniform mat4 ProjectionMatrix;

vec2 projectMotion(vec3 pos){
    vec4 tmp = (ProjectionMatrix * vec4(pos, 1.0));
    return (tmp.xy / tmp.w) * 0.5 + 0.5;
}

vec3 makeMotion(vec2 uv){
	vec4 normalsDistanceData = textureMSAA(normalsDistancetex, uv, 0);
	normalsDistanceData.a += (1.0 - step(0.001, normalsDistanceData.a)) * 10000.0;
	vec3 camSpacePos = reconstructCameraSpaceDistance(uv, normalsDistanceData.a);
	vec3 worldPos = FromCameraSpace(camSpacePos);
	
	vec3 pos1 = (CurrentViewMatrix * vec4(worldPos, 1.0)).xyz;
	vec3 pos2 = (LastViewMatrix * vec4(worldPos, 1.0)).xyz;
	float dist = distance(pos1, pos2);
	vec2 displace = (projectMotion(pos2) - projectMotion(pos1));
	vec2 direction = (projectMotion(pos2) - projectMotion(pos1));
	
	float st = (1.0 / resolution.x) * dist * 20.0;
	vec2 lookup = uv + direction * 0.05;
	
	vec3 color = vec3(0);
	for(int i=0;i<20;i++){
		color += texture(deferredTex, lookup).rgb;
		lookup += direction * 0.05;
	}
	
	return color / 20.0;
}

void main()
{
    vec3 color = makeMotion(UV);
	if(UseSSReflections){
		vec3 srdata = lookupSSRLowPass(UV, 1.0);
		color += srdata.rgb;
	}

    if(LensBlurAmount > 0.001 && DisablePostEffects == 0){
        float focus = CameraCurrentDepth;
        float adepth = textureMSAA(normalsDistancetex, vec2(0.5), 0).a;

        color = lensblur(avgdepth(UV), adepth, 0.99, 7.0);
    }

	if(DisablePostEffects == 0){
		if(UseBloom == 1) color += texture(bloomPassSource, UV).rgb * 0.2;
        color = ExecutePostProcessing(color, UV);
		//color = color / (1 + color);
		float gamma = 1.0/2.2;
		color = rgb_to_srgb(color);
	}
	
    outColor = clamp(vec4(color, toLogDepthEx(textureMSAAFull(normalsDistancetex, UV).a, 1000)), 0.0, 10000.0);
}