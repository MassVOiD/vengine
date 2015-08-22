#version 430 core

layout (binding = 0, rgba8) readonly uniform image2D IDTex;
uniform vec2 Mouse;

layout (std430, binding = 0) buffer R1
{
  vec4 Result[]; 
}; 

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;

void main(){
    ivec2 iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    if(distance(vec2(iUV), Mouse) < 3) Result[0] = imageLoad(IDTex, iUV);
}