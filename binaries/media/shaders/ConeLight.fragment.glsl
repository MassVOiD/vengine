#version 430 core
#include Mesh3dUniforms.glsl
//in vec3 normal;
smooth in vec4 vertexPosition;

uniform vec3 LightPosition;
uniform float FarPlane;

out vec4 outColor;

void main()
{
	vec3 d1 = vertexPosition.xyz;
	vec3 d2 = LightPosition;
	float depth = length(vertexPosition.xyz - LightPosition);
	float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*FarPlane + 1.0f);
	gl_FragDepth = badass_depth;
    outColor = vec4(0,0,0,0);
}