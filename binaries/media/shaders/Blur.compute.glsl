#version 430 core

layout (binding = 0, rgba16f) writeonly uniform image2D colorTexWR;
layout (binding = 1, rgba16f) readonly uniform image2D colorTex;
//layout (binding = 2, rgba8) readonly uniform image2D texColor;
//layout (binding = 3, r32f) readonly uniform image2D texDepth;
layout (binding = 2, rgba16f) readonly uniform image2D worldPosTex;
layout (binding = 3, rgba16f) readonly uniform image2D normalsTex;
layout (binding = 4, rgba16f) readonly uniform image2D colorTexLast;

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;
bool testVisibilityEX(ivec2 uv1, ivec2 uv2) {
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
//uniform float Rand;
/*
#define mPI2 (2*3.14159265)
bool testVisibilityEX(ivec2 uv1, ivec2 uv2) {
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
float randEX(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 RadiosityEX() 
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
}*/

//uniform mat4 PV;
uniform mat4 ProjectionMatrix;
uniform mat4 ViewMatrix;
mat4 PV = (ProjectionMatrix * ViewMatrix);
uniform vec3 CameraPosition;
uniform float HBAOContribution;
uniform float Rand;

vec3 FromCameraSpace(vec3 position){
    return position - -CameraPosition;
}
vec2 projdir(vec3 start, vec3 end){
	//vec3 dirPosition = start + end;
	
	vec4 clipspace = (PV) * vec4((start), 1.0);
	vec2 sspace1 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
	clipspace = (PV) * vec4((end), 1.0);
	vec2 sspace2 = (clipspace.xyz / clipspace.w).xy * 0.5 + 0.5;
	return (sspace2 - sspace1);
}

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


const float[9] binarysearch = float[9](0.5, 0.25, 0.75, 0.125, 0.875, 0.375, 0.625, 0.01, 0.98);

bool testVisibility3d(ivec2 cuv, vec3 w1, vec3 w2) {
    //vec3 direction = normalize(w2 - w1);
    
    float d3d1 = length(w1);
    float d3d2 = length(w2);
    ivec2 sdir = uvToIUv(projdir(FromCameraSpace(w1), FromCameraSpace(w2)));
    for(int i=0; i<9; i++) { 
        vec2 ruv = mix(cuv, cuv + sdir, binarysearch[i]);
        ivec2 iruv = ivec2(ruv); 
        vec3 wd = imageLoad(worldPosTex, iruv).rgb; 
        float rd3d = length(wd) + 0.001;
        if(rd3d < mix(d3d1, d3d2, i) && mix(d3d1, d3d2, i) - rd3d < 1.001) {
            return false;
        }
    }
    return true;
}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 Radiosity() 
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
    const int samples = 15;
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
        ));// * clamp(length(posCenter), 0.1, 2.0) * 0.4);
        float dotdiffuse = max(0, dot(normalize(displace),  (normalCenter)));
        if(dotdiffuse == 0) { counter+=octaves;continue; }
        for(int div = 0;div < octaves; div++)
        {
            if(testVisibility3d((iUV), posCenter, posCenter + displace))
            {
                ambient += ambientColor * dotdiffuse * weight;
            } else { counter += octaves - div; break; }
            displace = displace * 1.4;
            weight = weight * 0.8;
            counter++;
        }
    }
    vec3 rs = counter == 0 ? vec3(0) : (ambient / (counter));
    return rs + initialAmbient;
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
    const int samples = 15;
    
    // choose between noisy and slower, but better looking variant
    float randomizer = 138.345341 * rand(iuvToUv(iUV)) + Rand;
    // or faster, non noisy variant, which is also cool looking
    //const float randomizer = 138.345341;
    
    
    uint counter = 0;
    ivec2 size = imageSize(worldPosTex);
    
    vec3 cameraRelative = CameraPosition - FromCameraSpace(posCenter);

    for(int i=0;i<samples;i++)
    {
        float rd = randomizer * float(i);
        ivec2 displace = ivec2(
            fract(rd) * size.x, 
            fract(rd*12.2562) * size.y);
        if(testVisibilityEX(iUV, displace)){                
            vec3 posThere = imageLoad(worldPosTex, displace).rgb;
            vec3 normalThere = normalize(imageLoad(normalsTex, displace).rgb);
            float att = 1.0 / pow(((distance(posThere, posCenter)) + 1.0), 2.0) * 130;
            float diffuseComponent = max(0, dot(normalThere,  normalCenter));

            
            vec3 lightRelative = posThere - posCenter;
            vec3 R = reflect(lightRelative, normalCenter);
            float cosAlpha = -dot(normalize(cameraRelative), normalize(R));
            float specularComponent = clamp(pow(cosAlpha, 50.0), 0.0, 1.0);
            vec3 colorThere = imageLoad(colorTex, displace).rgb;
            
            color += colorThere * att * diffuseComponent + colorThere * specularComponent;
        }
    }
    return color / samples;
}

void main(){

    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    vec4 c = vec4(FullGI() * 6 * HBAOContribution, Radiosity() * HBAOContribution);
    //barrier();
    vec4 last = imageLoad(colorTexLast, iUV);
    c = (c + last*2) / 3;
    imageStore(colorTexWR, iUV, c);
}