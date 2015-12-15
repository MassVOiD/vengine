#version 430 core

layout(triangles, fractional_odd_spacing, ccw) in;

#include Mesh3dUniforms.glsl

in Data {
    flat int instanceId;
    vec3 WorldPos;
    vec2 TexCoord;
    vec3 Normal;
    vec4 Tangent;
    vec2 Data;
} Input[];
out Data {
    flat int instanceId;
    vec3 WorldPos;
    vec2 TexCoord;
    vec3 Normal;
    vec4 Tangent;
    vec2 Data;
} Output;

uniform int MaterialType;

#define MaterialTypeSolid 0
#define MaterialTypeRandomlyDisplaced 1
#define MaterialTypeWater 2
#define MaterialTypeSky 3
#define MaterialTypeWetDrops 4
#define MaterialTypeGrass 5
#define MaterialTypePlanetSurface 6
#define MaterialTypeTessellatedTerrain 7

uniform int UseBumpMap;
layout(binding = 29) uniform sampler2D bumpMap;


#include noise3D.glsl

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y )* v1 + vec3(gl_TessCoord.z) * v2;
}
vec4 interpolate4D(vec4 v0, vec4 v1, vec4 v2)
{
   	return vec4(gl_TessCoord.x) * v0 + vec4(gl_TessCoord.y )* v1 + vec4(gl_TessCoord.z) * v2;
}

float sns(vec2 p, float scale, float tscale){
    return snoise(vec3(p.x*scale, p.y*scale, Time * tscale * 0.5));
}
float getwater( vec2 position ) {

    float color = 0.0;
    color += sns(position + vec2(Time/3, Time/13), 0.03, 1.2) * 40;
    color += sns(position, 0.1, 1.2) * 10;
    color += sns(position, 0.25, 2.)*6;
    color += sns(position, 0.38, 3.)*3;
    //color += sns(position, 4., 2.)*0.9;
   // color += sns(position, 7., 6.)*0.3;
    //color += sns(position, 15., 2.)*0.7;
    //color += sns(position, 2., 2.) * 1.2;
    return color / 7.0;

}

float getPlanetSurface(){
    vec3 wpos = Output.WorldPos * 0.00111;
    float factor = snoise(wpos) * 12;
    factor += snoise(wpos * 0.1) * 20;
    factor += snoise(wpos * 0.06) * 50;
    factor += snoise(wpos * 0.01) * 80;
    return factor * 1;
}

void main()
{
   	// Interpolate the attributes of the output vertex using the barycentric coordinates
   	vec2 UV = interpolate2D(Input[0].TexCoord, Input[1].TexCoord, Input[2].TexCoord);
    Output.TexCoord = UV;
   	//barycentric = interpolate3D(Input[0].Barycentric, Input[1].Barycentric, Input[2].Barycentric);
   	vec3 normal = interpolate3D(Input[0].Normal, Input[1].Normal, Input[2].Normal);
   	Output.Tangent = interpolate4D(Input[0].Tangent, Input[1].Tangent, Input[2].Tangent);
   	Output.WorldPos = interpolate3D(Input[0].WorldPos, Input[1].WorldPos, Input[2].WorldPos);
	   	// Displace the vertex along the normal
	Output.instanceId = Input[0].instanceId;
    
    if(MaterialType == MaterialTypeWater){
        float factor = abs(getwater(UV * 15));
        vec3 lpos = Output.WorldPos;
        Output.WorldPos += normal * (factor) * 0.11;           
        vec3 nee = normalize((normal - ((Output.Tangent.xyz) *distance(lpos, Output.WorldPos))));
        nee = dot(nee, normal) < 0 ?  nee = -nee : nee;
        normal = normalize(nee);
    }
    if(MaterialType == MaterialTypePlanetSurface){
        float factor = getPlanetSurface();
        Output.WorldPos -= normal * (factor) * 11.0;
    }
    if(MaterialType == MaterialTypeTessellatedTerrain){
        float factor = texture(bumpMap, UV).r;
        //float factorx = texture(bumpMap, UV+vec2(0.0001, 0)).r * 90;
      //  float factory = texture(bumpMap, UV+vec2(0, 0.0001)).r * 90;
       // vec3 p1 = Output.WorldPos - Output.Tangent * 4 + normal * factorx;
       // vec3 p2 = Output.WorldPos + cross(Output.Tangent, normal) * 4 + normal * factory;
        Output.WorldPos += normal * factor;
       // vec3 n = normalize(cross(Output.WorldPos - p1, Output.WorldPos - p2));
      //  normal = n;
    }
    
        if(MaterialType == MaterialTypeGrass){

            Output.WorldPos += vec3(0, 0, 13) * (UV.y);
        } else if(UseBumpMap == 1){
            float factor = (texture(bumpMap, UV).r - 0.5) * 0.07;
          //  float factor = snoise(Output.WorldPos*normal / 4) * 0.2;
          //  factor += snoise(Output.WorldPos *10) * 0.03;
         //   Output.WorldPos += normal * (factor) * 1.3;
         //   normal = normalize(normal - (Output.Tangent * factor * 0.05));
        }
    
    
	
	Output.Normal = normalize(normal);
   	gl_Position = VPMatrix * vec4(Output.WorldPos, 1.0);
}
/**/