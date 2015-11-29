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

//y = (cos(x*pi)+1)/2
float cosmix(float a, float b, float factor){
    return mix(a, b, 1.0 - (cos(factor*3.1415)*0.5+0.5));
}
float ncos(float a){
    return cosmix(0, 1, a);
}

vec3 reconstructCameraSpace(vec2 uv){
	vec4 clip = inverse(ProjectionMatrix * ViewMatrix) * vec4(uv * 2.0 - 1.0, 0.01, 1.0);
	vec3 dir = normalize((clip.xyz / clip.w).xyz - CameraPosition);
	return dir * reverseLog(texture(depthTex, uv).r);
}