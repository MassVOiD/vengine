#version 430 core
#include Fragment.glsl
void main()
{
	outColor = vec4(positionWorldSpace.xyz, 1.0);
	updateDepth();
}