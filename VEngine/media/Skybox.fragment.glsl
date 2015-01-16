#version 430 core
out vec4 outColor;
in float time;

void main( void ) {
	vec2 resolution = vec2(1600, 900);
	vec2 position = (( gl_FragCoord.xy / resolution.xy )*2.0 - 1.0);
	vec2 uv = gl_FragCoord.xy / resolution;
	position.x *= resolution.x / resolution.y;
	vec3 color = vec3(0.0);
	
	float speed = 2.0;

	color += vec3(0.15, 0.4, 0.35) * (1.0 / distance(
		vec2(1.0, 0.0)
		, position) * 0.15);
	
	// color small dot
	color += vec3(0.15, 0.15, 0.15) * (1.0 / distance(
		vec2(sin(time*9.0) / 6.0 + 1.0, cos(time*9.0) / 6.0)
		, position) * 0.09);
	
	outColor = vec4(1.0, 0.0, 1.0, 1.0);

}