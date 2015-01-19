#version 430 core

out vec4 outColor;

uniform vec3 CameraDirection;
in vec2 UV;
const vec2 resolution = vec2(1600, 900);

void main( void ) {

	float ratio = resolution.x / resolution.y;
	vec2 p = ( gl_FragCoord.xy / resolution.xy) + vec2(0, CameraDirection.y / 2 );
	
	vec3 bottom = vec3(0.5, 0.5, 0.5) * 2.0;
	vec3 top = vec3(0.2, 0.4, 0.7);
	
	vec3 sky = mix(top, bottom, 1.0 - p.y);
	
	float size = 0.0555;
	float distan = 0.0;
	float ang = 0.0;
	vec2 pos = vec2(0.0,0.0);
	vec3 SUNNY = vec3(0.5);
	float r = 0.3;
	ang += 3.14;
	pos = vec2((sin(CameraDirection.x * 2) + sin(CameraDirection.z * 2)) ,-CameraDirection.y );
	vec2 correctUV = UV;
	correctUV.x *= ratio;
	distan += size / distance(pos,correctUV);
	vec3 c = vec3(0.055);
	SUNNY = c*distan;
	
	SUNNY *= vec3(0.1, 0.05, 0.01) * 100.0;
	outColor = vec4(sky + SUNNY, 1.0);
}