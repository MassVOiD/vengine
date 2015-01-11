#version 330 core
in vec2 UV;
in float time;
layout(location = 0) out vec4 outColor;
uniform sampler2D tex;
in vec3 normalCoord;	
in vec3 vectCoord;	 
in mat4 VM;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 rotationMatrix;
uniform mat4 globalTransformationMatrix;
uniform vec3 cameraPosition;
in vec3 campos;

smooth in vec3 positionWorldSpace;
smooth in vec3 pos_eye;
smooth in mat4 final_Trans;
smooth in vec3 light_eye;

smooth in vec3 normalFS;

uniform sampler2D normalMap;

void main()
{
	outColor = vec4(1.0, 1.0, 1.0, 1.0);

}