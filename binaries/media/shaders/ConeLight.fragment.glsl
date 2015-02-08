#version 430 core
//in vec3 normal;
smooth in vec3 vertexWorldSpace;

uniform vec3 LightPosition;
uniform float FarPlane;
uniform float LogEnchacer;

out float outColor;	

void main()
{
	float depth = distance(vertexWorldSpace, LightPosition);
	float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*FarPlane + 1.0f);
	gl_FragDepth = badass_depth;
		
    outColor = 0;
}