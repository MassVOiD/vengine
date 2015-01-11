#version 430 core
out vec4 outColor;
uniform sampler2D tex;

in float time;

void main( void ) {
	vec2 resolution = vec2(1600, 900);
	vec2 position = (( gl_FragCoord.xy / resolution.xy )*2.0 - 1.0);
	vec2 uv = gl_FragCoord.xy / resolution;
	position.x *= resolution.x / resolution.y;
	vec3 color = vec3(0.0);
	
	float speed = 2.0;

	color += vec3(0.25, 1.0, 0.25) * (1.0 / distance(
		vec2((cos(time*speed) + cos(time*speed * 4.)) * 0.5, (sin(time*speed) + sin(time*speed * 4.)) * 0.5)
		, position) * 0.05);
	
	
	color += vec3(0.25, 0.5, 1.0) * (1.0 / distance(
		vec2((sin(time*speed) + sin(time*speed * 4.)) * 0.5, (cos(time*speed) + cos(time*speed * 4.)) * 0.5)
		, position) * 0.05);
	
	color += vec3(1.0, 0.25, 1.0) * (1.0 / distance(
		vec2((sin(time*speed) + sin(time*speed * 4.)) * -0.5, (cos(time*speed) + cos(time*speed * 4.)) * -0.5)
		, position) * 0.05);
	
	outColor =vec4(color, 0.3);

}