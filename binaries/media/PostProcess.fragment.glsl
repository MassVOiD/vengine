#version 430 core

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 2) uniform sampler2D vertexPositions;
layout(binding = 3) uniform sampler2D depth2;
layout(binding = 4) uniform sampler2D coneLightVertexPos;
layout(binding = 5) uniform sampler2D coneLightDepth;

uniform mat4 LightsVPs_0;

in vec2 UV;
uniform float Time;
uniform float RandomSeed;

out vec4 outColor;

void main()
{

	vec3 color1 = texture(texColor, UV).rgb;
	vec3 color2 = texture(depth2, UV).rrr;

	if(UV.x > 0.49 && UV.x < 0.51 && abs(UV.y - 0.5) < 0.001) color1 = vec3(0);
	if(UV.y > 0.49 && UV.y < 0.51 && abs(UV.x - 0.5) < 0.001) color1 = vec3(0);
		
    outColor = vec4(color1, 1);
	
}