in vec2 UV;
uniform vec4 input_Color;
layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outWorldPos;
layout(location = 2) out vec4 outNormals;
layout(location = 3) out vec4 outMeshData;
#include LogDepth.glsl
#include Lighting.glsl

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

uniform float NormalMapScale;

void finishFragment(vec4 color){
	outColor = vec4((color.xyz) * DiffuseComponent, color.a);
	outWorldPos = vec4(positionWorldSpace.xyz, SpecularComponent);
	if(IgnoreLighting == 0){
		vec3 normalNew  = normal;
		if(UseNormalMap == 1){
			//normalNew = perturb_normal(normal, positionWorldSpace, UV * NormalMapScale);
			
		}
		
		outNormals = vec4((RotationMatrixes[instanceId] * vec4(normalNew, 0)).xyz, SpecularSize);
	} else {
		outNormals = vec4(0, 0, 0, 0);
	}	
	outMeshData = vec4(0, 0, 0, 0);
	updateDepth();
}