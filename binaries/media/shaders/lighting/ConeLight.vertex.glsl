#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl


uniform vec3 LightPosition;
out Data {
    int instanceId;
    vec3 WorldPos;
    vec2 TexCoord;
    vec3 Normal;
    vec3 Tangent;
    vec3 Data;
} Output;
uniform int Instanced;
#include Bones.glsl


void main(){
    vec4 v = vec4(in_position,1);
    mat4 mmat = ModelMatrix;
    if(Instances > 0) mmat = ModelMatrixes[gl_InstanceID];
    mat4 mvp = ProjectionMatrix * CameraTransformation * ViewMatrix * InitialTransformation *mmat;

    vec3 mspace = v.xyz;
    if(UseBoneSystem == 1){
        int bone = determineBone(mspace);
        mspace = applyBoneRotationChain(mspace, bone);
        //inorm = applyBoneRotationChainnormal(inorm, bone);
    }
    v = vec4(mspace, 1);    

    Output.WorldPos = (InitialTransformation * mmat * v).xyz;
    Output.Normal = in_normal;
    Output.Tangent = in_tangent;
    Output.instanceId = int(gl_InstanceID);
    Output.TexCoord = vec2(in_uv.x, -in_uv.y);

    gl_Position = mvp * v;
}