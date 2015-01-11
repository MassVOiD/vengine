#version 440
 
in vec2 texcoord;
uniform sampler2D tex;
out vec4 color;
in vec4 textColor;
 
 
vec4 blur(vec4 colour, vec2 tc)
{
	float samples =1.0; 
	float quality = 0.02;
	vec4 sum = colour;
	float diff = (samples) / 2.0;
	vec4 subcolor = vec4(-0.1) ;
	vec2 sizeFactor = vec2(1.0, 1.0) * quality;
	int count = 0;
	for (float x = -diff; x <= diff; x += 0.2)
	{
		for (float y = -diff; y <= diff; y += 0.2)
		{
			vec2 offset = vec2(x,y) * 0.02;
			sum += vec4(1.0, 1.0, 1.0, texture2D(tex, tc + offset).r) * textColor;
			count++;

		}
	}
	vec4 final = (sum) / count;
	
	return  normalize(final);
}
 
void main(void) {
  color = vec4(1.0, 1.0, 1.0, texture2D(tex, texcoord).r) * textColor;
 // color = color +  blur(color, texcoord);
}