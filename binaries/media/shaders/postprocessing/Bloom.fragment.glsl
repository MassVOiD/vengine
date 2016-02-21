#version 430 core
uniform int Pass;

in vec2 UV;// let get it done well this time
out vec4 outColor;
#include LightingSamplers.glsl


float kern[] = float[](
	0.02547,
	0.02447,
	0.02351,
	0.02101,
	0.01850,
	0.01550,
	0.01240,
	0.00940,
	0.00708,
	0.00508,
	0.00345,
	0.00205,
	0.00143,
	0.00103,
	0.00051,
	0.00028,
	0.00015,
	0.00009,
	0.00004
);

float cosmix(float a, float b, float factor){
    return mix(a, b, 1.0 - (cos(factor*3.1415)*0.5+0.5));
}
float ncos(float a){
    return cosmix(0, 1, clamp(a, 0.0, 1.0));
}
vec3 sampletex(vec2 uv)
{
	vec3 r = Pass == 0 ? textureMSAA(albedoRoughnessTex, uv, 0).rgb : texture(bloomMidPassTex, uv).rgb;
	//r = r*r*r;
	return r;
}

float getpix()
{
	return Pass == 0 ? (1.0 / txsize.x) : (1.0 / textureSize(bloomMidPassTex, 0).y);
}
#define samples 164
float getkern(float factor){
	return 1.0 - (cos(factor*3.1415)*0.5+0.5);
}
vec3 gauss(){
	vec2 lookup = Pass == 0 ? vec2(1, 0) : vec2(0, 1);
	vec3 accum = vec3(0);
	float pix = getpix();
	float pixx = 0;
	for(int i=0;i<samples;++i) {
		accum += sampletex(UV + pixx * lookup) * getkern(1.0 - float(i)/samples);
		pixx += pix;
	}
	pixx = pix;
	for(int i=1;i<samples;++i) {
		accum += sampletex(UV - pixx * lookup) * getkern(1.0 - float(i)/samples);
		pixx += pix;
	}
	return accum / samples;
}

void main()
{
    outColor = vec4(gauss(), 1.0);
}