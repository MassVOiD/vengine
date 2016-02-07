
vec3 getLightAtPoint(vec3 point){
	vec3 rad = vec3(0);
	for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(point, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

        float percent = 0;
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) {
            percent = getShadowPercent(lightScreenSpace, point, LightsShadowMapsLayer[i]);
			percent *= 1.0 - smoothstep(0.4, 0.5, distance(lightScreenSpace, vec2(0.5)));
        }
		float dist = distance(LightsPos[i], point);
        rad += percent * LightsColors[i].rgb * (1.0 / dist*dist);
    }
	return rad;
}

struct VoxelContainer {
	vec4 minimum;
	vec4 maximum;
	int voxelsIndex;
	int alignment1;
	int alignment2;
	int alignment3;
	int voxelsCount;
	int alignment11;
	int alignment22;
	int alignment33;
};

struct Voxel {
	vec4 minimum;
	vec4 maximum;
	vec4 albedo;
	vec4 normal;
};

layout (std430, binding = 3) buffer ContainersBuffer
{
    VoxelContainer AllContainers[]; 
}; 

layout (std430, binding = 4) buffer VoxelsBuffer
{
    Voxel AllVoxels[]; 
}; 

layout (std430, binding = 5) buffer VoxelsResultsBuffer
{
    vec4 AllResultsVoxels[]; 
}; 

uniform int ContainersCount;
uniform int VoxelsCount;
uniform int BaseVoxel;

vec3 correctZero(vec3 v){
	v.x = abs(v.x) < 0.01 ? 0.01 : v.x;
	v.y = abs(v.y) < 0.01 ? 0.01 : v.y;
	v.z = abs(v.z) < 0.01 ? 0.01 : v.z;
	return v;
}

float hitDistance = 99999.0;
bool tryIntersectBox(vec3 centerR, vec3 directionR, vec3 bMin, vec3 bMax)
{
	vec3 d = normalize(correctZero(directionR));
    vec3 tMin = (bMin - centerR) / d;
    vec3 tMax = (bMax - centerR) / d;
    vec3 t1 = min(tMin, tMax);
    vec3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    return tFar > tNear ;
}
bool tryIntersectBoxDistance(vec3 centerR, vec3 directionR, vec3 bMin, vec3 bMax, out float hit)
{
	vec3 d = normalize(correctZero(directionR));
    vec3 tMin = (bMin - centerR) / d;
    vec3 tMax = (bMax - centerR) / d;
    vec3 t1 = min(tMin, tMax);
    vec3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
	hit = tNear;
    return tFar > tNear && tNear > 1.01;
}

bool coneTrace(vec3 centerR, vec3 directionR, float coneangle, vec3 bMin, vec3 bMax, out float hit)
{
// god damn :)
	vec3 center = (bMax + bMin) * 0.5;
	float size  = distance(bMax, bMin);
	
	vec3 originToVoxel = normalize(center - centerR);
	float directionDifference = 1.0 - max(0, dot(originToVoxel, directionR));
	
	hit = distance(center, centerR);
    return directionDifference < coneangle;
}
float coneTraceImportance(vec3 centerR, vec3 directionR, vec3 bMin, vec3 bMax)
{
// god damn :)
	vec3 center = (bMax + bMin) * 0.5;
	float size  = distance(bMax, bMin);
	float dst  = distance(center, centerR);
	
	vec3 originToVoxel = normalize(center - centerR);
	float directionDifference = max(0, dot(originToVoxel, directionR));
		
    return directionDifference ;
}

#define SAMPLES 1

float randSeedGlobal;
float randSeedLocal;
float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 randomDirection(){
	vec3 res = vec3(0);
	res.x = rand(vec2(randSeedGlobal, randSeedLocal));
	randSeedLocal += 1.234;
	res.y = rand(vec2(randSeedGlobal, randSeedLocal));
	randSeedLocal += 1.234;
	res.z = rand(vec2(randSeedGlobal, randSeedLocal));
	randSeedLocal += 1.234;
	return normalize(res * 2.0 - 1.0);
}

vec3 traceRay(vec3 center, vec3 direction){
	int hitVoxelIndex = -1;
	float minDistance = 99999.0;
	hitDistance = 999999.0;
	vec3 res = vec3(0);
    for(int ci=0;ci<AllContainers.length();ci++){
	
		VoxelContainer container = AllContainers[ci];
		float zds = 0;
		if(tryIntersectBox(center, direction, container.minimum.xyz, container.maximum.xyz)){
		
			for(int vi=container.voxelsIndex;vi<container.voxelsIndex + container.voxelsCount;vi++){
			
				Voxel voxel = AllVoxels[vi];
				//float importance = coneTraceImportance(center, direction, voxel.minimum.xyz, voxel.maximum.xyz);
				//res += voxel.albedo.xyz * importance;
				
				float hh = 999999;
				if(tryIntersectBoxDistance(center, direction, voxel.minimum.xyz, voxel.maximum.xyz, hh)){
					if(hh < minDistance){
						hitVoxelIndex = vi;
						minDistance = hh;
					}
				}
			
				
			}
			
		}
		
	}
	vec3 result = vec3(0);
	if(hitVoxelIndex >= 0){
		vec3 center = (AllVoxels[hitVoxelIndex].minimum.xyz + AllVoxels[hitVoxelIndex].maximum.xyz) * 0.5 +AllVoxels[hitVoxelIndex].normal.xyz ;
		result = getLightAtPoint(center);
	}
	return result;
}
vec3 traceRayCone(vec3 center, vec3 direction){
	int hitVoxelIndex = -1;
	float minDistance = 99999.0;
	hitDistance = 999999.0;
	vec3 res = vec3(0);
	for(int ci=0;ci<AllContainers.length();ci++){
	
		VoxelContainer container = AllContainers[ci];
		if(coneTraceImportance(center, direction, container.minimum.xyz, container.maximum.xyz) > 0.8)
		for(int vi=container.voxelsIndex;vi<container.voxelsIndex + container.voxelsCount;vi++){
		
			Voxel voxel = AllVoxels[vi];
			float importance = coneTraceImportance(center, direction, voxel.minimum.xyz, voxel.maximum.xyz);
			vec3 c = (AllVoxels[vi].minimum.xyz + AllVoxels[vi].maximum.xyz) * 0.5 +AllVoxels[vi].normal.xyz ;
			float dst = distance(c, center);
			res += getLightAtPoint(c) * importance*0.01 * (1.0 / (1.0 + dst*dst));
				
		}
	}
	return res;
}
