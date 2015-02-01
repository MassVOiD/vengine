#version 430 core

in vec2 LightScreenSpace;
in vec3 positionWorldSpace;
in vec3 normal;

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;

uniform vec4 input_Color;

out vec4 outColor;


void main()
{
	bool shadow = false;
	if(LightScreenSpace.x < 0.0 || LightScreenSpace.x > 1.0) shadow = true;
	if(LightScreenSpace.y < 0.0 || LightScreenSpace.y > 1.0) shadow = true;
	vec3 color = input_Color.xyz;
	if(!shadow){
		vec2 fakeUV = LightScreenSpace.xy;
		float distance1 = texture(texColor, fakeUV).a;
		//float distance2 = distance(positionWorldSpace.xyz, texture(texColor, fakeUV).rgb);
		float diff = abs(distance1);
		if(diff > 19999999.0f) color *= 0.2;
	} else color *= 0.2;
	float diffuse = clamp(dot(vec3(1.0, 1.0, 1.0), normalize(normal)), 0.2, 1.0);
	
	//outColor = vec4(color, 1.0);
	float alpha = input_Color.a;
	outColor = vec4((color * diffuse).xyz, alpha);
	//outColor = vec4(diff, diff, diff, 1.0);
	
	//float alpha = input_Color.a;
    //outColor = vec4((input_Color * diffuse).xyz, alpha);
	//gl_FragDepth = logz;
}