#version 430 core
#include AttributeLayout.glsl
#include Mesh3dUniforms.glsl


uniform vec3 LightPosition;

uniform int Instanced;
out flat int instanceId;

smooth out vec2 UV;
smooth out vec3 normal;
#include Bones.glsl

//out vec3 normal;
smooth out vec3 vertexWorldSpace;

void main(){
    vec4 v = vec4(in_position,1);
    mat4 mvp = ProjectionMatrix * ViewMatrix;
    if(Instances != 1){
        mvp = mvp * ModelMatrixes[gl_InstanceID];
        vertexWorldSpace = (ModelMatrixes[gl_InstanceID] * v).xyz;
    } else {
        vec3 mspace = v.xyz;
        if(UseBoneSystem == 1){
            int bone = determineBone(mspace);
            mspace = applyBoneRotationChain(mspace, bone);
            //inorm = applyBoneRotationChainNormal(inorm, bone);
        }
        v = vec4(mspace, 1);    
        mvp = mvp * ModelMatrix;
        vertexWorldSpace = (ModelMatrix * v).xyz;
    }
    normal = in_normal;
    instanceId = gl_InstanceID;
    UV = vec2(in_uv.x, -in_uv.y);

    gl_Position = mvp * v;
}