#version 430 core

layout(triangles, fractional_odd_spacing, ccw) in;

#include Mesh3dUniforms.glsl

in vec3 ModelPos_ES_in[];
in vec3 WorldPos_ES_in[];
in vec2 TexCoord_ES_in[];
in vec3 Normal_ES_in[];
//in vec3 Barycentric_ES_in[];
in int instanceId_ES_in[];
in vec3 Tangent_ES_in[];

uniform int MaterialType;

#define MaterialTypeSolid 0
#define MaterialTypeRandomlyDisplaced 1
#define MaterialTypeWater 2
#define MaterialTypeSky 3
#define MaterialTypeWetDrops 4
#define MaterialTypeGrass 5
#define MaterialTypePlanetSurface 6

uniform int UseBumpMap;
layout(binding = 31) uniform sampler2D bumpMap;
smooth out vec3 normal;
smooth out vec3 tangent;
smooth out vec3 positionWorldSpace;
smooth out vec3 positionModelSpace;
smooth out vec2 UV;
out int instanceId;

#include noise3D.glsl

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y )* v1 + vec3(gl_TessCoord.z) * v2;
}


float snoise2d(vec3 v)
{ 
    const vec2  C = vec2(1.0/6.0, 1.0/3.0) ;
    const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

    // First corner
    vec3 i  = floor(v + dot(v, C.yyy) );
    vec3 x0 =   v - i + dot(i, C.xxx) ;

    // Other corners
    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = 1.0 - g;
    vec3 i1 = min( g.xyz, l.zxy );
    vec3 i2 = max( g.xyz, l.zxy );

    //   x0 = x0 - 0.0 + 0.0 * C.xxx;
    //   x1 = x0 - i1  + 1.0 * C.xxx;
    //   x2 = x0 - i2  + 2.0 * C.xxx;
    //   x3 = x0 - 1.0 + 3.0 * C.xxx;
    vec3 x1 = x0 - i1 + C.xxx;
    vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
    vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

    // Permutations
    i = mod289(i); 
    vec4 p = permute( permute( permute( 
    i.z + vec4(0.0, i1.z, i2.z, 1.0 ))
    + i.y + vec4(0.0, i1.y, i2.y, 1.0 )) 
    + i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float n_ = 0.142857142857; // 1.0/7.0
    vec3  ns = n_ * D.wyz - D.xzx;

    vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

    vec4 x_ = floor(j * ns.z);
    vec4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

    vec4 x = x_ *ns.x + ns.yyyy;
    vec4 y = y_ *ns.x + ns.yyyy;
    vec4 h = 1.0 - abs(x) - abs(y);

    vec4 b0 = vec4( x.xy, y.xy );
    vec4 b1 = vec4( x.zw, y.zw );

    //vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
    //vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
    vec4 s0 = floor(b0)*2.0 + 1.0;
    vec4 s1 = floor(b1)*2.0 + 1.0;
    vec4 sh = -step(h, vec4(0.0));

    vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
    vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

    vec3 p0 = vec3(a0.xy,h.x);
    vec3 p1 = vec3(a0.zw,h.y);
    vec3 p2 = vec3(a1.xy,h.z);
    vec3 p3 = vec3(a1.zw,h.w);

    //Normalise gradients
    vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    // Mix final noise value
    vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
    m = m * m;
    return 42.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), 
    dot(p2,x2), dot(p3,x3) ) );
}

float sns(vec2 p, float scale, float tscale){
    return snoise2d(vec3(p.x*scale, p.y*scale, Time * tscale * 0.5));
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
    vec3 wpos = positionWorldSpace * 0.00111;
    float factor = snoise(wpos) * 12;
    factor += snoise(wpos * 0.1) * 20;
    factor += snoise(wpos * 0.06) * 50;
    factor += snoise(wpos * 0.01) * 80;
    return factor * 1;
}

void main()
{
   	// Interpolate the attributes of the output vertex using the barycentric coordinates
   	UV = interpolate2D(TexCoord_ES_in[0], TexCoord_ES_in[1], TexCoord_ES_in[2]);
   	//barycentric = interpolate3D(Barycentric_ES_in[0], Barycentric_ES_in[1], Barycentric_ES_in[2]);
   	normal = interpolate3D(Normal_ES_in[0], Normal_ES_in[1], Normal_ES_in[2]);
   	tangent = interpolate3D(Tangent_ES_in[0], Tangent_ES_in[1], Tangent_ES_in[2]);
   	positionWorldSpace = interpolate3D(WorldPos_ES_in[0], WorldPos_ES_in[1], WorldPos_ES_in[2]);
   	positionModelSpace = interpolate3D(ModelPos_ES_in[0], ModelPos_ES_in[1], ModelPos_ES_in[2]);
	   	// Displace the vertex along the normal
	instanceId = instanceId_ES_in[0];
	normal = normalize(normal);
    
    if(MaterialType == MaterialTypeWater){
        float factor = getwater(UV * 5);
        vec3 lpos = positionWorldSpace;
        positionWorldSpace += normal * (factor) * 11.1;
        normal = normalize(normal - (tangent * factor * 0.05));
    }
    if(MaterialType == MaterialTypePlanetSurface){
        float factor = getPlanetSurface();
        positionWorldSpace -= normal * (factor) * 11.0;
    }
    if(UseBumpMap == 1){
        if(MaterialType == MaterialTypeGrass){
            float factor = (texture(bumpMap, UV).r);
            positionWorldSpace += normal * (factor) * 1.0;
            vec3 binormal = cross(normal, tangent);
            positionWorldSpace += tangent * (factor) * 0.4 * sns(positionWorldSpace.xz, 1, 1.0);
            positionWorldSpace += binormal * (factor) * 0.4 * sns(positionWorldSpace.xz, 1, 1.0);
        } else {
            //float factor = (texture(bumpMap, UV*0.01).r - 0.5);
            float factor = snoise(positionWorldSpace*normal / 4) * 0.2;
            factor += snoise(positionWorldSpace *10) * 0.03;
            positionWorldSpace += normal * (factor) * 1.3;
            normal = normalize(normal - (tangent * factor * 0.05));
        }
    
    }
	
   	gl_Position = ProjectionMatrix * ViewMatrix * vec4(positionWorldSpace, 1.0);
}
/**/