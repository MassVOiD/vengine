#version 450 core
layout( local_size_x = 8, local_size_y = 8, local_size_z = 8 ) in;

layout(binding = 0) uniform sampler3D VoxelsTextureIn;
layout (binding = 4, rgba16f) writeonly uniform image3D VoxelsTextureOut;

#define LOOKUPS 8
ivec3 lut[] = ivec3[](
    ivec3(0,0,0),
    
    ivec3(1,0,0),
    ivec3(0,1,0),
    ivec3(0,0,1),
    
    ivec3(1,1,0),
    ivec3(0,1,1),
    ivec3(1,0,1),
    
    ivec3(1,1,1)
);
void main(){
    ivec3 baIn = ivec3(gl_GlobalInvocationID.xyz * 2 );
    ivec3 baOut = ivec3(gl_GlobalInvocationID.xyz);
    
    vec4 color = vec4(0);
    ivec3 txs = ivec3(textureSize(VoxelsTextureIn, 0));
    ivec3 zerso = ivec3(0);
    for(int i=0;i<LOOKUPS;i++){
        ivec3 iv = baIn + lut[i];
        color += texelFetch(VoxelsTextureIn, iv, 0);
    }
        
    imageStore(VoxelsTextureOut, baOut, color / LOOKUPS);
 //   memoryBarrier();
}