#version 430 core
in vec2 UV;
in float time;

in vec3 positionWorldSpace;
in vec3 campos;

out vec4 outColor;
uniform sampler2D tex;
in vec3 normalCoord;
 
void main()
{

	float dist = distance(campos, positionWorldSpace) / 10;
	
	//outColor = texture(tex, UV) + ((dist*dist*dist)/200.0);
	outColor = texture(tex, UV);
	//outColor = vec4(1.0, 0.0, 1.0, 1.0);
}