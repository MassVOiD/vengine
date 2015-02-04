#version 430 core

//in vec3 normal;
in vec4 vertexPosition;

uniform vec3 LightPosition;
uniform float FarPlane;

out vec4 outColor;

void main()
{
	vec3 d1 = vertexPosition.xyz;
	vec3 d2 = LightPosition;
	float logEnchancer = 0.1f;
	float depth = length(vertexPosition.xyz - LightPosition);
	float badass_depth = log(logEnchancer*depth + 1.0f) / log(logEnchancer*FarPlane + 1.0f);
	gl_FragDepth = badass_depth;
    outColor = vec4(0,0,0,0);
}