#version 150

uniform mat4 projection;
uniform mat4 modelView;

in vec3 inVertex;

out vec4 exPosition;

void main(void) {
	gl_Position =  projection * modelView * vec4(inVertex, 1);
	exPosition = gl_Position;
}
--------------------------------------------------------------------------------
#version 150
in vec4 exPosition;

out vec4 color;

void main(void) {
	
	float depth = exPosition.z / exPosition.w ;
	depth = depth * 0.5 + 0.5;			//Don't forget to move away from unit cube ([-1,1]) to [0,1] coordinate system

	float moment1 = depth;
	float moment2 = depth * depth;

	// Adjusting moments (this is sort of bias per pixel) using derivative
	float dx = dFdx(depth);
	float dy = dFdy(depth);
	moment2 += 0.25*(dx*dx+dy*dy) ;

	color = vec4( moment1,moment2, 0.0, 0.0 );
}