#version 430 core

layout (std430, binding = 0) buffer B1
{
  mat4 Matrices[]; 
}; 
layout (std430, binding = 1) buffer B2
{
  vec4 Positions[]; 
}; 
layout (std430, binding = 2) buffer B3
{
  vec4 Velocities[]; 
}; 
layout (std430, binding = 3) buffer B4
{
  vec4 PathPoints[]; 
}; 

layout( local_size_x = 1, local_size_y = 1, local_size_z = 1 ) in;

uniform int BallsCount;
uniform int PathPointsCount;

#define GRAVITY (vec3(0, -0.12, 0))

vec3 processPath(uint index, vec3 position, float max_speed, float acceleration, float agility){
   // for(uint i=0;i<BallsCount;i++){
        vec3 p1 = vec3(0);
        vec3 p2 = vec3(0);
        float mindist = 99999;
        uint findex = 0;
        for(uint p=0;p<PathPointsCount;p++){
            vec3 pp = PathPoints[p].xyz;
            float d = distance(position, pp);
            if(mindist > d){
                mindist = d;
                p1 = pp;
                findex = p;
            }
        }
        mindist = 99999;
        uint findex2 = 0;
        for(uint p=0;p<PathPointsCount;p++){
            if(p == findex) continue;
            vec3 pp = PathPoints[p].xyz;
            float d = distance(position, pp);
            if(mindist > d){
                mindist = d;
                p2 = pp;
                findex2 = p;
            }
        }
        uint nid = findex == PathPointsCount - 1 ? 0 : findex + 1;
        vec3 nextPoint = PathPoints[nid].xyz;
        float d1 = distance(position, p1);
        float d2 = distance(position, p2);
        float sum = d1 + d2;
        float prc = d1 / sum;
        vec3 dirv = vec3(0);
        if(d1 < 2.7){            
             if(findex2 == nid){
                dirv = normalize(nextPoint - position);
            } else {
                vec3 np = p1 + normalize(nextPoint - p1) * 5;
                dirv = normalize(np - position) ;
            }
        } else {
            if(findex2 == nid){
                dirv = normalize(nextPoint - position);
            } else {
                dirv = normalize(p1 - position) ;
            }
        }
        
        float speed = clamp(min(d1, d2) * 1.00, 1.0, 8.0);
        
        return dirv * speed * max_speed;
    //}
}

uint getIndex(uint x, uint y, uint z){
    return x * gl_NumWorkGroups.y * gl_NumWorkGroups.z +
            y * gl_NumWorkGroups.z +
            z;
}

vec3 processCloth(uint index, vec3 position){
    uint gx = gl_WorkGroupID.x;
    uint gy = gl_WorkGroupID.y;
    uint gz = gl_WorkGroupID.z;
    if(gy == 0) return vec3(0);
    
    uint gidny = gy == 0 ? -1 : 
          getIndex(gx, gy - 1, gz);
    uint gidpy = gy == gl_NumWorkGroups.y - 1 ? -1 : 
          getIndex(gx, gy + 1, gz);
          
    uint gidnx = gx == 0 ? -1 : 
          getIndex(gx - 1, gy, gz);
    uint gidpx = gx == gl_NumWorkGroups.x - 1 ? -1 : 
          getIndex(gx + 1, gy, gz);
         
    uint gidnz = gz == 0 ? -1 : 
          getIndex(gx, gy, gz - 1);
    uint gidpz = gz == gl_NumWorkGroups.z - 1 ? -1 : 
          getIndex(gx, gy, gz + 1);
          
    bool isTopRow = gidpy == -1;
    //uint indices[] = {gidny, gidpy, gidnx, gidpx, gidnz, gidpz};
    uint indices[] = {gidny, gidpy};
    vec3 avrPos = vec3(0);
    vec3 avrVec = vec3(0);
    uint counter = 0;
    float dis = float( gl_NumWorkGroups.y / 2 - gy);
    for(int i=0;i<2;i++){
        if(indices[i] != -1){
            vec3 a = Positions[indices[i]].xyz - Positions[index].xyz;
            float len = length(a);
            vec3 dir = normalize(a) * clamp(len * 0.1, 0, 1);
            vec3 vel = Velocities[indices[i]].xyz;
            if(len < 1.5) dir *= 0;
            //if(len < 1.5) vel *= 0;
            avrPos +=  dir * (2 - min(i,1));
            avrVec += vel * (2 - min(i,1));
            counter++;
        } 
    }
    avrPos /= counter;
    vec3 diff = avrPos;
    //barrier();
    //Positions[group] = avrPos;
    return diff * 1 + ((Velocities[index].xyz + avrVec / counter) * 0.28) + GRAVITY;
}

uniform float Time;

#include noise3D.glsl

void main(){
	uint group = getIndex(gl_WorkGroupID.x, gl_WorkGroupID.y, gl_WorkGroupID.z);
		  
	vec3 translation = Positions[group].xyz;
	float bs = float(BallsCount);
    
    //vec3 vv = processPath(group, translation, 5.1, 1.0, 1.0);
    //vec3 vv = processPoint(group, translation, 2.1, 1.0, 1.0);
    //barrier();
    vec3 vv = processCloth(group, translation);
    barrier();
    vec3 vel = vv;
    
    vel *= 0.7789;
    //vel += ;
    int smoother = 3;
    vec4 fv = vec4((Velocities[group].xyz * smoother + vel) / (smoother + 1), 1);
    fv.z += snoise(vec3(translation.x/22, translation.y/22, translation.z/22 + Time) ) *0.01;
    fv.x += snoise(vec3(translation.x/12, translation.y/45, translation.z/33 + Time) ) *0.06;
    Velocities[group] = fv.xyzz;
    float fin = float(gl_WorkGroupID.y) / float(gl_NumWorkGroups.y);
	translation += fv.xyz * 2 * fin;
	Positions[group] = vec4(translation, 1);
    
	//translation.x += 0.01 * group;
	//translation.z += 0.01 * group;
	mat4 m = Matrices[group];
	m[3][0] = translation.x;
	m[3][1] = translation.y;
	m[3][2] = translation.z;
	Matrices[group] = m;
}