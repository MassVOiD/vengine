#version 430 core

layout( local_size_x = 32, local_size_y = 1, local_size_z = 32 ) in;

layout (binding = 0, rgba16f) writeonly uniform image2D outImage;
layout (binding = 1, rgba16f) readonly uniform image2D lastBuffer;
layout (binding = 2, rgba8) readonly uniform image2D texAlbedo;
layout (binding = 3, rgba16f) readonly uniform image2D texNormals;
layout (binding = 4, rgba16f) readonly uniform image2D texWorldPos;

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

struct PointLight
{
    vec4 PositionAndRadius;
    vec4 ColorAndSamples;
};
uniform int LightsCount;
layout (std430, binding = 7) buffer LightssBuffer
{
  PointLight PointLights[]; 
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
    float Roughness;
    float Distance;
    bool HasHit;
};

const IntersectionData emptyIntersection = IntersectionData(vec3(0), vec3(0), vec3(0), vec3(0), 0, INFINITY, false);

#include IntersectionTests.glsl


vec3 determineDirectionFromCamera(vec2 UV){
    mat4 inverted = inverse(ProjectionMatrix * ViewMatrix);
   // UV.y = 1.0 - UV.y;
    //UV.x = 1.0 - UV.x;
    UV = UV * 2 - 1;
    vec4 reconstructDir = inverted * vec4(UV, 0.01, 1.0);
    reconstructDir.xyz /= reconstructDir.w;
    vec3 dir = -normalize(
         CameraPosition - reconstructDir.xyz
    );
    return dir;
}

vec2 iuvToUv(ivec2 uv){
    ivec2 size = imageSize(outImage);
    return vec2(float(uv.x) / float(size.x), float(uv.y) / float(size.y));
}

#define M_PI 3.1415926535897932384626433832795

float CalculateFallof( float dist){
    //return 1.0 / pow(((dist) + 1.0), 2.0);
    return dist == 0 ? 1 : (3) / (4*M_PI*dist*dist);
    
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
    int foundID = -1;
    float bestDistance = INFINITY;
    for(int i=0;i<TotalBoxesCount;i++){
        /*if(i==8 && OctreeBoxes[boxCursor].Parent != -1){
            boxCursor = OctreeBoxes[boxCursor].Parent;
            i=0;
            continue;
        } else break;*/
        //if(OctreeBoxes[boxCursor].Children[i] == -1) continue;
        //OctreeBox box = OctreeBoxes[OctreeBoxes[boxCursor].Children[i]];
        OctreeBox box = OctreeBoxes[i];
        
        if(dot(direction, box.CenterAndRadius.xyz - origin) > 0 && tryIntersectBox(origin, direction, box.CenterAndRadius.xyz, box.CenterAndRadius.w)){
            int istart = box.TriangleContainerIndex;
            int iend = istart + box.TriangleCount;
            for(int xx = istart; xx < iend; xx++){
                float currentDistance = triangleIntersectionDistance(Triangles[TriangleContainersStream[xx]], origin, direction);
                
                if(currentDistance >= 0 && currentDistance <= bestDistance){
                    bestDistance = currentDistance;
                    foundID = TriangleContainersStream[xx];
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
    return foundID != -1 ? triangleIntersection(Triangles[foundID], origin, direction) : emptyIntersection;
}
IntersectionData getIntersectionRaw(vec3 origin, vec3 direction){
    int boxCursor = 0;
    IntersectionData bestCandidate = emptyIntersection;
    int foundID = -1;
    float bestDistance = INFINITY;
    for(int xx = 0; xx < TrianglesCount; xx++){
        float currentDistance = triangleIntersectionDistance(Triangles[xx], origin, direction);
        
        if(currentDistance >= 0.03 && currentDistance <= bestDistance){
            bestDistance = currentDistance;
            foundID = xx;
        }
    }
    return foundID != -1 ? triangleIntersection(Triangles[foundID], origin, direction) : emptyIntersection;
}

vec3 RandomlyDisplace(vec3 center, float radius){
    return center + (random3dSample() * radius);
}

vec3 TraceShadows(vec3 origin, vec3 normal){
    vec3 accumulatorTotal = vec3(0);
    for(int i=0;i<LightsCount;i++){
        vec3 accumulator = vec3(0);
        float att = CalculateFallof(distance(origin, PointLights[i].PositionAndRadius.xyz));
        for(int s=0;s<int(PointLights[i].ColorAndSamples.w);s++){
            vec3 lpos = RandomlyDisplace(PointLights[i].PositionAndRadius.xyz, PointLights[i].PositionAndRadius.w);
            vec3 direction = normalize(lpos - origin);
            IntersectionData bestCandidate = getIntersectionOctree(origin, direction);
            if( bestCandidate.Distance >= distance(lpos, origin)){
                accumulator += PointLights[i].ColorAndSamples.rgb * max(0,dot(direction, normal)) * att * 10;
            }
        }
        accumulatorTotal += accumulator / PointLights[i].ColorAndSamples.w;
    }
    return accumulatorTotal;
}
vec3 PathTrace(vec3 origin, vec3 direction){
    vec3 accumulator = vec3(0);
    vec3 lastIncidentColor = vec3(1);
    float totalDistance = 0;
    vec3 raycolor = imageLoad(texAlbedo, iUV).xyz;
    float rayStrength = 1;
    for(int i=1;i<4;i++){
        IntersectionData bestCandidate = getIntersectionRaw(origin, direction);
        if(bestCandidate.HasHit){
            totalDistance += bestCandidate.Distance;
            vec3 incidentColor = bestCandidate.Color;
           // raycolor *= 0.5;
            if(length(bestCandidate.Color) >= length(vec3(1))){
                accumulator += CalculateFallof(totalDistance/60+1) * incidentColor*1 * max(0,dot(-direction, bestCandidate.Normal)) *raycolor; 
                //accumulator += incidentColor * max(0,dot(vec3(0,1,0), bestCandidate.Normal));
                break;
            } 
            raycolor *= bestCandidate.Color * max(0,dot(-direction, bestCandidate.Normal));
               // incidentColor = TraceShadows(bestCandidate.Origin, bestCandidate.Normal);
               // if(i > 0)accumulator += CalculateFallof(totalDistance + 5.0) * max(0,dot(-direction, bestCandidate.Normal)) / i; 
               //rayStrength *= 1 * max(0,dot(-direction, bestCandidate.Normal));
           // }
        } else break;
        
        //lastIncidentColor *= bestCandidate.Color;
        origin = bestCandidate.Origin;
        //Seed(bestCandidate.NewDirection.xy);
        direction = random3dSample();// + bestCandidate.NewDirection;
        direction *= sign(dot(direction, bestCandidate.NewDirection));
        direction = normalize(mix(direction, bestCandidate.NewDirection, 1));
        //direction.y = abs(direction.y);
        direction = direction;
        //direction = normalize(direction);
        //if(!bestCandidate.HasHit) break;
        //if(i==0)accumulator += TraceShadows(origin, bestCandidate.Normal);
    }
   // accumulator*=0.2;
    return (accumulator);
    //return accumulator1;
}
#define CST 1.78107241799019798524
#include UsefulIncludes.glsl
vec3 ExecuteTracing(){
    vec3 albedo = imageLoad(texAlbedo, iUV).xyz;
    float albedoA = imageLoad(texAlbedo, iUV).a;
    vec3 worldPos = FromCameraSpace(imageLoad(texWorldPos, iUV).xyz);
    vec3 normal = imageLoad(texNormals, iUV).xyz;
    vec3 vwpos = normalize(worldPos - CameraPosition);
    float roughness = 1.0-imageLoad(texWorldPos, iUV).a;
    vec3 reflected = reflect(vwpos , normal);
    vec3 c = vec3(0);
    int count = 0;
    if(albedo != vec3(0) && albedoA != 0){
        for(int i=1;i<5;i++){
            randsPointer = int(getRand() + (gl_GlobalInvocationID.x + (gl_GlobalInvocationID.z + 1)*(gl_WorkGroupID.z + 1) * gl_WorkGroupSize.x))*i % RandomsCount;
            vec3 direction = random3dSample();// + bestCandidate.NewDirection;
            //direction *= sign(dot(direction, normal));
           // direction = mix(direction, bestCandidate.NewDirection, 0.8);
            direction *= sign(dot(direction, normal));
            direction = normalize(mix(direction, reflected, 1));
            c += PathTrace(worldPos, direction);
            count++;
            //c += TraceShadows(worldPos, normal);
        }
    }
    if(count == 0) return albedo *max(0, dot(reflected, normal));
    c = c/count;
   // if(length(albedo) >= length(vec3(1))) return vec3(1);
    return worldPos.y > -9 ? (c) : vec3(0);
}
    
vec3 buff(vec3 newcolor){
    vec3 lastResult = imageLoad(lastBuffer, iUV).xyz;
 //  return length(newcolor) > length(lastResult) ? newcolor : lastResult;
    return (lastResult * 15 + newcolor ) / 16;
}

uniform int RenderOffsetX;
uniform int RenderOffsetY;
void main(){
    iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.z
    ) + ivec2(RenderOffsetX, RenderOffsetY);
    
    vec3 color = vec3(0);
    vec3 lastResult = imageLoad(lastBuffer, iUV).xyz;
    int iter = 0;
   /* while(length(lastResult) > 0.1 && iter < 512){
        iUV = ivec2(
            getRand() * imageSize(lastBuffer).x,
            getRand() * imageSize(lastBuffer).y
        );
        lastResult = imageLoad(lastBuffer, iUV).xyz;
        iter++;
    } */
    //vec3 direction = determineDirectionFromCamera(iuvToUv(iUV));

    color += ExecuteTracing();
    
    
    //color *= 0.08;
    //color = vec3(0);
    //vec3 lastResult = imageLoad(lastBuffer, iUV).xyz;    
    vec3 gamma = vec3(1.0/2.2, 1.0/2.2, 1.0/2.2);
    color.rgb = vec3(pow(color.r, gamma.r),
    pow(color.g, gamma.g),
    pow(color.b, gamma.b));
    //color = buff(color)*1;
   // color *= 0;
    /*if(iUV.x < imageSize(lastBuffer).x/2 && iUV.y < imageSize(lastBuffer).y/2){
        float ix = float(iUV.x) * 4;
        float iy = float(iUV.y) * 4;
        ivec2 newi = iUV*2;
        vec3 c = imageLoad(texAlbedo, newi).rgb;
        color = c;
    }*/
    imageStore(outImage, iUV, vec4(clamp(color, 0, 1), 1));
}