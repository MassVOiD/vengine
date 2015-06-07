#version 430 core

layout (binding = 0, rgba16f) writeonly uniform image2D colorTexWR;
layout (binding = 1, rgba16f) readonly uniform image2D colorTex;
//layout (binding = 2, rgba8) readonly uniform image2D texColor;
//layout (binding = 3, r32f) readonly uniform image2D texDepth;
layout (binding = 2, rgba16f) readonly uniform image2D worldPosTex;
layout (binding = 3, rgba16f) readonly uniform image2D normalsTex;

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;

uniform float Rand;

#define mPI2 (2*3.14159265)
bool testVisibility(ivec2 uv1, ivec2 uv2) {
    vec3 w1 = imageLoad(worldPosTex, uv1).rgb; 
    vec3 w2 = imageLoad(worldPosTex, uv2).rgb;  
    float d3d1 = length(w1);
    float d3d2 = length(w2);
    
    for(float i=0;i<1.0;i+= 0.1) { 
        vec2 ruv = mix(uv1, uv2, i);
        ivec2 iruv = ivec2(int(ruv.x), int(ruv.y));
        vec3 wd = imageLoad(worldPosTex, iruv).rgb; 
        float rd3d = length(wd) + 0.01;
        if(rd3d < mix(d3d1, d3d2, i)) {
            return false;
        }
        //result = min(result, max(0, sign(rd3d - mix(d3d1, d3d2, i))));
    }
    return true;
}
float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 Radiosity() 
{
    vec2 txs = imageSize(colorTex);
    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    vec3 giLight = vec3(0);
    vec3 normalCenter = imageLoad(normalsTex, iUV).rgb;
    vec3 posCenter = imageLoad(worldPosTex, iUV).rgb;
    
    float randomizer = 138.345341 * rand(iUV) + Rand;
    //const float randomizer = 138.345341;
    
    const int samples = 64;
    vec3 colorCenter = imageLoad(colorTex, iUV).rgb;
    for(int i=0;i<samples;i++)
    {
        float rd = randomizer * float(i);
        ivec2 coord = ivec2(
            fract(rd) * txs.x, 
            fract(rd*12.2562) * txs.y
        );
        vec3 normalC = imageLoad(normalsTex, coord).rgb;
        vec3 color = imageLoad(colorTex, coord).rgb;
        vec3 posC = imageLoad(worldPosTex, coord).rgb;
        float diffuse =  max(0, dot(normalC, normalCenter));
        if(diffuse == 0) continue;
        float att = 1.0 / pow((distance(posC, posCenter) + 1.0), 2.0);
        if(att < 0.02) continue;
        if(testVisibility(iUV, coord))
        {
            giLight += color * diffuse * att;
            imageStore(colorTexWR, coord, vec4(colorCenter * diffuse * att, 1));
        }
    }
    return giLight / samples * 200;
}

void main(){

    vec4 c = vec4(Radiosity(), 1);
    //barrier();
    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    imageStore(colorTexWR, iUV, c);
}