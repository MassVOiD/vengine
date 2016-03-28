#version 430 core
#line 3

uniform int Grid;
uniform int Pass;
out vec4 c;
struct Box{
	uint hits;
	int normalhitsx;
	int normalhitsy;
	int normalhitsz;
};
layout (std430, binding = 9) coherent buffer BoxesBuffer
{
    Box Boxes[]; 
}; 

in vec4 Position;
in vec4 innormal;

int encodeFloat(float val){
	return int(128.0 * val);
}

uint getIndex(uint x, uint y, uint z){
    return x * Grid * Grid +
            y * Grid +
            z;
}

void incAtIndex(uvec3 i, vec3 normal){
    uint index = getIndex(i.x, i.y, i.z);
    atomicAdd(Boxes[index].hits, 1);
    atomicAdd(Boxes[index].normalhitsx, encodeFloat(normal.x));
    atomicAdd(Boxes[index].normalhitsy, encodeFloat(normal.y));
    atomicAdd(Boxes[index].normalhitsz, encodeFloat(normal.z));
}

void main()
{
	vec3 v = Position.xyz;
	if(Pass == 1) v = v.zxy;
	if(Pass == 2) v = v.yzx;
    vec3 bcord = Position.xyz * 0.5 + 0.5;
    incAtIndex(uvec3(
        uint(floor(bcord.x * Grid)),
        uint(floor(bcord.y * Grid)),
        uint(floor(bcord.z * Grid))), normalize(innormal.xyz));
	c = vec4(0);
	memoryBarrier();
	barrier();
}