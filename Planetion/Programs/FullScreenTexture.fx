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
uniform sampler2D uLightMap;

in vec2 exTexCoord;

out vec4 color;

void main(void) {
	vec4 albedo = texture(uTexture, exTexCoord);
	vec4 light = texture(uLightMap, exTexCoord);
	color = clamp(vec4(albedo.xyz*light.xyz,1),0.,1.);
	//color = (vec4(albedo.xyz*light.xyz + vec3(light.w,light.w,light.w),1));
	//color = light;
	//color.a = 1;
	//color = clamp(color,0.,1.);
	//color = albedo;
	//color.a = 1;
	//color = vec4(light.x,light.y,light.z,1);
	//color = vec4((2.0 * 1) / (1000 + 1 - light.x * (1000 - 1)),0,0,1);	
	//color = vec4(light.w,light.w,light.w,1);
}