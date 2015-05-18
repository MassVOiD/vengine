#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

out int instanceId_CS_in;
smooth out vec3 barycentric;

out vec3 ModelPos_CS_in;
out vec3 WorldPos_CS_in;
out vec2 TexCoord_CS_in;
out vec3 Normal_CS_in;
out vec3 Barycentric_CS_in;

void main(){

    vec4 v = vec4(in_position,1);
    //vec4 n = vec4(in_normal,0);
	Barycentric_CS_in = vec3(0, 0, 1);
    

    
	if(Instances > 1){
		ModelPos_CS_in = v.xyz;
		WorldPos_CS_in = (ModelMatrixes[gl_InstanceID] * v).xyz;
		Normal_CS_in = (RotationMatrixes[gl_InstanceID] * vec4(in_normal, 1.0)).xyz;

	} else {
		ModelPos_CS_in = v.xyz;
		WorldPos_CS_in = (ModelMatrix * v).xyz;
		Normal_CS_in = (RotationMatrix * vec4(in_normal, 1.0)).xyz;
	}
	instanceId_CS_in = gl_InstanceID;
	TexCoord_CS_in = vec2(in_uv.x, -in_uv.y);
	
}