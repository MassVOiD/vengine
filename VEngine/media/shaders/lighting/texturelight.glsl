#version 430 core
in vec2 UV;
in float time;
layout(location = 0) out vec4 outColor;
uniform sampler2D tex;
in vec3 normalCoord;	
in vec3 vectCoord;	 
in mat4 VM;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 rotationMatrix;
uniform mat4 globalTransformationMatrix;
uniform vec3 cameraPosition;
in vec3 campos;

smooth in vec3 positionWorldSpace;
smooth in vec3 pos_eye;
smooth in mat4 final_Trans;
smooth in vec3 light_eye;

smooth in vec3 normalFS;

uniform sampler2D normalMap;

float diffuse(vec3 n_eye){
	vec3 lightRelativeToVPos = light_eye - pos_eye;
	float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (n_eye));
	float angle = clamp(dotdiffuse, 0.0, 1.0);
	return (angle)*2;
}

float specular(vec3 n_eye){
	vec3 lightRelativeToVPos = light_eye - pos_eye;
	vec3 cameraRelativeToVPos = cameraPosition - pos_eye;
	vec3 R = reflect(lightRelativeToVPos, n_eye);
	float cosAlpha = dot(normalize(cameraRelativeToVPos), normalize(R));
	float clamped = clamp(-cosAlpha, 0.0, 1.0);
	return pow(clamped, 518.0);
}

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

void main()
{

	vec3 nmap = texture(normalMap, UV * 10.0).rbg * 2.0 - 1.0;
	vec3 n_eye = vec3 (rotationMatrix * vec4(normalize(rotate_vector_by_vector(normalFS, nmap)), 1));
	
	vec3 basecolor = texture(tex, UV*5.0).xyz;
	
	vec3 spec = vec3(1.0, 1.0, 1.0) * specular(n_eye);
	vec3 diff = vec3(1.0, 1.0, 1.0) * diffuse(n_eye);
	
	outColor = vec4((basecolor * diff + spec).xyz, 0.5);
	
	//vec3 basecolor = texture(tex, UV*5.0).xyz;
	//vec3 basecolor = vec3(0.0, 0.3, 0.9);
	//basecolor = basecolor * positionWorldSpace.y;
	//outColor = vec4((basecolor).xyz, 0.5);
}