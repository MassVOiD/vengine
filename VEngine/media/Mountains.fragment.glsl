#version 430 core

in vec4 positionWorldSpace;
uniform vec3 CameraPosition;

out vec4 outColor;

float hash3(vec3 uv) {
	return fract(sin(uv.x * 15.78 + uv.y * 35.14 + uv.z * 26.1134) * 43758.23);
}

void main()
{
	vec4 color = vec4(0.1, 0.9, 0.1, 1.0) + (hash3(positionWorldSpace.xyz) * 0.2);
	
	color.r += clamp(positionWorldSpace.y/500.0, 0.0, 1.0);
	if(positionWorldSpace.y > 300.0){
		color *= clamp((positionWorldSpace.y - 300.0)/50.0, 1.0, 10.0);
	}

	float d = clamp(180.0f / length(CameraPosition - positionWorldSpace.xyz), 0.0, 1.0);
	color *= d;
	
    outColor = color;
}