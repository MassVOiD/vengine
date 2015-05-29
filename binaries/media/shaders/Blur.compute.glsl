#version 430 core


uniform int PASS;

layout (binding = 0, rgba16f) writeonly uniform image2D colorTexWR;
layout (binding = 1, rgba16f) readonly uniform image2D colorTex;
//layout (binding = 2, rgba8) readonly uniform image2D texColor;
//layout (binding = 3, r32f) readonly uniform image2D texDepth;
layout (binding = 4, rgba16f) readonly uniform image2D worldPosTex;
layout (binding = 5, rgba16f) readonly uniform image2D normalsTex;

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;

uniform float HBAOContribution;
uniform float HBAOStrength;
uniform float Rand;
uniform vec3 CameraPosition;

#define mPI2 (2*3.14159265)

float testVisibility(ivec2 uv1, ivec2 uv2) {
    vec3 w1 = imageLoad(worldPosTex, uv1).rgb; 
    vec3 w2 = imageLoad(worldPosTex, uv2).rgb;  
    float d3d1 = length(w1);
    float d3d2 = length(w2);
    //float rd = fract(2.2356451 * gl_GlobalInvocationID.x * gl_GlobalInvocationID.y * 1.4345783);
    //float iter = 0.05 + 0.1 * rd;
    float result = 1;
    for(float i=0;i<1.0;i+= 0.1) { 
        vec2 ruv = mix(uv1, uv2, i);
        ivec2 iruv = ivec2(int(ruv.x), int(ruv.y));
        vec3 wd = imageLoad(worldPosTex, iruv).rgb; 
        float rd3d = length(wd) + 0.01;
        //if(rd3d < mix(d3d1, d3d2, i)) {
        //    return false;
        //}
        result = min(result, max(0, sign(rd3d - mix(d3d1, d3d2, i))));
    }
    return result;
}
vec3 Radiosity() 
{
    vec2 txs = imageSize(colorTex);
    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    vec3 originalColor = imageLoad(colorTex, iUV).rgb;
    //return originalColor;
    vec3 centerPosition = imageLoad(worldPosTex, iUV).rgb;  
    //vec3 lookupPosition = CameraPosition;
    float A = length(centerPosition);
    float AInv = 1.0 / (length(centerPosition) + 1.0);
    vec3 outc = vec3(0);
    int counter = 0;
    float minval = 0;
    vec3 normalCenter = imageLoad(normalsTex, iUV).rgb;
	//float specSize = imageLoad(normalsTex, iUV).a;
	vec3 positionCenter = imageLoad(worldPosTex,iUV).rgb;  
	//float speccomp = imageLoad(worldPosTex, iUV).a;  
	float distanceToCamera = length(positionCenter);
	vec3 outBuffer = vec3(0);
	vec3 cameraSpace = -positionCenter;
    float frand = 2.2356451 * float(gl_GlobalInvocationID.x) * float(gl_GlobalInvocationID.y) + centerPosition.x + centerPosition.y + centerPosition.z;
    for(float g = 0.05; g < mPI2; g += 2.5) 
    {
        for(float g2 = 0.02; g2 < 1; g2 += 0.24) 
        {           
            float rd = (g * g2 * frand);
            vec2 coord = (vec2(sin(g + g2 + frand), cos(g + g2 + frand)) * ((g2) * 10.1 * AInv)) * fract(rd);
            coord *= txs;
            ivec2 cUV = iUV + ivec2(int(coord.x), int(coord.y));
            //if(cUV.x < 0 || cUV.x > txs.x || cUV.y < 0 || cUV.y > txs.y) continue;
            
            float vis = testVisibility(cUV, iUV);
            if(vis == 1){
                vec3 worldPosition = imageLoad(worldPosTex, cUV).rgb;
                vec3 normalThere = imageLoad(normalsTex, cUV).rgb;
                
                float worldDistance = distance(positionCenter, worldPosition);
                if(worldDistance < 0.12) continue;

                float attentuation = 1.0 / (pow(((worldDistance)), 2.0) + 1.0) * 40.0;
                
                vec3 lightRelativeToVPos = worldPosition - positionCenter;
                float diffuse = 1.0 - clamp(dot(normalCenter, normalThere), 0.0, 1.0);
                
                vec3 R = reflect(lightRelativeToVPos, normalCenter.xyz);
                float cosAlpha = max(0, -dot(normalize(cameraSpace), normalize(R)));
                float specularComponent = pow(cosAlpha, 97.0);
                
                vec3 col = imageLoad(colorTex, cUV).rgb * 7;
                col = col * diffuse * 15 * attentuation +  col * specularComponent * 74;
                
                outc += col * vis;
            }
            counter+=2;
        }   
    }
    vec3 calc = (outc / counter);
    return clamp(calc, 0.0, 1.0);
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