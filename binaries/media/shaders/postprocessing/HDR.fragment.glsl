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
    if(amount < 0.02) return fxaa(currentTex, UV).rgb;
    amount -= 0.02;
    float radius = max_radius;  
    float centerDepthDistance = abs((centerDepth) - (depthfocus));
    //float centerDepth = texture(texDepth, UV).r;
    float focus = length(texture(worldPosTex, vec2(0.5)).rgb);
    float cc = length(texture(worldPosTex, UV).rgb);
    for(float x = 0; x < mPI2; x+=0.5){ 
        for(float y=0.1;y<1.0;y+= 0.1){  
            
            //ngon
            
            vec2 crd = vec2(sin(x + y) * ratio, cos(x + y)) * (rand(UV + vec2(x, y)));
            //float alpha = texture(alphaMaskTex, crd*1.41421).r;
            //if(length(crd) > 1.0) continue;
            vec2 coord = UV+crd * 0.02 * amount;  
            //coord.x = clamp(abs(coord.x), 0.0, 1.0);
            //coord.y = clamp(abs(coord.y), 0.0, 1.0);
            float depth = length(texture(worldPosTex, coord).xyz);
            vec3 texel = texture(currentTex, coord).rgb;
            float w = length(texel) + 0.1;
            float dd = length(crd * 0.1 * amount)/0.125;
            w *= dd;
            
            weight+=w;
            finalColor += texel * w;
            
        }
    }
    return weight == 0.0 ? vec3(0.0) : finalColor/weight;
}

uniform int UseBloom;
vec3 lookupBloomBlurred(vec2 buv, float radius){
    vec3 outc = vec3(0);
    float counter = 0;
    for(float g = 0; g < mPI2*2; g+=0.5)
    { 
        for(float g2 = 0; g2 < 3.14; g2+=0.5)
        { 
            float h = rand(UV+vec2(g, g2));
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (h * radius*0.1);
            vec4 color = texture(bloomTex, buv + gauss).rgba * (pow(1.0-h, 1.2));
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
                outc += color;
            }
            //counter++;
        }
    }
    return counter == 0 ? vec3(0) : (outc / counter) * (snoise(vec3(buv.x*400, buv.y*400, Time*55.4))*0.5+0.5);
}
uniform float InputFocalLength;
float avgdepth(vec2 buv){
    float outc = float(0);
    float counter = 0;
    float fDepth = length(texture(worldPosTex, vec2(0.5, 0.5)).rgb);
    for(float g = 0; g < mPI2; g+=0.4)
    { 
        for(float g2 = 0; g2 < 1.0; g2+=0.11)
        { 
            vec2 gauss = vec2(sin(g + g2)*ratio, cos(g + g2)) * (g2 * 0.05);
            vec3 color = texture(worldPosTex, buv + gauss).xyz;
            float adepth = length(color);
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
    return min(abs(outc / counter), 4.0);
}

vec3 edgeDetect(vec2 uv){
    float minentrophy = 99, maxentrophy = 0, minentrophyangle = 0;
    vec3 center = texture(currentTex, uv).rgb;
    float texel = length(vec2(1.0) / resolution);
    for(float g = 0; g < mPI2; g+=0.05)
    { 
            vec2 gauss = vec2(sin(g)*ratio, cos(g)) * texel*2;
            vec2 newuv = uv + gauss;
            vec3 col = texture(currentTex, newuv).rgb;
            float entrophy = max(0, distance(center, col));
            minentrophy = min(minentrophy, entrophy);
            if(entrophy > maxentrophy){
                minentrophyangle = g;
            }
            maxentrophy = max(maxentrophy, entrophy);
    }
    #define AA_RANGE (3.0)
    vec3 buff = vec3(0);
    float counter = 0.0;
    
    for(float a = 0; a < mPI2; a+=0.2)
    { 
        for(float g = -AA_RANGE; g < AA_RANGE; g+=1)
        { 
            vec2 gauss = vec2(sin(a)*ratio, cos(a)) * (texel) * g;
            vec2 newuv = uv + gauss;
            vec3 col = texture(currentTex, newuv).rgb;
            float entrophy = max(0, distance(center, col));
            
                buff += col * entrophy + center * (1.0 - entrophy);
                counter += entrophy;
            
        }
    }

    return counter == 0 ? vec3(0) : (buff / counter);
}

vec3 getAverageOfAdjacent(vec2 uv){
    ivec2 center = ivec2(uv * resolution);
    vec3 buf = vec3(0);
    ivec2 unitx = ivec2(1, 0);
    ivec2 unity = ivec2(0, 1);
    buf += texelFetch(currentTex, center + unitx, 0).rgb;
    buf += texelFetch(currentTex, center - unitx, 0).rgb;
    buf += texelFetch(currentTex, center + unity, 0).rgb;
    buf += texelFetch(currentTex, center - unity, 0).rgb;
    
    buf += texelFetch(currentTex, center + unitx + unity, 0).rgb;
    buf += texelFetch(currentTex, center + unitx - unity, 0).rgb;
    buf += texelFetch(currentTex, center - unitx + unity, 0).rgb;
    buf += texelFetch(currentTex, center - unitx - unity, 0).rgb;
    
    return buf / 8;
}

uniform int ShowSelected;
uniform int UnbiasedIntegrateRenderMode;
void main()
{
    vec4 color1 = fxaa(currentTex, UV);
    //vec3 avg = getAverageOfAdjacent(UV);
   // if(distance(getAverageOfAdjacent(UV), texture(currentTex, UV).rgb) > 0.6) color1.rgb = avg;
    //vec4 color1 = vec4(edgeDetect(UV), 1.0);
    if(ShowSelected == 1) color1.rgb += lookupSelected(UV, 0.02);
    //vec4 color1 = vec4(0,0,0,1);
    float depth = texture(depthTex, UV).r;
    centerDepth = depth;
    if(LensBlurAmount > 0.001){
        float focus = CameraCurrentDepth;
        float adepth = length(texture(worldPosTex, vec2(0.5)).xyz);
        //float fDepth = reverseLog(CameraCurrentDepth);

        color1.xyz = lensblur(avgdepth(UV), adepth, 0.99, 7.0);
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
    
    if(UseBloom == 1) color1.xyz += lookupBloomBlurred(UV, 0.1).rgb;  
    color1.a = texture(depthTex, UV).r;
    
    vec3 last = texture(lastIndirectTex, UV).rgb;
    float f1 = length(last) / length(vec3(1));
    float f2 = length(color1.rgb);
    
    vec3 additiveMix = mix(last, color1.rgb, UnbiasedIntegrateRenderMode == 1 ? 0.08 : 1.0);
    
    
    outColor = clamp(vec4(additiveMix, 1.0), 0.0, 1.0);
}