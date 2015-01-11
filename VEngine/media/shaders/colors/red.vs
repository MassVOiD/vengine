#version 430 core
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 normal;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 rotationMatrix;
uniform mat4 globalTransformationMatrix;
uniform vec3 cameraPosition;
uniform float realTime;
uniform sampler2D normalMap;

out vec2 UV;
out float time;
out vec4 colorint;
out vec3 normalCoord;
out vec3 normalFS;
out vec3 vectCoord;
out vec3 campos;


smooth out vec3 pos_eye;
smooth out mat4 final_Trans;
smooth out vec3 light_eye;
out mat4 VM;

smooth out vec3 positionWorldSpace;
void main(){

	normalFS = normal;
    vec4 v = vec4(vertexPosition_modelspace,1); 

	mat4 finalTrans = globalTransformationMatrix * modelMatrix;
	positionWorldSpace = (finalTrans * v).xyz;
	
	vec3 lightPos = vec3(sin(realTime)*30.0,cos(realTime/2.0)*30.0,cos(realTime)*30.0);

	pos_eye = vec3 (finalTrans*  vec4 (vertexPosition_modelspace, 1));
	light_eye = lightPos + pos_eye;
	
	//
	//vec3 normal2 = normal + (texture(normalMap, UV*5.0).xyz * 2.0 - 1.0);
	final_Trans = finalTrans;
	VM = viewMatrix;
	
	
	mat4 mvp = projectionMatrix * viewMatrix * finalTrans;
    gl_Position = mvp * v;
	time = realTime;
	
    UV.x = texcoord.x;
    UV.y = -texcoord.y;
}