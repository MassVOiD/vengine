#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

out int instanceId_CS_in;

out vec3 ModelPos_CS_in;
out vec3 WorldPos_CS_in;
out vec2 TexCoord_CS_in;
out vec3 Normal_CS_in;
//out vec3 Barycentric_CS_in;
out vec3 Tangent_CS_in;

void main(){

    vec4 v = vec4(in_position,1);
    //vec4 n = vec4(in_normal,0);
	//Barycentric_CS_in = vec3(0, 0, 1);
    

    
	if(Instances > 0){
		WorldPos_CS_in = (ModelMatrixes[gl_InstanceID] * v).xyz;

	} else {
		WorldPos_CS_in = (ModelMatrix * v).xyz;
	}
    ModelPos_CS_in = v.xyz;
    Normal_CS_in = in_normal;
    Tangent_CS_in = in_tangent;
	instanceId_CS_in = gl_InstanceID;
	TexCoord_CS_in = vec2(in_uv.x, -in_uv.y);
	
}