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

out vec2 UV;
out float time;
out vec4 colorint;
out vec3 normalCoord;
out vec3 vectCoord;
out vec3 campos;


smooth out vec3 pos_eye;
smooth out vec3 n_eye;
smooth out vec3 light_eye;
out vec3 cubetexcoord;

smooth out vec3 positionWorldSpace;
void main(){

    vec4 v = vec4(vertexPosition_modelspace,1); // Transform an homogeneous 4D vector, remember ?
	//gl_Position.y -= 10.0;
	//v+= vec4(cameraPosition, 0.0);
	//v.z += mod(gl_InstanceID, 1024) / 2;
	//v.x+= floor(gl_InstanceID / 1024) / 2;
	//v *= vec4(scale, 1);
	mat4 finalTrans = globalTransformationMatrix * modelMatrix;
	positionWorldSpace = (finalTrans * v).xyz;
	
	vec3 lightPos = vec3(40.0, 0.0, 0.0);
	light_eye = vec3 (viewMatrix * vec4 (lightPos, 1.0));
	
	pos_eye = vec3 (viewMatrix * finalTrans* vec4 (vertexPosition_modelspace, 1.0));
	n_eye = vec3 (viewMatrix * finalTrans * vec4 (normal, 0.0));
	
	
	cubetexcoord = (v).xyz;
	mat4 mvp = projectionMatrix * viewMatrix * finalTrans;
    gl_Position = mvp * (v);
	time = realTime;
	normalCoord = normalize((modelMatrix * vec4(normal, 1)).xyz);
	campos = cameraPosition;
}