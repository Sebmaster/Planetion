#version 140

in vec2 inVertex;
in vec2 inTexCoord;

out vec2 exTexCoord;

void main(void) {
	gl_Position = vec4(inVertex, 0., 1.);
	exTexCoord = inTexCoord;
}
--------------------------------------------------------------------------------
#version 140

uniform sampler2D uTexture;
uniform vec2 uScale;

// Precalculated weights and offsets
float weights[15] = { 0.1061154, 0.1028506, 0.1028506, 0.09364651, 0.09364651, 
	0.0801001, 0.0801001, 0.06436224, 0.06436224, 0.04858317, 0.04858317, 
	0.03445063, 0.03445063, 0.02294906, 0.02294906 };

float offsets[15] = { 0, 0.00125, -0.00125, 0.002916667, -0.002916667, 
	0.004583334, -0.004583334, 0.00625, -0.00625, 0.007916667, -0.007916667, 
	0.009583334, -0.009583334, 0.01125, -0.01125 };

in vec2 exTexCoord;

out vec4 color;

void main(void) {
	color = vec4(0, 0, 0, 1);
    
	// Sample from the surrounding pixels using the precalculated
	// pixel offsets and color weights
    for (int i = 0; i < 15; i++) {
		color += texture(uTexture, exTexCoord + vec2(offsets[i], 0)) * weights[i];
		color += texture(uTexture, exTexCoord + vec2(0, offsets[i])) * weights[i];
	}
	color /= 2;
}