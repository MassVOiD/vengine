#version 430 core
//in vec3 normal;
smooth in vec3 vertexWorldSpace;
smooth in vec3 positionWorldSpace;
uniform vec3 LightPosition;
uniform vec3 CameraPosition;
uniform float FarPlane;
uniform float LogEnchacer;
#include LogDepth.glsl


out float outColor;	

void main()
{
	float depth = distance(vertexWorldSpace, LightPosition);
	float badass_depth = toLogDepth(depth);
	gl_FragDepth = badass_depth;
		
    outColor = 0;
}