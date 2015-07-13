#version 430 core

layout (binding = 0, rgba16f) writeonly uniform image2D colorTexWR;
layout (binding = 1, rgba16f) readonly uniform image2D colorTex;
//layout (binding = 2, rgba8) readonly uniform image2D texColor;
//layout (binding = 3, r32f) readonly uniform image2D texDepth;
layout (binding = 2, rgba16f) readonly uniform image2D worldPosTex;
layout (binding = 3, rgba16f) readonly uniform image2D normalsTex;
layout (binding = 4, rgba16f) readonly uniform image2D colorTexLast;

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;

uniform mat4 ProjectionMatrix;
uniform mat4 ViewMatrix;
mat4 PV = (ProjectionMatrix * ViewMatrix);
uniform vec3 CameraPosition;
uniform float HBAOContribution;
uniform float Rand;

#include UsefulIncludes.glsl

ivec2 uvToIUv(vec2 uv){
    ivec2 size = imageSize(worldPosTex);
    //ivec2 size = ivec2(1366, 768);
    return ivec2(uv.x * size.x, uv.y * size.y);
}
vec2 iuvToUv(ivec2 uv){
    ivec2 size = imageSize(worldPosTex);
    //ivec2 size = ivec2(1366, 768);
    return vec2(float(uv.x) / float(size.x), float(uv.y) / float(size.y));
}
float textureMaxFromLine(float v1, float v2, vec2 p1, vec2 p2, image2D sampler){
    float ret = 0;
    for(float i=0;i<1;i+=0.03) 
        ret = max(mix(v1, v2, i) - length(imageLoad(worldPosTex, uvToIUv(mix(p1, p2, i))).rgb), ret);
        
    return ret;
}

vec2 saturatev2(vec2 v){
    return clamp(v, 0.0, 1.0);
}

bool testVisibility3d2(vec3 w1, vec3 w2) {
    vec4 clipspace = (PV) * vec4((w1), 1.0);
    vec2 sspace1 = saturatev2((clipspace.xyz / clipspace.w).xy * 0.5 + 0.5);
    vec4 clipspace2 = (PV) * vec4((w2), 1.0);
    vec2 sspace2 = saturatev2((clipspace2.xyz / clipspace2.w).xy * 0.5 + 0.5);
    float d3d1 = (length(ToCameraSpace(w1)));
    float d3d2 = (length(ToCameraSpace(w2)));
    float mx = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, worldPosTex));
    //float mx2 = (textureMaxFromLine(d3d1, d3d2, sspace1, sspace2, texDepth));

    return mx == 0;
}
vec2 projdir(vec3 start, vec3 end){
	//vec3 dirPosition = start + end;
	
	vec4 clipspace = (PV) * vec4((start), 1.0);
	vec2 sspace1 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
	clipspace = (PV) * vec4((end), 1.0);
	vec2 sspace2 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
	return (sspace2 - sspace1);
}



float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float Radiosity() 
{    
    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    ivec2 iUV2 = ivec2(
        gl_LocalInvocationID.x,
        gl_LocalInvocationID.y
    );
    vec3 posCenter = imageLoad(worldPosTex, iUV).rgb;
    vec3 normalCenter = normalize(imageLoad(normalsTex, iUV).rgb);
    vec3 ambient = vec3(0);
    const int samples = 20;
    const int octaves = 4;
    
    // choose between noisy and slower, but better looking variant
    float randomizer = 138.345341 * rand(iuvToUv(iUV)) + Rand;
    // or faster, non noisy variant, which is also cool looking
    //const float randomizer = 138.345341;
    
    vec3 ambientColor = vec3(1,1,1);
    float initialAmbient = 0.01;
    
    uint counter = 0;
    
    for(int i=0;i<samples;i++)
    {
        float rd = randomizer * float(i);
        float weight = 1;
        vec3 displace = (vec3(
            fract(rd) * 2 - 1, 
            fract(rd*12.2562), 
            fract(rd*7.121214) * 2 - 1
        )) * clamp(length(posCenter), 0.1, 2.0) * 0.1;
        float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)));
        if(dotdiffuse == 0) { counter+=octaves;continue; }
        for(int div = 0;div < octaves; div++)
        {
            if(testVisibility3d2(posCenter, posCenter + displace))
            {
                ambient += ambientColor * dotdiffuse * weight;
            } else { counter += octaves - div; break; }
            displace = displace * 2.94;
            weight = weight * 0.47;
            counter++;
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return (rs + initialAmbient).r;
}
vec3 FullGI() 
{    
    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    ivec2 iUV2 = ivec2(
        gl_LocalInvocationID.x,
        gl_LocalInvocationID.y
    );
    vec3 posCenter = imageLoad(worldPosTex, iUV).rgb;
    vec3 normalCenter = normalize(imageLoad(normalsTex, iUV).rgb);
    vec3 color = vec3(0);
    const int samples = 160;
    
    // choose between noisy and slower, but better looking variant
    float randomizer = 138.345341 * rand(iuvToUv(iUV)) + Rand;
    // or faster, non noisy variant, which is also cool looking
    //const float randomizer = 138.345341 + Rand;
    
    
    uint counter = 0;
    ivec2 size = imageSize(worldPosTex);
    
    vec3 cameraRelative = CameraPosition - FromCameraSpace(posCenter);

    for(int i=0;i<samples;i++)
    {
        float rd = randomizer * float(i) * 125.1234897;
        ivec2 displace = ivec2(
            fract(rd) * size.x, 
            fract(rd*12.2562) * size.y);
        vec3 posThere = imageLoad(worldPosTex, displace).rgb;
        vec3 normalThere = normalize(imageLoad(normalsTex, displace).rgb);
        float att = 1.0 / pow(((distance(posThere, posCenter)) + 1.0), 4.0) * 430;
        float diffuseComponent =  max(0, -dot(normalThere,  normalCenter));
       /* vec3 lightRelative = posThere - posCenter;
        vec3 R = reflect(lightRelative, normalCenter);
        float cosAlpha = -dot(normalize(cameraRelative), normalize(R));
        float specularComponent = clamp(pow(cosAlpha, 50.0), 0.0, 1.0);*/
        vec3 colorThere = imageLoad(colorTex, displace).rgb * 70;
        //if(testVisibilityEX(iUV, displace, posCenter, posThere)){                

            
            
            //color += colorThere * att * diffuseComponent + colorThere * specularComponent;
            color += colorThere * att * diffuseComponent;
       // }
    }
    return color / samples;
}

void main(){

    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    vec4 c = vec4(0,0,0, Radiosity());
    //barrier();
    vec4 last = imageLoad(colorTexLast, iUV);
    c = (c + last*1) / 2;
    imageStore(colorTexWR, iUV, c);
}