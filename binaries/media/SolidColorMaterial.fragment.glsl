#version 430 core

in vec2 LightScreenSpace;
in vec3 positionWorldSpace;
in vec3 normal;

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

uniform vec4 input_Color;
uniform vec3 LightsPos_0;
uniform float LightsFarPlane_0;
uniform float RandomSeed;

out vec4 outColor;

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
	vec3 color = input_Color.xyz;
	if(!shadow){
	
		vec2 fakeUV = LightScreenSpace.xy + hash2x2(LightScreenSpace.xy) * 0.0004;
		float distance1 = texture(texDepth, fakeUV).r;
		float distance2 = distance(positionWorldSpace.xyz, LightsPos_0);
		float logEnchancer = 1.0f;
		float badass_depth = log(logEnchancer*distance2 + 1.0) / log(logEnchancer*LightsFarPlane_0 + 1.0);
		float diff = abs(distance1 -  badass_depth);
		
		if(diff > (0.0008)) color *= 0.2;
		//color = vec3(diff);
	} else {
		color *= 0.2;
	}
	
	float diffuse = clamp(dot(normalize(LightsPos_0), normalize(normal)), 0.2, 1.0);
	
	//outColor = vec4(color, 1.0);
	float alpha = input_Color.a;
	outColor = vec4((color * diffuse).xyz, alpha);
	//outColor = vec4(diff, diff, diff, 1.0);
	
	//float alpha = input_Color.a;
    //outColor = vec4((input_Color * diffuse).xyz, alpha);
	//gl_FragDepth = logz;
}