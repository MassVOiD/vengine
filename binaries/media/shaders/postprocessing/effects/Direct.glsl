struct SimpleLight
{
    vec4 Position;
    vec4 Direction;
    vec4 Color;
    vec4 alignment;
};

layout (std430, binding = 6) buffer SLBf
{
    SimpleLight simpleLights[]; 
}; 


vec3 raymarchFog(vec3 start, vec3 end){
    vec3 color1 = vec3(0);

    //vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;    
    
    bool foundSun = false;
    for(int i=0;i<LightsCount;i++){
    
        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(end, 1.0);
        
        
        float fogDensity = 0.0;
        float fogMultiplier = 111.0;
        vec2 fuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
        vec3 lastPos = start - mix(start, end, 0.01);
        const float stepr = 0.009;
        float samples = 1.0 / stepr;
        float stepsize = distance(start, end) / samples;
		float rd = rand2s(UV);
        for(float m = 0.0; m< 1.0;m+= stepr){
			rd += 1.432135647;
            vec3 pos = mix(start, end, m + fract(rd) * stepsize);
            float distanceMult = stepsize;
            //float distanceMult = 5;
            lastPos = pos;
            float att = CalculateFallof(distance(pos, LightsPos[i]));
            //att = 1;
            lightClipSpace = lightPV * vec4(pos, 1.0);
            
            float fogNoise = 1.0;
    
            vec2 frfuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
            float frfuvz = (lightClipSpace.z / lightClipSpace.w);
            //float idle = 1.0 / 1000.0 * fogNoise * fogMultiplier * distanceMult;
            float idle = 0.0;
            if(lightClipSpace.z < 0.0 || frfuv.x < 0.0 || frfuv.x > 1.0 || frfuv.y < 0.0 || frfuv.y > 1.0){ 
                fogDensity += idle;
                continue;
            }
            float diff =1.0 - (lookupDepthFromLight(i, frfuv, frfuvz));
		
			float culler = 1;//clamp(1.0 - distance(frfuv, vec2(0.5)) * 2.0, 0.0, 1.0);
			//float fogNoise = 1.0;
			fogDensity += diff * (idle + 1.0 / 20.0 * culler * fogNoise * fogMultiplier * att * distanceMult) * smoothstep(0.0, 11.0, distance(pos, LightsPos[i].rgb));
            
        }
        color1 += LightsColors[i].xyz * fogDensity;
        
    }
    return color1;
}

vec3 makeFog(FragmentData data){
    vec3 cspaceEnd = data.cameraPos;
    if(length(cspaceEnd) > 280) cspaceEnd = normalize(cspaceEnd) * 280;
    vec3 fragmentPosWorld3d = data.worldPos;
    return vec3(raymarchFog(CameraPosition, fragmentPosWorld3d));
}



vec3 DirectLight(FragmentData data){
    vec3 color1 = vec3(0);
    
    //float parallax = step(100.0, metalness);
    //metalness = fract(metalness);
	
	float rr = 0.5;
    
    for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(data.worldPos, 1.0);
        if(lightClipSpace.z <= 0.0) continue;
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   

        float percent = 0;
        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0) {
            percent = getShadowPercent(lightScreenSpace, data.worldPos, LightsShadowMapsLayer[i]);
			//percent *= 1.0 - smoothstep(0.4, 0.5, distance(lightScreenSpace, vec2(0.5)));
        }
        vec3 radiance = shade(CameraPosition, data.specularColor, data.normal, data.worldPos, LightsPos[i], LightsColors[i].rgb, data.roughness, false) * (data.roughness);
		vec3 difradiance = shade(CameraPosition, data.diffuseColor, data.normal, data.worldPos, LightsPos[i], LightsColors[i].rgb, 1.0, false) * (data.roughness + 1.0);
        color1 += (radiance + difradiance) * 0.5 * percent;
    }/*
    for(int i=0;i<SimpleLightsCount;i++){
        vec3 pos = simpleLights[i].Position.xyz;
        vec3 n = simpleLights[i].Direction.xyz;
        float angle = cos(simpleLights[i].Color.a);
        float dt = dot(normalize(position - pos), n);
        float factor = smoothstep(angle, 1.0, dt);
        //float cosangle = 
        vec3 col = simpleLights[i].Color.rgb;
        color1 += shade(CameraPosition, albedo, normal, position, pos, col, roughness, false) * AOValue;
    }*/
	
	if(UseFog == 1) color1 += makeFog(data);
    if(DisablePostEffects == 1) color1 *= smoothstep(0.0, 0.1, data.cameraDistance);
    return color1;
}