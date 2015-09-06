/*fragment*/
#version 430 core
in vec2 UV;

#include LogDepth.glsl

out vec4 outColor;

void main()
{
	//vec4 color = texture(tex, UV);
	//if(color.a < 0.3) discard;
	float depth = getDepth();
	gl_FragDepth = depth;
	outColor = vec4(0.0, 0.0, 0.0, 0.0);
	//outColor = vec4(color.xyz, color.a - alphaDelta);
}