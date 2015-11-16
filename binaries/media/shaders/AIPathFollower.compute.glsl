#version 430 core
layout( local_size_x = 1000, local_size_y = 1, local_size_z = 1 ) in;

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
layout (std430, binding = 4) buffer B5
{
    vec4 Pressure[]; 
}; 

struct UniBallProps{
    vec4 PositionAndScale;
    vec4 VelocityAndPressure;
    vec4 Accumulator;
    vec4 DampingAndMass;
    int Joints[8]; 
};



uniform int BallsCount;
uniform int PathPointsCount;

#define GRAVITY (vec3(0, -0.02, 0))

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

void processBallPhysics(uint group){
    vec3 translation = Positions[group].xyz;
    vec3 velocity = Velocities[group].xyz;
    

    translation += velocity * 0.8;
    memoryBarrierBuffer();
    for(int i=0;i<BallsCount;i++) if(i!=group){
        vec3 t = Positions[i].xyz;
        vec3 dir = normalize(t - translation);
        vec3 rvel = Velocities[i].xyz ;
        float dt = max(0, dot(normalize(velocity), dir));
        float dt2 = max(0, dot(normalize(rvel), dir));
        if(distance(t, translation) < 2){
            //barrier();
            
            velocity = reflect(Velocities[i].xyz, -dir) * dt2 + velocity * (1.0-dt2);
            //Velocities[i].xyz = reflect(velocity, dir) * dt + rvel * (1.0-dt);
            translation =  t - dir * 2.0;
            memoryBarrierBuffer();
            //break;
        }
    }
    memoryBarrierBuffer();
    // velocity = Velocities[group].xyz;
    velocity += GRAVITY;
    // velocity += (vec3(0,0,70) - translation)*0.001 ;
    
    if(translation.y - 1 < 0) {
        velocity.y = abs(velocity.y) * 0.88+0.0;
        translation.y = 1;
    }
    #define barr 22
    if(translation.x < -barr) {
        velocity.x = abs(velocity.x) * 0.98;
        translation.x = -barr;
    }
    if(translation.x > barr) {
        velocity.x = -abs(velocity.x) * 0.98;
        translation.x = barr;
    }
    
    if(translation.z < -barr) {
        velocity.z = abs(velocity.z) * 0.98;
        translation.z = -barr;
    }
    if(translation.z > barr) {
        velocity.z = -abs(velocity.z) * 0.98;
        translation.z = barr;
    }
    velocity *= 0.993;
    if(length(velocity) > 5) velocity = normalize(velocity)*5;

    Positions[group] = vec4(translation, 1);
    Velocities[group] = vec4(velocity, 1);
    
    //translation.x += 0.01 * group;
    //translation.z += 0.01 * group;
    mat4 m = Matrices[group];
    m[3][0] = translation.x;
    m[3][1] = translation.y;
    m[3][2] = translation.z;
    Matrices[group] = m;
}

uniform float Time;


uint globalIndex(){
    return uint(gl_GlobalInvocationID.x);

}

uniform int PhysicsPass;
uniform int PhysicsBallCount;
#define PPASS_DETECT 0
#define PPASS_PROCESS 1
#define PPASS_FIX 2

float PhysicsStep = 0;
vec3 predictPosition(uint index){
    return Positions[index].xyz + PhysicsStep * Velocities[index].xyz;
}

vec3 getGravityUniform(vec3 p1, vec3 p2){
    float f = distance(p1, p2);
    return normalize(p2 - p1) * (1.0 / (f*f));
}
float rand2d(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
#include noise4D.glsl
#define BOUNC 0.7
#define ELASTIC 0.5
vec3 fixvelocity = vec3(0);
vec3 getCollisionVelocity(uint a, uint b){
    vec3 a2b = normalize(Positions[a].xyz - Positions[b].xyz);
    vec3 u1 = Velocities[a].xyz;
    vec3 u2 = Velocities[b].xyz;
    float ac1 = dot(u1, a2b);
    float ac2 = dot(u2, a2b);
    
    vec3 after = (u1 + ELASTIC * (ac2 - ac1) * a2b) * BOUNC;
    float indexdelta = float(a - b);
    if(distance(Positions[b].xyz, Positions[a].xyz) < 2.0) {
      //  Pressure[a].xyz = a2b*2;
      //  Pressure[a].a += 0.9 + 0.1* rand2d(vec2(Time, float(b))); 
      fixvelocity += a2b*2 * PhysicsStep;
    }
    
    
    return after;
}

vec3 getDrag(uint index){
    float p =  1.293;
    vec3 u = -clamp(Velocities[index].xyz, -60, 60);
    float Cd = 0.47;
    float A = 0.2;
    return 0.5 * p * u * Cd * A;
}

#define GRIDSIZE (64)
#define IGRIDSIZE (1.0/64)


void UnifiedPhysics(uint index){
    if(PhysicsPass == PPASS_DETECT){
        vec3 collectedCollisionsSum = vec3(0);
        float weight = 0;
        vec3 ppos = predictPosition(index);
        vec3 gravity = vec3(0);
        fixvelocity = vec3(0);
        vec3 pressure = vec3(0);
        float pressureWeight = PhysicsBallCount - 1.0;
        float d = 0;
        for(uint i=index+1;i<PhysicsBallCount;i++){
            d = distance(ppos, predictPosition(i));
            pressure += normalize(Positions[i].xyz - ppos) / (d + 0.01);
            if(d <= 2.0){
                collectedCollisionsSum += getCollisionVelocity(index, i);
                weight += 1.0;
            }
        }
        for(uint i=0;i<index;i++){
            d = distance(ppos, predictPosition(i));
            pressure += normalize(Positions[i].xyz - ppos) / (d + 0.01);
            if(d <= 2.0){
                collectedCollisionsSum += getCollisionVelocity(index, i);
                weight += 1.0;
            }
        }   
        pressure /= pressureWeight;
        if(weight > 0) PathPoints[index] = vec4(collectedCollisionsSum / weight + fixvelocity, 0);
        else PathPoints[index] = vec4(Velocities[index].xyz, 0);
        //Pressure[index] = pressure.xyzz;
        
    }
    else if(PhysicsPass == PPASS_PROCESS){
        vec3 collectedCollisionsSum = PathPoints[index].xyz;
        collectedCollisionsSum += PhysicsStep * getDrag(index);
        collectedCollisionsSum += vec3(0, PhysicsStep * -9.81, 0);
        collectedCollisionsSum = clamp(collectedCollisionsSum, -60, 60) * 0.918;
        //collectedCollisionsSum += (vec3(snoise(vec4(Positions[index].xyz*0.05, Time)), snoise(vec4(Positions[index].yzx*0.05, Time)), snoise(vec4(Positions[index].zxy*0.05, Time))) ) * 1.1;
        //if(Pressure[index].a > 0.0 ) Positions[index].xyz += Pressure[index].xyz;
        Positions[index].xyz += PhysicsStep * PathPoints[index].xyz;
        float xy = 115;
        vec3 boundmin = vec3(-xy, 1, -xy);
        vec3 boundmax = vec3(xy, 199, xy);
        if(Positions[index].x > boundmax.x) {collectedCollisionsSum.x *= -1; collectedCollisionsSum *= BOUNC*ELASTIC;}
        if(Positions[index].x < boundmin.x) {collectedCollisionsSum.x *= -1; collectedCollisionsSum *= BOUNC*ELASTIC;}
        if(Positions[index].y > boundmax.y) {collectedCollisionsSum.y *= -1; collectedCollisionsSum *= BOUNC*ELASTIC;}
        if(Positions[index].y < boundmin.y) {collectedCollisionsSum.y *= -1; collectedCollisionsSum *= BOUNC*ELASTIC;}
        if(Positions[index].z > boundmax.z) {collectedCollisionsSum.z *= -1; collectedCollisionsSum *= BOUNC*ELASTIC;}
        if(Positions[index].z < boundmin.z) {collectedCollisionsSum.z *= -1; collectedCollisionsSum *= BOUNC*ELASTIC;}
        Positions[index].x = clamp(Positions[index].x, boundmin.x, boundmax.x);
        Positions[index].y = clamp(Positions[index].y, boundmin.y, boundmax.y);
        Positions[index].z = clamp(Positions[index].z, boundmin.z, boundmax.z);
        
        
        Velocities[index] = vec4(collectedCollisionsSum, 0.0);
    }

}
void makeSimpleMovement(uint index){
    float inc = float(index) * 0.01;
    vec3 dfield = vec3(0);//(vec3(0, 20, 0) + Positions[index].xyz)* 0.4;
    
    //  dfield += vec3(snoise(vec4(Positions[index].xyz*0.05, Time + inc)), 
    //     snoise(vec4(Positions[index].yzx*0.05, Time + inc)), 
    //      snoise(vec4(Positions[index].zxy*0.05, Time + inc)));
    dfield += vec3(snoise(vec4(Positions[index].xyz*0.005, 0.1*Time + inc)), 
    snoise(vec4(Positions[index].yzx*0.005, 0.7*Time + inc)), 
    snoise(vec4(Positions[index].zxy*0.05, 0.2*Time + inc))) * 3;
    vec3 npos = Positions[index].xyz + dfield * 0.1; 
    
    //npos = clamp(npos, 0.0, 100.0);
    Positions[index] = vec4(npos, 1.0);
}

void main(){
    uint group = globalIndex();
    if(group > Velocities.length()) return;
    PhysicsStep = PathPoints[group].a == 0.0 ? 0.0 : PathPoints[group].a - Time;
    PhysicsStep = 0.26;
    
    PathPoints[group].a = Time;
    UnifiedPhysics(group);
}