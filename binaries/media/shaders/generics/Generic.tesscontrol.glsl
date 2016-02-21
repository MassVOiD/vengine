#version 430 core
#include Mesh3dUniforms.glsl

// define the number of CPs in the output patch
layout (vertices = 3) out;

uniform vec3 gEyeWorldPos;
in Data {
#include InOutStageLayout.glsl
} Input[];
out Data {
#include InOutStageLayout.glsl
} Output[];

uniform int MaterialType;

#define MaterialTypeSolid 0
#define MaterialTypeRandomlyDisplaced 1
#define MaterialTypeWater 2
#define MaterialTypeSky 3
#define MaterialTypeWetDrops 4
#define MaterialTypeGrass 5
#define MaterialTypePlanetSurface 6
#define MaterialTypeTessellatedTerrain 7
uniform int UseBumpMap;

float GetTessLevel(float Distance0, float Distance1)
{
    float rd = ((Distance0 +Distance1)/2);
    if(rd < 150.0) 
        return 33.0;
    else if(rd < 280.0) 
        return 8.0;
    return 4.0;
}
float GetTessLevelAlternative(float Distance0, float Distance1, float surfaceSize)
{
    float x = surfaceSize;
    float rd = ((Distance0 +Distance1)/2);
   // if(rd > 100) return 2;
    return 21.0;// / pow(1.2, (((Distance0 +Distance1)*0.02)+1));
}

uniform float TesselationMultiplier;

vec2 ss(vec3 pos){
	vec4 tmp = (VPMatrix * vec4(pos, 1.0));
	return tmp.xy / tmp.w;
}

float surfacess(vec3 p1, vec3 p2, vec3 p3){
	vec2 a = ss(p1);
    vec2 b = ss(p2);
    vec2 c = ss(p3);
    vec2 hp = mix(a, b, 0.5);
    float h = distance(hp, c);
    float p = distance(a, b);
    return 0.5 * p * h;
}

void main()
{
    // Set the control points of the output patch
    Output[gl_InvocationID].TexCoord = Input[gl_InvocationID].TexCoord;
    Output[gl_InvocationID].Normal = Input[gl_InvocationID].Normal;
    Output[gl_InvocationID].WorldPos = Input[gl_InvocationID].WorldPos;
   // Output[gl_InvocationID].Barycentric = Barycentric_CS_in[gl_InvocationID];
    Output[gl_InvocationID].Tangent = Input[gl_InvocationID].Tangent;
    Output[gl_InvocationID].Data = Input[gl_InvocationID].Data;
    Output[gl_InvocationID].instanceId = Input[gl_InvocationID].instanceId;
       //Barycentric_ES_in = Output[0].Barycentric;
    
    // Calculate the distance from the camera to the three control points
    float EyeToVertexDistance0 = distance(CameraPosition, Input[0].WorldPos);
    float EyeToVertexDistance1 = distance(CameraPosition, Input[1].WorldPos);
    float EyeToVertexDistance2 = distance(CameraPosition, Input[2].WorldPos);
    float EyeToVertexDistanceMin1 = min(EyeToVertexDistance0, EyeToVertexDistance1);
    float EyeToVertexDistanceMin2 = min(EyeToVertexDistance1, EyeToVertexDistance2);
    
    /*float surfaceSize = (distance(WorldPos_CS_in[0], WorldPos_CS_in[1]) +
        distance(WorldPos_CS_in[1], WorldPos_CS_in[2]) + 
        distance(WorldPos_CS_in[2], WorldPos_CS_in[0])) * 0.33;*/
       
	float ssarea = surfacess(Input[0].WorldPos, Input[1].WorldPos, Input[2].WorldPos);
	float tesslevel = clamp(ssarea * 1256, 1, 32);
	Output[gl_InvocationID].Data.x = tesslevel;

	gl_TessLevelOuter[0] = tesslevel;
	gl_TessLevelOuter[1] = tesslevel;
	gl_TessLevelOuter[2] = tesslevel;
	gl_TessLevelInner[0] = gl_TessLevelOuter[0];
	gl_TessLevelInner[1] = gl_TessLevelOuter[0];

}