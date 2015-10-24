#version 430 core

layout (binding = 0, rg8) volatile uniform image2D colorTexWR;
layout (binding = 1, rg8) volatile uniform image2D colorTexWRHelper;
layout(binding = 2) uniform sampler2D Input;
layout(binding = 3) uniform sampler2D worldPosTex;
layout(binding = 4) uniform sampler2D normalsTex;

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;

#define DIRECTION_X 0
#define DIRECTION_Y 1
uniform int Direction;
uniform int Length;

vec2 iSize;

vec2 iUVtoUV(ivec2 u){
    return vec2(u) / iSize;
}

uniform float FarPlane;
#define MATH_E 2.7182818284
float reverseLogEx(float dd, float far){
	return pow(2, dd * log2(far+1.0)) - 1;
}
float reverseLog(float dd){
	return reverseLogEx(dd, FarPlane);
}

float doBlurTemporalUpscale(ivec2 uv){

    ivec2 isize = imageSize(colorTexWRHelper);
    int clamper = ((Direction == DIRECTION_X) ? isize.x : isize.y);

    float outc = 0;
    float weight = 0;
    float wposCenter = texture(worldPosTex, iUVtoUV(uv)).r;
    vec3 normCenter = texture(normalsTex, iUVtoUV(uv)).rgb;
    for(int i = - 5; i < 5; i ++ ){
        ivec2 nuv = uv + (Direction == DIRECTION_X ? ivec2(i, 0) : ivec2(0, i));
        float s = Direction == DIRECTION_X ? 
          (texture(Input, iUVtoUV(nuv)).r) : 
          (imageLoad(colorTexWRHelper, nuv).r);

        float depth = Direction == DIRECTION_X ? 
          (texture(Input, iUVtoUV(nuv)).g) : 
          (imageLoad(colorTexWRHelper, nuv).g);
          
        float distanceBool = max(0, sign(-(abs(wposCenter - depth) - 0.003)));
        vec3 norm = texture(normalsTex, iUVtoUV(nuv)).rgb;
        float normalBool = max(0, pow(dot(normCenter, norm), 8));
        //s *= (length(s) - (length(vec3(1)) - 0.5));
        outc += s * normalBool * distanceBool * normalBool;
        weight +=   normalBool * distanceBool * normalBool;
    }    
    return weight == 0 ? texture(Input, iUVtoUV(uv)).r : outc / weight;
}
/*
vec3 doBlurSmartAA(ivec2 uv){

    ivec2 isize = imageSize(colorTexWRHelper);
    int clamper = ((Direction == DIRECTION_X) ? isize.x : isize.y);

    vec3 scenter = Direction == DIRECTION_X ? 
      (texelFetch(Input, nuv).rgb) : 
      (imageLoad(colorTexWRHelper, nuv).rgb);
    vec3 outc = vec3(0);
    float weight = 0;
    float entrophy = 0.0;
    for(int i = - 3; i < 3; i ++ ){
        ivec2 nuv = uv + (Direction == DIRECTION_X ? ivec2(i, 0) : ivec2(0, i));
        vec3 s = Direction == DIRECTION_X ? 
          (texelFetch(Input, nuv).rgb) : 
          (imageLoad(colorTexWRHelper, nuv).rgb);
        
        entrophy = 
          
        float distanceBool = max(0, sign(-(abs(wposCenter - depth) - 0.003)));
        vec3 norm = texture(normalsTex, iUVtoUV(nuv)).rgb;
        float normalBool = max(0, pow(dot(normCenter, norm), 8));
        //s *= (length(s) - (length(vec3(1)) - 0.5));
        outc += s * normalBool * distanceBool;
        weight +=   normalBool * distanceBool;
    }    
    return weight == 0 ? texture(Input, iUVtoUV(uv)).rgb : outc / weight;
}
*/

void main(){

    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    iSize = vec2(imageSize(colorTexWRHelper));
    float blur = doBlurTemporalUpscale(iUV);
   // vec3 blur = Direction == DIRECTION_X ? 
  //        (texture(Input, iUVtoUV(iUV)).rgb) : 
 //         (imageLoad(colorTexWRHelper, iUV).rgb);
    if(Direction == DIRECTION_X){
        vec2 c = vec2(blur, texture(Input, iUVtoUV(iUV)).g);
        barrier();
        imageStore(colorTexWRHelper, iUV, c.rggg);
    } else {
        vec2 c = vec2(blur, imageLoad(colorTexWRHelper, (iUV)).g);
        //c.rgb += imageLoad(colorTexWR, (iUV)).rgb;
        barrier();
        imageStore(colorTexWRHelper, iUV, c.rggg);
    }

}