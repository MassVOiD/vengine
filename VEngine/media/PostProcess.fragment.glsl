#version 430 core

layout(binding = 0) uniform sampler2D texColor;
layout(binding = 1) uniform sampler2D texDepth;
layout(binding = 2) uniform sampler2D normalTexColor;
layout(binding = 3) uniform sampler2D normalTexDepth;

in vec2 UV;
uniform float Time;
uniform float RandomSeed;

out vec4 outColor;

float hash1(float uv) {
	return fract(sin(uv * 15.78 * RandomSeed) * 43758.23);
}
vec2 hash2x2(vec2 uv) {
	float x = fract(sin(uv.x * 17.765 * RandomSeed + uv.y * 45.234 * RandomSeed) * 43758.1223);
	float y = fract(sin(uv.y * 75.923 * RandomSeed + uv.y * 12.882 *-RandomSeed) * 73758.9783);
	return vec2(x, y);
}
float rgb2h(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = c.g < c.b ? vec4(c.bg, K.wz) : vec4(c.gb, K.xy);
    vec4 q = c.r < p.x ? vec4(p.xyw, c.r) : vec4(c.r, p.yzx);

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return abs(q.z + (q.w - q.y) / (6.0 * d + e));
}

void main()
{
	float depth = texture(texDepth, UV).r;
	vec3 normal = texture(normalTexColor, UV).rgb;
	vec3 color = texture(texColor, UV).rgb;
	//vec3 color = vec3(0,0,0);
	
	float light = 0.0;
	
	if(depth < 1.0) for (float i = 0; i < 12; i += 1.0)
	{
		vec2 shift = normalize(hash2x2(UV * hash1(i))) * (i/12.0);
		shift.x = shift.x * 2.0 - 1.0;
		shift.y = shift.y * 2.0 - 1.0;
		vec2 point = UV + shift * 0.0094;
		vec3 depthHere = texture(normalTexColor, point).rgb;
		float d = texture(texDepth, point).r;
		float delta = abs(rgb2h(normal) - rgb2h(depthHere));
		float delta2 = d - depth;
		if(delta2 < 0.001 && delta > 0.1 && delta < 0.5) light += delta / 8.0f;
	}
	color -= light;
    outColor = vec4(color, 1.0);
	
}