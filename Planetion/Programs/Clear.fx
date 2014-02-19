#version 140

in vec2 inVertex;

void main(void) {
	gl_Position = vec4(inVertex, 0., 1.);
}
--------------------------------------------------------------------------------
#version 140

out vec4 color;
out vec4 normal;

void main(void) {
	color = vec4(0, 0, 0, 0);
	normal = vec4(0, 0, 0, 0);
}