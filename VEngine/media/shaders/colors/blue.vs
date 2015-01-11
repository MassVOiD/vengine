#version 330 core

layout(location = 0) in vec3 vertexPosition_modelspace;
uniform mat4 modelViewProj;

void main(){

    vec4 v = vec4(vertexPosition_modelspace,1); // Transform an homogeneous 4D vector, remember ?
    gl_Position = modelViewProj * v;
}