
bool tryIntersectBox(vec3 origin, vec3 direction,
                    vec3 center,
                    float radius)
{

    vec3 bMin = center - vec3(radius);
    vec3 bMax = center + vec3(radius);
    vec3 tMin = (bMin - origin) / direction;
    vec3 tMax = (bMax - origin) / direction;
    vec3 t1 = min(tMin, tMax);
    vec3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    return tFar > tNear && tNear > - radius*2;
}

float triangleIntersectionDistance(Triangle triangle, vec3 origin, vec3 direction){
    vec3 e0 = triangle.vertices[1].Position.xyz - triangle.vertices[0].Position.xyz;
	vec3 e1 = triangle.vertices[2].Position.xyz - triangle.vertices[0].Position.xyz;

	vec3  h = cross(direction, e1);
	float a = dot(e0, h);
    
	float f = 1.0 / a;

	vec3  s = origin - triangle.vertices[0].Position.xyz;
	float u = f * dot(s, h);

	vec3  q = cross(s, e0);
	float v = f * dot(direction, q);

	float t = f * dot(e1, q);
            
    vec3 incidentPosition = triangle.vertices[0].Position.xyz+(triangle.vertices[1].Position.xyz-triangle.vertices[0].Position.xyz)*u+(triangle.vertices[2].Position.xyz-triangle.vertices[0].Position.xyz)*v;

    return t > 0.0 && t < INFINITY && 
       (a <= -EPSILON || a >= EPSILON) && 
        u >= 0.0 && u <= 1.0 && 
        v >= 0.0 && u + v <= 1.0 && 
        t >= EPSILON ? distance(origin, incidentPosition) : -1;

}

IntersectionData triangleIntersection(Triangle triangle, vec3 origin, vec3 direction){
    vec3 e0 = triangle.vertices[1].Position.xyz - triangle.vertices[0].Position.xyz;
	vec3 e1 = triangle.vertices[2].Position.xyz - triangle.vertices[0].Position.xyz;

	vec3  h = cross(direction, e1);
	float a = dot(e0, h);
    
	float f = 1.0 / a;

	vec3  s = origin - triangle.vertices[0].Position.xyz;
	float u = f * dot(s, h);

	vec3  q = cross(s, e0);
	float v = f * dot(direction, q);

	float t = f * dot(e1, q);
        
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
    

    return IntersectionData(
        reflect(direction, incidentNormal),
        incidentPosition,
        incidentNormal,
        incidentColor,
        triangle.vertices[0].Albedo.a,
        distance(origin, incidentPosition),
        t > 0.0 && t < INFINITY && 
        (a <= -EPSILON || a >= EPSILON) && 
        u >= 0.0 && u <= 1.0 && 
        v >= 0.0 && u + v <= 1.0 && 
        t >= EPSILON
    );

}
