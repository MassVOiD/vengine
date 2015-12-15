#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

out Data {
#include InOutStageLayout.glsl
} Output;

/*#include Bones.glsl

uniform int MaterialType;
#define MaterialTypeRainsOptimizedSphere 13
layout (std430, binding = 4) buffer BallsBuff
{
    vec4 BallsPositionsAndScales[]; 
}; 
*/
// not optimized
/*
void main(){

	Output.instanceId = int(gl_InstanceID);
    vec4 v = vec4(in_position,1);
    

    
	Output.TexCoord = vec2(in_uv.x, in_uv.y);
    
    vec3 inorm = in_normal;
	mat4 mmat = ModelMatrixes[gl_InstanceID];

    vec3 mspace = v.xyz;

    //v = vec4(mspace, 1);
    vec3 wpos = (mmat * v).xyz;
    vec3 norm = inorm;
    vec4 tang = in_tangent;
    if(MaterialType == MaterialTypeRainsOptimizedSphere){
       // if(int(gl_InstanceID) >= BallsPositionsAndScales.length()) return;
        vec4 a = BallsPositionsAndScales[gl_InstanceID];
        vec3 n = normalize(CameraPosition - a.xyz);
        norm = n;
        vec3 tu = CameraTangentUp;
        tang = vec4(tu, tang.w);
        vec3 tl = CameraTangentLeft;
        wpos = a.xyz - tl - tu + tl * 2.0 * in_uv.x + tu * 2.0 * in_uv.y;
    }
    Output.WorldPos = wpos;

	Output.Normal = norm;
	Output.Tangent = tang;
	
	
    gl_Position = (ProjectionMatrix  * ViewMatrix) * vec4(wpos, 1);
}*/

//optimized

void main(){

    vec4 v = vec4(in_position,1);
	Output.instanceId = int(gl_InstanceID);
	Output.TexCoord = vec2(in_uv.x, in_uv.y);
    Output.WorldPos = (ModelMatrixes[gl_InstanceID] * v).xyz;
	Output.Normal = in_normal;
	Output.Tangent = in_tangent;
    gl_Position = (VPMatrix) * vec4(Output.WorldPos, 1);
}