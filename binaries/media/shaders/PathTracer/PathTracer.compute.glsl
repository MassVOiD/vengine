#version 430 core

layout( local_size_x = 32, local_size_y = 32, local_size_z = 1 ) in;

layout (binding = 0, rgba16f) writeonly uniform image2D outImage;
layout (binding = 1, rgba16f) readonly uniform image2D lastBuffer;

struct Vertex
{
  vec4 Position;
  vec4 Normal;
  vec4 Albedo;
};

struct Triangle
{
  Vertex[3] vertices;
};

layout (std430, binding = 0) buffer MeshData
{
  Triangle Triangles[]; 
}; 

layout (std430, binding = 1) buffer OctreeContainers
{
  int TriangleContainersStream[]; 
}; 

struct OctreeBox
{
    vec4 CenterAndRadius;
    int Children[8];
    int Parent;
    int TriangleContainerIndex;
    int TriangleCount;
    int TriangleCountForLayout;
};

layout (std430, binding = 2) buffer OctreeBoxesBuffer
{
  OctreeBox OctreeBoxes[]; 
}; 
layout (std430, binding = 6) buffer RandomsBuffer
{
  float Randoms[]; 
}; 

uniform int TrianglesCount;
uniform mat4 ProjectionMatrix;
uniform mat4 ViewMatrix;
uniform vec3 CameraPosition;
uniform float Rand;
const float EPSILON  = 1e-6;
const float INFINITY = 1e+4;
ivec2 iUV;

struct IntersectionData{
    vec3 NewDirection;
    vec3 Origin;
    vec3 Normal;
    vec3 Color;
    float Distance;
    bool HasHit;
};

const IntersectionData emptyIntersection = IntersectionData(vec3(0), vec3(0), vec3(0), vec3(0), INFINITY, false);

#include IntersectionTests.glsl


vec3 determineDirectionFromCamera(vec2 UV){
    mat4 inverted = inverse(ProjectionMatrix * ViewMatrix);
    UV.y = 1.0 - UV.y;
    //UV.x = 1.0 - UV.x;
    UV = UV * 2 - 1;
    vec4 reconstructDir = inverted * vec4(UV, 0.01, 1.0);
    reconstructDir.xyz /= reconstructDir.w;
    vec3 dir = normalize(
         CameraPosition - reconstructDir.xyz
    );
    return dir;
}

vec2 iuvToUv(ivec2 uv){
    ivec2 size = imageSize(outImage);
    return vec2(float(uv.x) / float(size.x), float(uv.y) / float(size.y));
}

float CalculateFallof(float dist){
    return 1.0 / pow(((dist) + 1.0), 2.0);
}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float randomizer = 0;
void Seed(vec2 seeder){
    randomizer += 138.345341 * (rand(iuvToUv(iUV)) + rand(seeder)) * Rand;
}

int randsPointer = 0;
uniform int RandomsCount;
float getRand(){
    float r = Randoms[randsPointer];
    randsPointer++;
    if(randsPointer >= RandomsCount) randsPointer = 0;
    return r;
}

vec3 random3dSample(){
    return normalize(vec3(
        getRand() * 2 - 1, 
        getRand() * 2 - 1, 
        getRand() * 2 - 1
    ));
}

uniform int TotalBoxesCount;

IntersectionData getIntersectionOctree(vec3 origin, vec3 direction){
    int boxCursor = 0;
    IntersectionData bestCandidate = emptyIntersection;
    for(int i=0;i<TotalBoxesCount;i++){
        /*if(i==8 && OctreeBoxes[boxCursor].Parent != -1){
            boxCursor = OctreeBoxes[boxCursor].Parent;
            i=0;
            continue;
        } else break;*/
        //if(OctreeBoxes[boxCursor].Children[i] == -1) continue;
        //OctreeBox box = OctreeBoxes[OctreeBoxes[boxCursor].Children[i]];
        OctreeBox box = OctreeBoxes[i];
        
        if(box.TriangleCount > 0 && tryIntersectBox(origin, direction, box.CenterAndRadius.xyz, box.CenterAndRadius.w)){
            int istart = box.TriangleContainerIndex;
            int iend = istart + box.TriangleCount;
            for(int xx = istart; xx < iend; xx++){
                IntersectionData currentHitData = triangleIntersection(Triangles[TriangleContainersStream[xx]], origin, direction);
                
                if(currentHitData.HasHit && currentHitData.Distance <= bestCandidate.Distance){
                    bestCandidate = currentHitData;
                }
            }
            /*IntersectionData currentHitData = IntersectionData(
                vec3(0),
                vec3(0),
                vec3(0),
                vec3(box.CenterAndRadius.xyz * 0.1),
                distance(origin, box.CenterAndRadius.xyz),
                true
            );
            if(currentHitData.HasHit && currentHitData.Distance <= bestCandidate.Distance){
                    bestCandidate = currentHitData;
            }*/
        }
    }
    return bestCandidate;
}

vec3 PathTrace(vec3 origin, vec3 direction){
    vec3 accumulator = vec3(0);
    vec3 accumulator1 = vec3(0);
    vec3 lastIncidentColor = vec3(1);
    for(int i=0;i<1;i++){
        IntersectionData bestCandidate = getIntersectionOctree(origin, direction);
        if(bestCandidate.HasHit){
            vec3 incidentColor = bestCandidate.Color;
            if(length(bestCandidate.Color) > length(vec3(1))){
                if(i > 0)accumulator += bestCandidate.Color*5 * CalculateFallof(bestCandidate.Distance) * max(0,dot(-direction, bestCandidate.Normal)); 
                else accumulator += incidentColor;
                //accumulator += incidentColor * max(0,dot(vec3(0,1,0), bestCandidate.Normal));
            }
        } else if(i == 0)accumulator += vec3(1,0,0);
        
        //lastIncidentColor *= bestCandidate.Color;
        if(i == 0)origin = bestCandidate.Origin;
        if(i == 0)accumulator1 = bestCandidate.Color;
        //Seed(bestCandidate.NewDirection.xy);
        direction = random3dSample();// + bestCandidate.NewDirection;
        direction *= sign(dot(direction, bestCandidate.Normal));
        if(i == 2)direction = mix(direction, bestCandidate.NewDirection, 0.8);
        //direction = normalize(direction);
        //if(!bestCandidate.HasHit) break;
    }
    
    //return (accumulator*3) * accumulator1;
    return accumulator1;
}

vec3 buff(vec3 newcolor){
    vec3 lastResult = imageLoad(lastBuffer, iUV).xyz;
    return length(newcolor) > length(lastResult) ? newcolor : lastResult;
    //return (lastResult * 60 + newcolor) / 61;
}

void main(){
    iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    randsPointer = int((gl_GlobalInvocationID.x + gl_GlobalInvocationID.y*gl_WorkGroupID.x * gl_WorkGroupSize.x)) % RandomsCount;
    vec3 direction = determineDirectionFromCamera(iuvToUv(iUV));
    vec3 color = vec3(0);
    for(int i=0;i<1;i++){
        color += PathTrace(CameraPosition, direction);
    }
    //vec3 lastResult = imageLoad(lastBuffer, iUV).xyz;    
    vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2);
    color.rgb = vec3(pow(color.r, gamma.r),
    pow(color.g, gamma.g),
    pow(color.b, gamma.b));
    //color = buff(color * 1.2)*1;
    //color *= 1.09;
    imageStore(outImage, iUV, vec4(color, 1));
}