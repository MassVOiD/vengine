#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

out vec4 outColor;

layout (std430, binding = 6) buffer RandomsBuffer
{
    float Randoms[]; 
}; 

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand2s(seeder)) ;
}

vec2 randsPointer = vec2(0);
uniform float RandomsCount;
float getRand2(){
    //if(randsPointer >= 22.0) randsPointer = 0;
    float r = rand2s(randsPointer);
    randsPointer+=0.11;
    return r;
}
vec3 random3dSample(){
    return normalize(vec3(
        getRand2() * 2 - 1, 
        getRand2() * 2 - 1, 
        getRand2() * 2 - 1
    ));
}


// using this brdf makes cosine diffuse automatically correct
vec3 BRDF(vec3 reflectdir, vec3 norm, float roughness){
    vec3 displace = random3dSample();
    displace = displace * sign(dot(norm, displace));
    return mix(displace, reflectdir, roughness);
}


mat3 TBN;
// using this brdf makes cosine diffuse automatically correct
vec3 BRDFBiased(vec3 reflectdir, vec3 norm, float roughness, vec2 huv){
    vec3 displace = TBN * hemisphereSample_cos(huv.x, huv.y);
    displace = displace * sign(dot(norm, displace));
    return mix(displace, reflectdir, roughness);
}
mat4 VP = (ProjectionMatrix * ViewMatrix);
vec2 projectOnScreen(vec3 worldcoord){
    vec4 clipspace = (VP) * vec4(worldcoord, 1.0);
    vec2 sspace1 = ((clipspace.xyz / clipspace.w).xy + 1.0) / 2.0;
    //if(clipspace.z < 0.0) return vec2(-1);
    return sspace1;
}


vec3 vec3pow(vec3 inputx, float po){
    return vec3(
    pow(inputx.x, po),
    pow(inputx.y, po),
    pow(inputx.z, po)
    );
}

vec3 lookupCubeMap(vec3 displace){
    vec3 c = texture(cubeMapTex, displace).rgb;
    return vec3pow(c, 0.2);
}

vec3 convertRGBtoHSV(vec3 c) {
    float colorMax = max(max(c.r,c.g), c.b);
    float colorMin = min(min(c.r,c.g), c.b);
    float delta = colorMax - colorMin;
    float h = 0.0, s = 0.0, v = colorMax;
    if (colorMax != 0.0) s = (colorMax - colorMin ) / colorMax;
    if (delta != 0.0) {
        if (c.r == colorMax) h = (c.g - c.b) / delta;
        else if (c.g == colorMax) h = 2.0 + (c.b - c.r) / delta;
        else h = 4.0 + (c.r - c.g) / delta;
        h *= 60.0;
        if (h < 0.0) h += 360.0;
    }
    return vec3(h,s,v);
}

mat4 iMVPI;
vec3 reconstructWorldPos(vec2 uv){
	return FromCameraSpace(texture(worldPosTex, uv).rgb);
   // vec4 reconstructDir = iMVPI * vec4((uv * 2 - 1), 1.0, 1.0);
   // reconstructDir.xyz /= reconstructDir.w;
  //  
   // return normalize(reconstructDir.xyz - CameraPosition) * revlog + CameraPosition;
}

uniform int UseHBAO;
float hbao(vec2 uv){
    if(texture(diffuseColorTex, uv).r >= 999){ 
        return 1.0;
    }
	iMVPI = inverse(ProjectionMatrix * ViewMatrix);
    // gather data
    uint idvals = texture(meshIdTex, uv).g;
    uint tangentEncoded = texture(meshIdTex, uv).a;
    vec3 tangent = unpackSnorm4x8(tangentEncoded).xyz;
    /*
    uint packpart1 = packUnorm4x8(vec4(AORange, AOStrength, AOAngleCutoff, SubsurfaceScatteringMultiplier));
    uint packpart2 = packUnorm4x8(vec4(VDAOMultiplier, VDAOSamplingMultiplier, VDAORefreactionMultiplier, 0));
    */
    vec4 vals = unpackUnorm4x8(idvals);
    float aorange = vals.x * 2 + 0.1;
    float aostrength = vals.y * 4 + 0.1;
    float aocutoff = 1.0 - vals.z;
    
    

	vec4 normin = texture(normalsTex, uv);
    vec3 norm = normin.rgb;
    vec3 posc = texture(worldPosTex, uv).rgb;
    vec3 posccsp = FromCameraSpace(posc);
	
	vec3 ps1 = texture(worldPosTex, uv).rgb;
	//return distance(posccsp, ps1)*0.1;
    
    /*
    TBN = inverse(transpose(mat3(
        norm,
        cross(tangent, norm),
        tangent
    )));*/
	
	TBN = inverse(transpose(mat3(
        tangent,
        cross(norm, tangent),
        norm
    )));
    
    float buf = 0.0, div = 1.0/(length(posc)+1.0);
    float counter = 0.0;
    vec3 dir = normalize(reflect(posc, norm));
    float meshRoughness = texture(meshDataTex, uv).a;
    float samples = mix(2, 3, meshRoughness);
    float stepsize = PI*2 / samples;
    float ringsize = length(posc)*0.3;
    //for(float g = 0; g < samples; g+=1)
    for(float g = 0.0; g <= PI*2; g+=stepsize)
    {
        float minang = 0;

        //vec3 displace = normalize(BRDF(dir, norm, meshRoughness)) * ringsize;
		float grd = getRand2() * stepsize;
        vec2 zx = vec2(sin(g + grd), cos(g + grd));
        vec3 displace = mix((TBN * normalize(vec3(zx, sqrt(1.0 - length(zx))))), dir, 1.0 - meshRoughness) * ringsize;
        //vec3 displace = normalize(BRDFBiased(dir, norm, meshRoughness, (vec2(getRand2(), getRand2())))) * ringsize;
        
        vec2 sspos2 = projectOnScreen(posccsp + displace);
		//sspos2 = uv + normalize(sspos2 - uv)*0.8;
		float dt = 0;
		vec3 pos = vec3(0);
        for(float g3 = 0.0; g3 < 1.0; g3+=0.04)
        {
            float z = getRand2();
            //vec2 gauss = mix(uv, sspos2, g3*g3 + 0.001);
            //
			vec2 gauss = mix(uv, sspos2, g3*g3 + 0.01);
			//if(gauss.x < 0 || gauss.x > 1.0 || gauss.y < 0 || gauss.y > 1) break;
            pos = texture(worldPosTex, gauss).rgb;
            dt = max(0, dot(norm, normalize(pos - posc))-0.0);
			minang = max(dt * (ringsize - length(pos - posc))/ringsize, minang);
        }
        //if(minang > aocutoff) minang = 1;
		//	
        buf += minang;//log2(minang*10+1)/log2(10);
        counter+=1.0;
    }
    return 1.0 - (buf/counter);
}

uniform float AOGlobalModifier;
void main()
{   
    vec3 color1 = vec3(0);
    
    Seed(UV+1 + Time);
    randsPointer = vec2(randomizer * 113.86786 );
    float au = 0;
    if(UseHBAO == 1){
       // if(UV.x < 0.5) au = vec4(hbao(), 0, 0, 1);
       //else au = vec4(Radiosity(), 0, 0, 1);
       au = pow(hbao(UV), AOGlobalModifier);
    }
    outColor = vec4(texture(normalsTex, UV).rgb * 0.5 + 0.5, au);
    
    
}