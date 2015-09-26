#version 430 core

layout (binding = 0, rgba16f) volatile uniform image2D colorTexWR;
layout (binding = 1, rgba16f) volatile uniform image2D colorTexWRHelper;
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

vec3 doBlur(ivec2 uv){

    ivec2 isize = imageSize(colorTexWRHelper);
    int clamper = ((Direction == DIRECTION_X) ? isize.x : isize.y);

    vec3 outc = vec3(0);
    float weight = 0;
    float wposCenter = texture(worldPosTex, iUVtoUV(uv)).r;
    vec3 normCenter = texture(normalsTex, iUVtoUV(uv)).rgb;
    for(int i = - 9; i < 9; i ++ ){
        ivec2 nuv = uv + (Direction == DIRECTION_X ? ivec2(i, 0) : ivec2(0, i));
        vec3 s = Direction == DIRECTION_X ? 
          (texture(Input, iUVtoUV(nuv)).rgb) : 
          (imageLoad(colorTexWRHelper, nuv).rgb);

        float depth = Direction == DIRECTION_X ? 
          (texture(Input, iUVtoUV(nuv)).a) : 
          (imageLoad(colorTexWRHelper, nuv).a);
          
        float distanceBool = max(0, sign(-(abs(wposCenter - depth) - 0.003)));
        vec3 norm = texture(normalsTex, iUVtoUV(nuv)).rgb;
        float normalBool = max(0, pow(dot(normCenter, norm), 8));
        //s *= (length(s) - (length(vec3(1)) - 0.5));
        outc += s * normalBool * distanceBool;
        weight +=   normalBool * distanceBool;
    }    
    return weight == 0 ? texture(Input, iUVtoUV(uv)).rgb : outc / weight;
}


void main(){

    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    iSize = vec2(imageSize(colorTexWRHelper));
    vec3 blur = doBlur(iUV);
    if(Direction == DIRECTION_X){
        vec4 c = vec4(blur, texture(Input, iUVtoUV(iUV)).a);
        barrier();
        imageStore(colorTexWRHelper, iUV, c);
    } else {
        vec4 c = vec4(blur, imageLoad(colorTexWRHelper, (iUV)).a);
        //c.rgb += imageLoad(colorTexWR, (iUV)).rgb;
        barrier();
        imageStore(colorTexWRHelper, iUV, c);
    }

}