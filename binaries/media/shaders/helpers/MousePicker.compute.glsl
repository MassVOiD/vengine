#version 430 core

layout (binding = 0, rg32ui) readonly uniform uimage2D IDTex;
uniform vec2 Mouse;

layout (std430, binding = 0) buffer R1
{
  uint Result; 
}; 

layout( local_size_x = 1, local_size_y = 1, local_size_z = 1 ) in;

void main(){
     Result = imageLoad(IDTex, ivec2(Mouse)).r;
}