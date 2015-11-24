#version 430 core

uniform int Grid;
out vec4 c;
layout (std430, binding = 4) buffer BoxesBuffer
{
    uint Boxes[]; 
}; 

in vec4 Position;

uint getIndex(uint x, uint y, uint z){
    return x * Grid * Grid +
            y * Grid +
            z;
}

void incAtIndex(uvec3 i){
    uint index = getIndex(i.x, i.y, i.z);
    atomicAdd(Boxes[index], 1);
}

void main()
{
    vec3 bcord = Position.xyz * 0.5 + 0.5;
    incAtIndex(uvec3(
        uint(floor(bcord.x * Grid)),
        uint(floor(bcord.y * Grid)),
        uint(floor(bcord.z * Grid))));
	c = vec4(0);
}