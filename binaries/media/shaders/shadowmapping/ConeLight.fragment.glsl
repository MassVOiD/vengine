#version 430 core

layout(location = 0) out vec4 outDiffuseColorDistance;
layout(location = 1) out vec4 outNormals;
in Data {
#include InOutStageLayout.glsl
} Input;
#include Mesh3dUniforms.glsl
#include LightingSamplers.glsl
void main()
{
	float dist = distance(CameraPosition, Input.WorldPos);
	vec3 c = DiffuseColor;
	if(UseDiffuseTex == 1) c = texture(diffuseTex, Input.TexCoord).rgb; 
	outDiffuseColorDistance = vec4(c, dist);
	outNormals = vec4((RotationMatrixes[Input.instanceId] * vec4(normalize(Input.Normal), 0)).rgb, dist);
	
}