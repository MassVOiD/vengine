#version 450 core
layout( local_size_x = 16, local_size_y = 8, local_size_z = 8 ) in;


layout (binding = 6, r32ui) uniform uimage3D VoxelsTextureRed;
layout (binding = 1, r32ui) uniform uimage3D VoxelsTextureGreen;
layout (binding = 2, r32ui) uniform uimage3D VoxelsTextureBlue;
layout (binding = 3, r32ui) uniform uimage3D VoxelsTextureCount;
layout (binding = 4, rgba16f) uniform image3D VoxelsTextureResult;


void main(){
    ivec3 ba = ivec3(gl_GlobalInvocationID.xyz);
    uint r = imageLoad(VoxelsTextureRed, ba).r;
    uint g = imageLoad(VoxelsTextureGreen, ba).r;
    uint b = imageLoad(VoxelsTextureBlue, ba).r;
    uint c = imageLoad(VoxelsTextureCount, ba).r;
    vec3 rgb = c == 0 ? vec3(0) : vec3(r, g, b) / 128.0 / float(c);
    //imageStore(VoxelsTextureResult, ivec3(gl_GlobalInvocationID.xyz), vec4(rgb, float(c)));
    imageStore(VoxelsTextureResult, ba, vec4(rgb, float(c)));

}