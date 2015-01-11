#version 440
 
layout (location=0) in vec4 coord;
out vec2 texcoord;
uniform vec4 incolor;
out vec4 textColor;
 
void main(void) {
  gl_Position = vec4(coord.xy, 0, 1);
  textColor = incolor;
  texcoord = coord.zw;
}