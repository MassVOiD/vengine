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



vec2 projectDL(vec3 pos){
    vec4 tmp = (VPMatrix * vec4(pos, 1.0));
    return (tmp.xy / tmp.w) * 0.5 + 0.5;
}


vec3 makeLightPoint(vec3 point, vec3 color){
	vec2 tc = projectDL(point);
    float rot = (tc.x + tc.y) * 12;
    mat2 RM = mat2(cos(rot), -sin(rot), sin(rot), cos(rot));
	vec3 res = vec3(0);
	vec3 camdir = reconstructCameraSpaceDistance(UV, 1.0);
	vec2 ratiocorrection = vec2(1, ratio);
	float x = distance(UV * ratiocorrection, tc * ratiocorrection);
	vec2 diffvector = RM * (UV * ratiocorrection - tc * ratiocorrection) * 5.0;
	float dim = 1.0 - min(1.0, length(diffvector));
	diffvector = diffvector * 0.5 + 0.5;
	vec3 glarecolor = textureLod(glareTex, clamp(diffvector, 0.0, 1.0), 0).rgb;

	float dst1 = textureMSAA(normalsDistancetex, tc, 0).a;
	dst1 += (1.0 - step(0.0001, dst1)) * 99999.0;
	float mod1 = step(0, dot(point - CameraPosition, camdir));
	float mod2 = mod1 * step(0, dst1 - distance(CameraPosition, point));
	res += glarecolor*1.2 * color * dim * mod2;
	
	return res;
}

#define KERNEL 6
#define PCFEDGE 1
float PCFSun(int i, vec2 uvi, float comparison){

    float shadow = 0.0;
    float pixSize = 1.0 / textureSize(sunCascadesArray,0).x;
    float bound = KERNEL * 0.5 - 0.5;
    bound *= PCFEDGE;
    for (float y = -bound; y <= bound; y += PCFEDGE){
        for (float x = -bound; x <= bound; x += PCFEDGE){
			vec3 uv = vec3(clamp(uvi+ vec2(x,y)* pixSize, 0.0 + pixSize, 1.0 - pixSize), float(i));
            shadow += texture(sunCascadesArray, vec4(uv, comparison));
        }
    }
	return shadow / (KERNEL * KERNEL);
}

vec3 SunLight(FragmentData data){
	int chosenCascade = 0;
	vec2 texcoord = vec2(0);
	float comparison = 0;
	float tolerance = 0.0;
	for(int i = SunCascadeCount - 1; i >= 0; i--){
		vec4 lightClipSpace = (SunMatricesP[i] * SunMatricesV[i]) * vec4(data.worldPos, 1.0);
        vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;   
		float depth = ((lightClipSpace.z / lightClipSpace.w) * 0.5 + 0.5);

        if(lightScreenSpace.x >= 0.0 && lightScreenSpace.x <= 1.0 && lightScreenSpace.y >= 0.0 && lightScreenSpace.y <= 1.0  && depth > 0.0 && depth < 1.0) {
            chosenCascade = i;
			texcoord = lightScreenSpace;
			comparison = depth;
			tolerance = 0.0001 + float(i) * 0.0005;
        }
	}
	float percent = PCFSun(chosenCascade, texcoord, comparison - tolerance);
	//mat4 mat = SunMatrices[chosenCascade];
	vec3 lightPos = data.worldPos - SunDirection;
	
	vec3 radiance = shade(CameraPosition, data.specularColor, data.normal, data.worldPos, lightPos, SunColor, data.roughness, false) * (data.roughness);
	
	vec3 difradiance = shade(CameraPosition, data.diffuseColor, data.normal, data.worldPos, lightPos, SunColor, 1.0, false) * (data.roughness + 1.0);
	
    //return vec3(comparison);
    return (radiance + difradiance) * 0.5 * percent;
}

vec3 DirectLight(FragmentData data){
    vec3 color1 = vec3(0);
    
    //float parallax = step(100.0, metalness);
    //metalness = fract(metalness);
	
	float rr = 0.5;
    
    for(int i=0;i<LightsCount;i++){

        mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(data.worldPos, 1.0);
		color1 += makeLightPoint(LightsPos[i], LightsColors[i].rgb);
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
	
    if(DisablePostEffects == 1) color1 *= smoothstep(0.0, 0.1, data.cameraDistance);
	
	if(SunCascadeCount > 0) color1 += SunLight(data);
	
    return color1;
}