#version 430 core
in float time;
layout(location = 0) out vec3 outColor;
uniform samplerCube tex;
in vec3 normalCoord;	
in vec3 vectCoord;	 
in vec3 cubetexcoord;

in vec3 campos;

void main()
{

	vec3 basecolor = texture(tex, cubetexcoord).xyz;
	outColor = basecolor;
}