#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

out Data {
#include InOutStageLayout.glsl
} Output;

void main(){

    vec4 v = vec4(in_position,1);
	Output.instanceId = int(gl_InstanceID);
	Output.TexCoord = vec2(in_uv.x, 1.0 - in_uv.y);
    Output.WorldPos = (ModelMatrixes[gl_InstanceID] * v).xyz;
	Output.Normal = in_normal;
	Output.Tangent = in_tangent;
    gl_Position = (VPMatrix) * vec4(Output.WorldPos, 1);
}