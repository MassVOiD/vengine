#version 430 core

layout (std430, binding = 0) buffer SSBOTest
{
  mat4 Matrices[]; 
}; 
layout (std430, binding = 1) buffer SSBOTest2
{
  vec3 Positions[]; 
}; 
layout (std430, binding = 2) buffer SSBOTest3
{
  vec3 Velocities[]; 
}; 

layout( local_size_x = 1, local_size_y = 1, local_size_z = 1 ) in;

uniform int BallsCount;

#define GRAVITY (vec3(0, -2.2, 0))

void processCollision(uint index, vec3 position, float radius){
    for(uint i=0;i<BallsCount;i++){
        vec3 translation = Positions[i];
        if(distance(position, translation) < radius * 2){
            //Velocities[index] = -Velocities[index];
            //Velocities[i] = -Velocities[i];
            barrier();
            float angle = max(0, dot(normalize(Velocities[index]), normalize(translation - position)));
            float angleinv = 1.0 - angle;
            barrier();
            Velocities[i] = (Velocities[index]  * angleinv) + (Velocities[i] * angle);
            Velocities[index] = (reflect(Velocities[index], normalize(translation - position))  * angle) + (Velocities[index] * angleinv);
            barrier();
            
        }
    }
}

void main(){

	uint group = 
          gl_WorkGroupID.z * gl_NumWorkGroups.x * gl_NumWorkGroups.y +
          gl_WorkGroupID.y * gl_NumWorkGroups.x + 
          gl_WorkGroupID.x;
		  
	vec3 translation = Positions[group];
	float bs = float(BallsCount);
    vec3 vel = Velocities[group];
	translation += vel;
    float limit = 20;
    if(translation.x > limit){
        vel.x = -vel.x;
    }    
    if(translation.y > limit){
        vel.y = -vel.y;
    }    
    if(translation.z > limit){
        vel.z = -vel.z;
    }
    
    if(translation.x < -limit){
        vel.x = -vel.x;
    }    
    if(translation.y < limit){
        vel.y = -vel.y;
    }    
    if(translation.z < -limit){
        vel.z = -vel.z;
    }
    translation = clamp(translation, -limit, limit);
    
    //vel *= 0.999;
    vel += GRAVITY;
    
    Velocities[group] = vel;
	Positions[group] = translation;
    barrier();
    processCollision(group, translation, 1.0);
    
	//translation.x += 0.01 * group;
	//translation.z += 0.01 * group;
	mat4 m = Matrices[group];
	m[3][0] = translation.x;
	m[3][1] = translation.y;
	m[3][2] = translation.z;
	Matrices[group] = m;
}