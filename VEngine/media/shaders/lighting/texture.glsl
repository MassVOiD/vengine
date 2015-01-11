#version 330 core
in vec2 UV;
in float time;
layout(location = 0) out vec3 outColor;
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
	float dotdiffuse = dot(normalize(-light_eye), normalize (n_eye));
	float angle = clamp(dotdiffuse, 0.0, 1.0);
	return (angle)*2;
}

float specular(vec3 n_eye){
	vec3 newcam = cameraPosition - pos_eye;
	vec3 R = reflect(light_eye, n_eye);
	float cosAlpha = dot(normalize(newcam), normalize(R));
	float clamped = clamp(cosAlpha, 0.0, 1.0);
	return pow(clamped/2, 38.0);
}

void main()
{

	vec3 nmap = texture(normalMap, UV * 10.0).rgb * 2.0 - 1.0;
	vec3 n_eye = vec3 (rotationMatrix * vec4(normalize(normalFS + nmap), 1));
	
	vec3 basecolor = texture(tex, UV*5.0).xyz;
	
	vec3 spec = vec3(1.0, 1.0, 1.0) * specular(n_eye);
	vec3 diff = vec3(1.0, 1.0, 1.0) * diffuse(n_eye);
	
	outColor = (basecolor * diff + spec).xyz;

}