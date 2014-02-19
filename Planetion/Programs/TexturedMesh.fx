#version 140

uniform mat4 projection;
uniform mat4 modelView;

in vec3 inVertex;
in vec3 inNormal;
in vec2 inTexCoord;

out vec2 exTexCoord;

void main(void) {
	gl_Position = projection * modelView * vec4(inVertex, 1.);
	exTexCoord = inTexCoord;
}
--------------------------------------------------------------------------------
#version 140

uniform sampler2D DiffuseMap;

in vec2 exTexCoord;

out vec4 color;

void main(void) {
	color = texture(DiffuseMap, exTexCoord);

	if (color.x == 0 && color.y == 0 && color.z == 0 && color.w == 0 && exTexCoord.x == 0 && exTexCoord.y == 0) {
		color = vec4(1, 1, 1, 1);
	}

	color = color;
}