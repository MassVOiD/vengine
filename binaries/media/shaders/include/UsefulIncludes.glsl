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


//y = (cos(x*pi)+1)/2
float cosmix(float a, float b, float factor){
    return mix(a, b, 1.0 - (cos(factor*3.1415)*0.5+0.5));
}
float ncos(float a){
    return cosmix(0, 1, clamp(a, 0.0, 1.0));
}
uniform vec3 FrustumConeLeftBottom;
uniform vec3 FrustumConeBottomLeftToBottomRight;
uniform vec3 FrustumConeBottomLeftToTopLeft;
//mat4 imvp =inverse(ProjectionMatrix * ViewMatrix);
vec3 reconstructCameraSpaceFull(vec2 uv){
    vec3 dir = normalize((FrustumConeLeftBottom + FrustumConeBottomLeftToBottomRight * uv.x + FrustumConeBottomLeftToTopLeft * uv.y));
    return dir * texture(distanceTex, uv).r;
}
vec3 reconstructCameraSpace(vec2 uv, int samplee){
    vec3 dir = normalize((FrustumConeLeftBottom + FrustumConeBottomLeftToBottomRight * uv.x + FrustumConeBottomLeftToTopLeft * uv.y));
    return dir * texture(distanceTex, uv).r;
}
vec3 getTangentPlane(vec3 inp){
    return normalize(cross(inp.xzy,inp));    
}


float toLogDepthEx(float depth, float far){
	//float badass_depth = log(LogEnchacer*depth + 1.0f) / log(LogEnchacer*far + 1.0f);
    float badass_depth = log2(max(1e-6, 1.0 + depth)) / (log2(far));
    //float badass_depth = log2(1.0 + depth) / log2(far+1.0);
	return badass_depth;
}
float toLogDepth(float depth){
	return toLogDepthEx(depth, FarPlane);
}
float reverseLogEx(float dd, float far){
	//return pow(2, dd * log2(far+1.0) ) - 1;
	return pow(2, dd * log2(far)) - 1.0;
}
float reverseLog(float dd){
	return reverseLogEx(dd, FarPlane);
}