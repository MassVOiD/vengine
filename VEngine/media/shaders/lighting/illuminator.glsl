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
uniform vec2 mouse;
uniform vec2 resolution;
uniform sampler2D normalMap;

uniform vec3 lightPos;

uniform float time;

uniform mat3 lights[];
/* in format
[ X,  Y,  Z]
[ R,  G,  B]
[iR, iG, iB]
*/

out vec2 UV;
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
	
	
	
	//uint counter =  floor(gl_InstanceID /32);
	
	//v.x += 3.0 * (mod(gl_InstanceID, 128));
	//v.z += 3.0 * gl_InstanceID/128;
	//v.y += gl_InstanceID/(128*128);

	mat4 finalTrans = globalTransformationMatrix * modelMatrix;
	positionWorldSpace = (finalTrans * v).xyz;
	
	vec3 lightPos = vec3(sin(time)*30.0, 10,cos(time)*30.0);

	pos_eye = vec3 (finalTrans*  vec4 (vertexPosition_modelspace, 1));
	light_eye = vec3(0, -20, 0);
	
	final_Trans = finalTrans;
	VM = viewMatrix;
	
	mat4 mvp = projectionMatrix * viewMatrix * finalTrans;
    gl_Position = mvp * v;
	
    UV.x = texcoord.x;
    UV.y = -texcoord.y;
}
