#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl

out Data {
    flat int instanceId;
    vec3 WorldPos;
    vec2 TexCoord;
    vec3 Normal;
    vec3 Tangent;
    vec3 Data;
} Output;

#include Bones.glsl

uniform int MaterialType;
#define MaterialTypeRainsOptimizedSphere 13
layout (std430, binding = 4) buffer BallsBuff
{
    vec4 BallsPositionsAndScales[]; 
}; 

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
    vec3 tang = in_tangent;
    if(MaterialType == MaterialTypeRainsOptimizedSphere){
       // if(int(gl_InstanceID) >= BallsPositionsAndScales.length()) return;
        vec4 a = BallsPositionsAndScales[gl_InstanceID];
        vec3 n = normalize(CameraPosition - a.xyz);
        norm = n;
        vec3 tu = CameraTangentUp;
        tang = tu;
        vec3 tl = CameraTangentLeft;
        wpos = a.xyz - tl - tu + tl * 2.0 * in_uv.x + tu * 2.0 * in_uv.y;
    }
    Output.WorldPos = wpos;

	Output.Normal = norm;
	Output.Tangent = tang;
	
	
    gl_Position = (ProjectionMatrix  * ViewMatrix) * vec4(wpos, 1);
}