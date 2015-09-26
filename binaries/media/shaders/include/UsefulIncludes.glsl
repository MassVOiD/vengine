vec3 ToCameraSpace(vec3 position){
    return position + -CameraPosition;
}

vec3 FromCameraSpace(vec3 position){
    return position - -CameraPosition;
}

vec3 ProjectPointOnLine(vec3 p, vec3 a, vec3 b){
    vec3 u = normalize(b - a);
    return dot(u, p - a) * u + a;
}

struct RSMLight
{
    vec4 Position;
    vec4 normal;
    vec4 Color;
};

layout (std430, binding = 9) buffer RSMBuffer
{
    RSMLight rsmLights[64*64]; 
}; 