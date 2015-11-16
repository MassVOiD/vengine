#version 430 core

in vec2 UV;
#include LogDepth.glsl
#include Lighting.glsl
#include UsefulIncludes.glsl
#include Shade.glsl

#include noise4D.glsl

out vec4 outColor;

vec3 raymarchFog(vec3 start, vec3 end){
	vec3 color1 = vec3(0);

	//vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;	
	
    bool foundSun = false;
	for(int i=0;i<LightsCount;i++){
	
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
        vec4 lightClipSpace = lightPV * vec4(end, 1.0);
		
		
		float fogDensity = 0.0;
		float fogMultiplier = 12.4;
        vec2 fuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
		vec3 lastPos = start - mix(start, end, 0.01);
        float samples = 1.0 / 0.01;
        float stepsize = distance(start, end) / samples;
		for(float m = 0.0; m< 1.0;m+= 0.01){
			vec3 pos = mix(start, end, m);
            float distanceMult = stepsize;
            //float distanceMult = 5;
            lastPos = pos;
			float att = CalculateFallof(distance(pos, LightsPos[i])) * LightsColors[i].a;
			//att = 1;
			lightClipSpace = lightPV * vec4(pos, 1.0);
			
			//float fogNoise = snoise(vec4(pos * 1 + vec3(0.2, -0.1, 0), Time*0.1)) + 1;
            float fogNoise = 1.0;
	
            vec2 frfuv = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;
			//float idle = 1.0 / 1000.0 * fogNoise * fogMultiplier * distanceMult;
			float idle = 0.0;
			if(lightClipSpace.z < 0.0 || frfuv.x < 0.0 || frfuv.x > 1.0 || frfuv.y < 0.0 || frfuv.y > 1.0){ 
				fogDensity += idle;
				continue;
            }
            float badass_depth = toLogDepthEx(distance(pos, LightsPos[i]), LightsFarPlane[i]);
            float diff = (badass_depth - lookupDepthFromLight(i, frfuv));
			if(diff < 0) {
				float culler = 1;//clamp(1.0 - distance(frfuv, vec2(0.5)) * 2.0, 0.0, 1.0);
				//float fogNoise = 1.0;
				fogDensity += idle + 1.0 / 200.0 * culler * fogNoise * fogMultiplier * att * distanceMult;
			} else {
				fogDensity += idle;
			}
		}
		color1 += LightsColors[i].xyz * fogDensity;
		
	}
    vec3 worldPos = FromCameraSpace(texture(worldPosTex, UV).rgb);
	return color1;
}

vec3 makeFog(){
	vec3 cspaceEnd = texture(worldPosTex, UV).xyz;
    if(length(cspaceEnd) > 800) cspaceEnd = normalize(cspaceEnd) * 100;
	vec3 fragmentPosWorld3d = FromCameraSpace(cspaceEnd);
    return clamp(vec3(raymarchFog(CameraPosition, fragmentPosWorld3d)), 0.0, 1.0);
}

void main()
{
    outColor = vec4(makeFog(), texture(depthTex, UV).r);
}