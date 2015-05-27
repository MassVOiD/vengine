in vec2 UV;
in vec3 tangent;
uniform vec4 input_Color;
layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outWorldPos;
layout(location = 2) out vec4 outNormals;
layout(location = 3) out vec4 outMeshData;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl

uniform int MaterialType;
#define MaterialTypeSolid 0
#define MaterialTypeRandomlyDisplaced 1
#define MaterialTypeWater 2
#define MaterialTypeSky 3
#define MaterialTypeWetDrops 4

layout(binding = 16) uniform sampler2D bumpMap;

vec3 rotate_vector_by_quat( vec4 quat, vec3 vec )
{
	return vec + 2.0 * cross( cross( vec, quat.xyz ) + quat.w * vec, quat.xyz );
}

vec3 rotate_vector_by_vector( vec3 vec_first, vec3 vec_sec )
{
	vec3 zeros = vec3(0.0, 1.0, 0.0);
	vec3 cr = cross(zeros, vec_sec);
	float angle = dot(normalize(cr), normalize(vec_sec));
	return rotate_vector_by_quat(vec4(cr, angle), vec_first);
}

mat3 cotangent_frame(vec3 N, vec3 p, vec2 uv)
{
    // get edge vectors of the pixel triangle
    vec3 dp1 = dFdx( p );
    vec3 dp2 = dFdy( p );
    vec2 duv1 = dFdx( uv );
    vec2 duv2 = dFdy( uv );
 
    // solve the linear system
    vec3 dp2perp = cross( dp2, N );
    vec3 dp1perp = cross( N, dp1 );
    vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;
 
    // construct a scale-invariant frame 
    float invmax = inversesqrt( max( dot(T,T), dot(B,B) ) );
    return mat3( T * invmax, B * invmax, N );
}

vec3 perturb_normal( vec3 N, vec3 V, vec2 texcoord )
{
    // assume N, the interpolated vertex normal and 
    // V, the view vector (vertex to eye)
   vec3 map = texture(normalMap, texcoord ).xyz;
   map = map * 255./127. - 128./127.;
    mat3 TBN = cotangent_frame(N, -V, texcoord);
    return normalize(TBN * map);
}
vec3 perturb_normalRaw( vec3 N, vec3 V, vec3 map )
{
    // assume N, the interpolated vertex normal and 
    // V, the view vector (vertex to eye)
   map = map * 255./127. - 128./127.;
    mat3 TBN = cotangent_frame(N, -V, UV);
    return normalize(TBN * map);
}
vec3 perturb_bump( float B, vec3 N, vec3 V)
{

   vec3 map = vec3(0.5, 0.5, 1);
   map = map * 255./127. - 128./127.;
    mat3 TBN = cotangent_frame(N, -V - (N*B), UV);
    return normalize(TBN * map);
}
#ifdef GL_ES
precision mediump float;
#endif

vec3 mod289(vec3 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 permute(vec4 x) {
    return mod289(((x*34.0)+1.0)*x);
}

vec4 taylorInvSqrt(vec4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(vec3 v)
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
    return snoise(vec3(p.x*scale, p.y*scale, Time * tscale * 0.5));
}
float getwater( vec2 position ) {

    float color = 0.0;
    color += sns(position, 64., 4.);
    color += sns(position, 128., 2.);
    color += sns(position, 256., 2.) * 2.;
    return clamp(color / 7.0 + 0.5, 0, 1) * 2 - 1;

}
uniform float NormalMapScale;

void finishFragment(vec4 color){
	outColor = vec4((color.xyz) * DiffuseComponent, color.a);
	outWorldPos = vec4(ToCameraSpace(positionWorldSpace.xyz), 1);
	if(IgnoreLighting == 0){
		vec3 normalNew  = normalize(normal);
		if(UseNormalMap == 1){
			normalNew = perturb_normal(normalNew, positionWorldSpace, UV * NormalMapScale);
    
		} else if(UseBumpMap == 1){
            float factor = (texture(bumpMap, UV).r - 0.5);
            //vec3 bitan = cross(normal, tangent);
			normalNew = normalize(normalNew - (tangent * factor));
    
		} else {
            float factor = (length(vec3(0.5)) - length(color.xyz)) * 0.2;
			normalNew = normalize(normalNew - (tangent * factor));
    
		}
        if(MaterialType == MaterialTypeWater){
            float factor = getwater(UV * 5) * 0.3;
			normalNew = normalize(normalNew - (tangent * factor));
           // outColor.xyz *= (factor + 1) / 8 + 0.75;
        }
        if(MaterialType == MaterialTypeWetDrops){
            float pn = snoise(positionWorldSpace* 13.)  * 2 - 1;
            //pn = clamp(pow(pn, 3.0), 0.5, 1.0);
			normalNew = normalize(normalNew - (tangent * pn * 0.05));
           // outColor.xyz *= (factor + 1) / 8 + 0.75;
        }
		if(Instances == 1){
			outNormals = vec4((RotationMatrix * vec4(normalNew, 0)).xyz, 1);
		} else {
			outNormals = vec4((RotationMatrixes[instanceId] * vec4(normalNew, 0)).xyz, 1);
		}
	} else {
		outNormals = vec4(0, 0, 0, 1);
	}	
	// mesh data is packed as follows:
	/*
	outColor.a - invalid to read
	outWorldPos.a - specular component
	outNormals.a - specular size
	outMeshData.r - reflection strength
	outMeshData.g - refraction strength
	*/
	outMeshData = vec4(ReflectionStrength, RefractionStrength, color.a, 1);
	updateDepth();
}