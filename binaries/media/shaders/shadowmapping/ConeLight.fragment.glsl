#version 430 core

out vec4 color;
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
	color = vec4(c, dist);
	
}