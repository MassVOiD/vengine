#version 430 core
out vec3 outColor;
uniform sampler2D tex;
in vec2 cubetexcoord;


vec3 effect(vec3 colour, vec2 tc)
{
	float samples = 2.0; 
	float quality = 0.02;
	vec3 sum = vec3(1.0);
	float diff = (samples - (samples/2));
	vec3 subcolor =vec3(-0.1) ;
	vec2 sizeFactor = vec2(1.0, 1.0) * quality;
	int count = 0;
	for (float x = -diff; x <= diff; x += 0.1)
	{
		for (float y = -diff; y <= diff; y += 0.1)
		{
			vec2 offset = vec2(x,y) * sizeFactor;
			sum += texture(tex, tc + offset).xyz;
			count++;

		}
	}
	vec3 final = (sum) / count;
	return  (final* final* final - vec3(0.5, 0.5, 0.5)) * 2;
}

vec3 blur(vec3 colour, vec2 tc)
{
	float samples =2.0; 
	float quality = 0.02;
	vec3 sum = colour;
	float diff = (samples) / 2;
	vec3 subcolor =vec3(-0.1) ;
	vec2 sizeFactor = vec2(1.0, 1.0) * quality;
	int count = 0;
	for (float x = -diff; x <= diff; x += 0.2)
	{
		for (float y = -diff; y <= diff; y += 0.2)
		{
			vec2 offset = vec2(x,y) * 0.02;
			sum += texture(tex, tc + offset).xyz;
			count++;

		}
	}
	vec3 final = (sum) / count;
	final *= pow(length(final), 4.0);
	final -= vec3(1.9);
	
	return  normalize(final);
}

vec3 hdr(vec3 colour)
{
	return colour;
}

void main()
{

	vec3 basecolor = texture(tex, cubetexcoord).xyz;
	//basecolor = basecolor + effect(basecolor, cubetexcoord) / 2;
	outColor = basecolor;
	//outColor =  normalize(basecolor + blur(basecolor, cubetexcoord));
	//outColor = hdr(basecolor);
}