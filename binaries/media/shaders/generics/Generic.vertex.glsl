#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

out Data {
#include InOutStageLayout.glsl
} Output;

uniform int InvertUVy;

void main(){

    vec4 v = vec4(in_position,1);
    Output.instanceId = int(gl_InstanceID);
    Output.TexCoord = vec2(in_uv.x, InvertUVy == 1 ? in_uv.y : (1.0 - in_uv.y));
    Output.WorldPos = transform_vertex(int(gl_InstanceID), v.xyz);
    Output.Normal = in_normal;
    Output.Tangent = in_tangent;
	vec4 outpoint = (VPMatrix) * vec4(Output.WorldPos, 1);
//	outpoint.w = 0.5 + 0.5 * outpoint.w;
	//outpoint.w = - outpoint.w;
	Output.Data.y = (outpoint.z / outpoint.w) * 0.5 + 0.5; 
    gl_Position = outpoint;// + vec4(0, 0.9, 0, 0);
}