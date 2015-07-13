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
    vec3 Color;
    float Distance;
    bool HasHit;
};

const IntersectionData emptyIntersection = IntersectionData(vec3(0), vec3(0), vec3(0), INFINITY, false);

vec3 baryCentricMix(vec3 t1, vec3 t2, vec3 t3, vec3 barycentric){
    return 
    vec3(barycentric.x) * t1 + 
    vec3(barycentric.y) * t2 + 
    vec3(barycentric.z) * t3;
}

IntersectionData triangleIntersection(Triangle triangle, vec3 origin, vec3 direction){
    vec3 e0 = triangle.vertices[1].Position.xyz - triangle.vertices[0].Position.xyz;
	vec3 e1 = triangle.vertices[2].Position.xyz - triangle.vertices[0].Position.xyz;

	vec3  h = cross(direction, e1);
	float a = dot(e0, h);
    

	 if(a > -EPSILON && a < EPSILON) 
		return emptyIntersection;

	float f = 1.0 / a;

	vec3  s = origin - triangle.vertices[0].Position.xyz;
	float u = f * dot(s, h);

	if(u < 0.0 || u > 1.0)
		return emptyIntersection;

	vec3  q = cross(s, e0);
	float v = f * dot(direction, q);

	if(v < 0.0 || u + v > 1.0)
		return emptyIntersection;

	float t = f * dot(e1, q);

	if (t < EPSILON)
		return emptyIntersection;
        
    vec3 center = mix(mix(triangle.vertices[0].Position.xyz, triangle.vertices[1].Position.xyz, 0.5), triangle.vertices[2].Position.xyz, 0.5);

    vec3 v1 = triangle.vertices[0].Position.xyz;
    vec3 v2 = triangle.vertices[1].Position.xyz;
    vec3 v3 = triangle.vertices[2].Position.xyz;
    vec3 incidentPosition = v1+(v2-v1)*u+(v3-v1)*v;
    
    vec3 n1 = triangle.vertices[0].Normal.xyz;
    vec3 n2 = triangle.vertices[1].Normal.xyz;
    vec3 n3 = triangle.vertices[2].Normal.xyz;
    vec3 incidentNormal = n1+(n2-n1)*u+(n3-n1)*v;
    
    vec3 c1 = triangle.vertices[0].Albedo.xyz;
    vec3 c2 = triangle.vertices[1].Albedo.xyz;
    vec3 c3 = triangle.vertices[2].Albedo.xyz;
    vec3 incidentColor = triangle.vertices[0].Albedo.xyz;
    
    
	if(t > 0.0 && t < INFINITY){
        return IntersectionData(
            reflect(direction, incidentNormal),
            incidentPosition,
            incidentColor,
            distance(origin, incidentPosition),
            true
        );
    }
}

vec3 determineDirectionFromCamera(vec2 UV){
    mat4 inverted = inverse(ProjectionMatrix * ViewMatrix);
    UV.y = 1.0 - UV.y;
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

vec3 PathTrace2(vec3 origin, vec3 direction){
    IntersectionData bestCandidate = emptyIntersection;
    for(int i=0; i < TrianglesCount; i++){
        IntersectionData inter = triangleIntersection(Triangles[i], origin, direction);
        if(inter.HasHit && inter.Distance <= bestCandidate.Distance){
            bestCandidate = inter;
        }
    }
    return bestCandidate.HasHit ? vec3(bestCandidate.Distance * 0.1) : vec3(1,0,0);
}

float CalculateFallof(float dist){
    return 1.0 / pow(((dist) + 1.0), 2.0);
}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 random3dSample(){
    float randomizer = 138.345341 * rand(iuvToUv(iUV)) * Rand;
    return vec3(
        fract(randomizer) * 2 - 1, 
        fract(randomizer*12.2562) * 2 - 1, 
        fract(randomizer*7.121214) * 2 - 1
    );
}

vec3 PathTrace(vec3 origin, vec3 direction){
    vec3 accumulator = vec3(0);
    vec3 accumulator1 = vec3(0);
    vec3 lastIncidentColor = vec3(1);
    for(int i=0;i<16;i++){
        IntersectionData bestCandidate = emptyIntersection;
        for(int i=0; i < TrianglesCount; i++){
            IntersectionData inter = triangleIntersection(Triangles[i], origin, direction);
            if(inter.HasHit && inter.Distance <= bestCandidate.Distance){
                bestCandidate = inter;
            }
        }
        if(bestCandidate.HasHit){
            vec3 incidentColor = bestCandidate.Color;
           // if(bestCandidate.Color == vec3(1)){
                accumulator += bestCandidate.Color*50 * CalculateFallof(bestCandidate.Distance)*7;
           // }
        }
        
        //lastIncidentColor *= bestCandidate.Color;
        origin = bestCandidate.Origin;
        if(i == 0)accumulator1 = bestCandidate.Color;
        direction = random3dSample();//bestCandidate.NewDirection;
        //direction *= sign(dot(direction, bestCandidate.NewDirection));
        direction = normalize((bestCandidate.NewDirection));
        //if(!bestCandidate.HasHit) break;
    }
    
    return (accumulator);
}

void main(){
    iUV = ivec2(
        gl_GlobalInvocationID.x,
        gl_GlobalInvocationID.y
    );
    vec3 direction = determineDirectionFromCamera(iuvToUv(iUV));
    vec3 color = PathTrace(CameraPosition, direction);
    vec3 lastResult = imageLoad(lastBuffer, iUV).xyz;
    //color = (lastResult * 10 + color) / 11;
    color *= 1.09;
    imageStore(outImage, iUV, vec4(color, 1));
}