#version 430 core
#include Fragment.glsl

layout(binding = 0) uniform sampler2D bumpMap;

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
vec3 perturb_bump( float B, vec3 N, vec3 V)
{

   vec3 map = vec3(0.5, 0.5, 1);
   map = map * 255./127. - 128./127.;
    mat3 TBN = cotangent_frame(N, -V - (N*B), UV);
    return normalize(TBN * map);
}

uniform float NormalMapScale;

void main()
{	
	if(IgnoreLighting == 0){
		vec3 normalNew  = normal;
		if(UseNormalMap == 1){
			//vec3 nmap = texture(normalMap, UV * NormalMapScale).rbg;
			//normalNew = normalize(rotate_vector_by_vector(normal, nmap));
			normalNew = perturb_normal(normal, positionWorldSpace, UV * NormalMapScale);
			
		}
		if(UseBumpMap == 1){
			float bmap = texture(bumpMap, UV).r;
			//normalNew = normalize(rotate_vector_by_vector(normal, nmap));
			normalNew = perturb_bump(bmap, normal, positionWorldSpace);
			
		}
		
		outColor = vec4((RotationMatrixes[instanceId] * vec4(normalNew, 0)).xyz, SpecularSize);
	} else {
		outColor = vec4(0, 0, 0, 0);
	}
	updateDepth();
}