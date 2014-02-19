#version 150

uniform mat4 projection;
uniform mat4 modelView;
uniform mat4 model;

in vec3 inVertex;
in vec3 inNormal;
in vec2 inTexCoord;

out vec2 exTexCoord;
smooth out vec3 exNormal;
out vec3 exPosition;

void main(void) {
	gl_Position =  projection * modelView * vec4(inVertex, 1);
	exTexCoord = inTexCoord;
	exNormal = normalize((transpose(inverse(modelView)) * vec4(inNormal, 0)).xyz);
	//exNormal = normalize(inNormal);

	exPosition = (model * vec4(inVertex, 1)).xyz;
}
--------------------------------------------------------------------------------
#version 150

uniform sampler2D DiffuseMap;
uniform vec4 Diffuse = vec4(1,1,1,1);

in vec2 exTexCoord;
smooth in vec3 exNormal;
in vec3 exPosition;

out vec4 color;
out vec4 normalsSpecular;
out vec4 position;

void main(void) {
	color = texture(DiffuseMap, exTexCoord);

	if (color.x == 0 && color.y == 0 && color.z == 0 && color.w == 0 && exTexCoord.x == 0 && exTexCoord.y == 0) {
		color = Diffuse;
	}
	//color = vec4(normalize(exNormal), 1);
	color.w = 1;
	normalsSpecular = vec4(normalize(exNormal), 1);
	position = vec4(exPosition, 1);
}