#version 430 core

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 3) uniform sampler2D tex;

in vec3 normal;
in vec2 UV;

out vec4 outColor;

uniform vec3 LightsPos_0;
uniform float RandomSeed;
in vec2 LightScreenSpace;
in vec3 positionWorldSpace;


vec2 hash2x2(vec2 co) {
	return vec2(
		fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * RandomSeed * 43758.5453),
		fract(sin(dot(co.yx ,vec2(12.9898,78.233))) * 1/RandomSeed * 43758.5453));
}

void main()
{
	bool shadow = false;
	if(LightScreenSpace.x < 0.0 || LightScreenSpace.x > 1.0) {shadow = true;}
	if(LightScreenSpace.y < 0.0 || LightScreenSpace.y > 1.0) {shadow = true;}
	vec3 color = texture(tex, UV).xyz;
	if(!shadow){
	
		vec2 fakeUV = LightScreenSpace.xy + hash2x2(LightScreenSpace.xy) * 0.0004;
		float distance1 = texture(texDepth, fakeUV).r;
		float distance2 = distance(positionWorldSpace.xyz, LightsPos_0);
		float logEnchancer = 1.0f;
		float badass_depth = log(logEnchancer*distance2 + 1.0) / log(logEnchancer*300.0 + 1.0);
		float diff = abs(distance1 -  badass_depth);
		
		if(diff > (0.0007)) color *= 0.2;
		//color = vec3(diff);
	} else {
		color *= 0.2;
	}
	
	float diffuse = clamp(dot(normalize(vec3(LightsPos_0.x, LightsPos_0.y, -LightsPos_0.z)), normalize(normal)), 0.2, 1.0);
	
	//outColor = vec4(color, 1.0);
	outColor = vec4((color).xyz, 1.0);
	//outColor = vec4(diff, diff, diff, 1.0);
	
	//float alpha = input_Color.a;
    //outColor = vec4((input_Color * diffuse).xyz, alpha);
	//gl_FragDepth = logz;
}