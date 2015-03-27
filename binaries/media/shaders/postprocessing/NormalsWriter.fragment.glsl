#version 430 core
#include Fragment.glsl
void main()
{	
	vec3 normalNew  = normal;
	if(UseNormalMap == 1){
		vec3 nmap = texture(normalMap, UV).rbg * 2.0 - 1.0;
		normalNew = (vec4(normalize(rotate_vector_by_vector(normal, nmap)), 1)).xyz;
		
	}
	
	outColor = vec4(normalNew, 1.0);
	updateDepth();
}