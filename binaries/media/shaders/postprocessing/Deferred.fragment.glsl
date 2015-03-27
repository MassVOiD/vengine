#version 430 core

in vec2 UV;
#include Lighting.glsl
#include LogDepth.glsl

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 30) uniform sampler2D worldPosTex;
layout(binding = 31) uniform sampler2D normalsTex;

const int MAX_SIMPLE_LIGHTS = 2000;
uniform int SimpleLightsCount;
uniform vec3 SimpleLightsPos[MAX_SIMPLE_LIGHTS];
uniform vec4 SimpleLightsColors[MAX_SIMPLE_LIGHTS];

out vec4 outColor;

void main()
{
	vec3 colorOriginal = texture(texColor, UV).rgb;
	vec3 color1 = colorOriginal * 0.03;
	gl_FragDepth = texture(texDepth, UV).r;

	vec3 fragmentPosWorld3d = texture(worldPosTex, UV).xyz;
	vec3 normal = texture(normalsTex, UV).xyz;
	
	
	for(int i=0;i<LightsCount;i++){
		float distanceToLight = distance(fragmentPosWorld3d, LightsPos[i]);
		mat4 lightPV = (LightsPs[i] * LightsVs[i]);
		
		
		vec3 lightRelativeToVPos = LightsPos[i] - fragmentPosWorld3d.xyz;
		vec3 cameraRelativeToVPos = CameraPosition - fragmentPosWorld3d.xyz;
		vec3 R = reflect(lightRelativeToVPos, normal);
		float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
		float specularComponent = clamp(pow(cosAlpha, 80.0 / SpecularSize), 0.0, 1.0);
		

		lightRelativeToVPos = LightsPos[i] - fragmentPosWorld3d.xyz;
		float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (normal));
		float diffuseComponent = clamp(dotdiffuse, 0.0, 1.0);
		
		

		// do shadows
		vec4 lightClipSpace = lightPV * vec4(fragmentPosWorld3d, 1.0);
		if(lightClipSpace.z >= 0.0){ 
			vec2 lightScreenSpace = ((lightClipSpace.xyz / lightClipSpace.w).xy + 1.0) / 2.0;	
			if(lightScreenSpace.x > 0.0 && lightScreenSpace.x < 1.0 && lightScreenSpace.y > 0.0 && lightScreenSpace.y < 1.0){ 
				float percent = clamp(getShadowPercent(lightScreenSpace, fragmentPosWorld3d, i), 0.0, 1.0);

				float culler = clamp((1.0 - distance(lightScreenSpace, vec2(0.5)) * 2.0), 0.0, 1.0);
				//color1 = colorOriginal * culler / (distanceToLight / 10.0);
				
				color1 += ((colorOriginal * (diffuseComponent * LightsColors[i].rgb)) 
				+ (LightsColors[i].rgb * specularComponent)) * LightsColors[i].a 
				* culler / (distanceToLight / 10.0) * percent;
				
			}
		}
		
	}
	

	for(int i=0;i<SimpleLightsCount;i++){
		float distanceToLight = distance(fragmentPosWorld3d, SimpleLightsPos[i]);
		
		vec3 lightRelativeToVPos = SimpleLightsPos[i] - fragmentPosWorld3d.xyz;
		vec3 cameraRelativeToVPos = CameraPosition - fragmentPosWorld3d.xyz;
		vec3 R = reflect(lightRelativeToVPos, normal);
		float cosAlpha = -dot(normalize(cameraRelativeToVPos), normalize(R));
		float specularComponent = clamp(pow(cosAlpha, 80.0 / SpecularSize), 0.0, 1.0);
		

		lightRelativeToVPos = SimpleLightsPos[i] - fragmentPosWorld3d.xyz;
		float dotdiffuse = dot(normalize(lightRelativeToVPos), normalize (normal));
		float diffuseComponent = clamp(dotdiffuse, 0.0, 1.0);
		color1 += ((colorOriginal * (diffuseComponent * SimpleLightsColors[i].rgb)) 
				+ (SimpleLightsColors[i].rgb * specularComponent))
			    / (distanceToLight / 10.0);
		
	}	
	
    outColor = vec4(color1, 1);
}