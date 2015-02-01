#version 430 core

in vec4 positionWorldSpace;
uniform vec3 CameraPosition;

out vec4 outColor;

void main()
{
	vec4 color = vec4(0.3, 0.3, 0.3, 1.0);
	
	if(mod(positionWorldSpace.x, 6.0f) < 3.0f && mod(positionWorldSpace.z, 6.0f) < 3.0f){
		color.x = 1.0f;
		color.y = 1.0f;
		color.z = 1.0f;
	}
	float d = clamp(80.0f / length(CameraPosition - positionWorldSpace.xyz), 0.0, 1.0);
	color *= d;
	
    outColor = color;
}