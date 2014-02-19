#version 150

uniform mat4 uModelView;
uniform vec3 uLightDirection;

in vec3 inVertex;
in vec2 inTexCoord;

out vec2 exTexCoord;
out vec3 L;
out vec3 exPosition;


void main(void) {
	gl_Position = vec4(inVertex, 1.);
	exTexCoord = inTexCoord;
	L = (inverse(uModelView) * vec4(uLightDirection, 1)).xyz;
	L = -normalize((transpose(inverse(uModelView)) * vec4(uLightDirection, 0)).xyz);
	exPosition = (uModelView * vec4(inVertex, 1)).xyz;
}
--------------------------------------------------------------------------------
#version 150

uniform mat4 uModelView;
uniform mat4 uProjection;
uniform sampler2D uColor;
uniform sampler2D uNormalsSpecular;
uniform sampler2D uDepth;
uniform sampler2D uPosition;

uniform float nearPlane = 0.1;
uniform float farPlane = 2000.;

uniform bool uDoShadowMapping;
uniform sampler2D uShadowMap;
uniform int uShadowMaps;
uniform mat4 uShadowMats[4];

uniform vec4 uLightColor = vec4(1,1,1,1);

uniform float uLightIntensity = 1;

in vec2 exTexCoord;
in vec3 L;
in vec3 exPosition;

out vec4 color;

vec4 ShadowCoordPostW;

vec4 Phong(vec3 Position, vec3 N, float SpecularIntensity, float SpecularPower) {
	float diff = max(0.0, dot(normalize(N), normalize(L)));
	vec4 color = diff * texture(uColor, exTexCoord);
	color += vec4(0.5,0.5,0.5,0);
	vec3 refl = normalize(reflect(-normalize(L), normalize(N)));
	float spec = max(0.0, dot(normalize(N), refl));
	if (diff != 0) {
		float fSpec = pow(spec, 32.);
		color.w = fSpec;
	}
	return color;
}

// This define the value to move one pixel left or right
uniform float xPixelOffset = 1. / 3200;

// This define the value to move one pixel up or down
uniform float yPixelOffset = 1. / 2400;

float lookup( vec2 offSet, vec4 ShadowCoord, vec4 ShadowCoordWDivide, int map) {
	vec2 texcoord = ShadowCoordWDivide.xy + vec2(offSet.x * xPixelOffset, offSet.y * yPixelOffset);
	texcoord.x *= 1. / uShadowMaps;
	texcoord.x += (1. / uShadowMaps) * map;
	//float dis = textureProj(uShadowMap,ShadowCoord + vec4(offSet.x * xPixelOffset * ShadowCoord.w, offSet.y * yPixelOffset * ShadowCoord.w, 0.05, 0.0)).x;
	float dis = texture(uShadowMap,texcoord).x;
	if (ShadowCoordWDivide.z > dis)
		return 0.;
	return 1.0;
}


void main(void) {
	vec4 albedoSpec = texture(uColor, exTexCoord);
	vec4 normSpec = texture(uNormalsSpecular, exTexCoord);
	vec3 normals = normalize(normSpec).xyz;
	float specularIntensity = albedoSpec.w;
	float specularPower = normSpec.w;
	float depth = texture(uDepth, exTexCoord).x;
	vec3 position = (uModelView * texture(uPosition, exTexCoord)).xyz;
	color = Phong(position, normals, specularIntensity, specularPower);
	//color = vec4(normals,1);
	//color = vec4(1,1,1,0);
	//color = vec4(normals,1);
	if (uDoShadowMapping) {
		vec4 position2 = texture(uPosition, exTexCoord);
		position2.a = 1;
		float shadow = 1;
		for (int i = 0; i < uShadowMaps; i++) {
			vec4 lightspace = (uShadowMats[i] * position2);
			vec4 ShadowCoordWDivide = lightspace / lightspace.w;

			vec4 lightspace2 = (uShadowMats[i+1] * position2);
			vec4 ShadowCoordWDivide2 = lightspace2 / lightspace2.w;
			//ShadowCoordWDivide.z += 0.001;
			if (ShadowCoordWDivide.x >=0 && ShadowCoordWDivide.x <=1 && ShadowCoordWDivide.y >=0 && ShadowCoordWDivide.y <= 1) {
				float x, y;
				switch (i) {
				case 0:
				case 1:
					shadow = 0;
					for (y = -1.75 ; y <= 1.75; y += 0.5)
						for (x = -1.75 ; x <= 1.75 ; x += 0.5)
							shadow += lookup(vec2(x, y), lightspace, ShadowCoordWDivide, i);
					shadow += lookup(vec2(0, 0), lightspace2, ShadowCoordWDivide2, i+1);
					shadow /= 65.0;
					break;
				case 2:
					shadow = 0;
					for (y = -1.5; y <= 1.5 ; y += 1)
						for (x = -1.5; x <= 1.5 ; x += 1)
							shadow += lookup(vec2(x, y), lightspace, ShadowCoordWDivide, i);
					shadow /= 16.0;
					break;
				case 3:
					shadow = 0;
					for (y = -0.5; y <= 0.5 ; y += 1)
						for (x = -0.5; x <= 0.5 ; x += 1)
							shadow += lookup(vec2(x, y), lightspace, ShadowCoordWDivide, i);
					shadow /= 4.0;
					break;
				default:
					shadow = lookup(vec2(0,0), lightspace, ShadowCoordWDivide, i);
					break;
				}
				//shadow = lookup(vec2(0,0), lightspace, ShadowCoordWDivide, i);
				color = vec4((shadow * 0.5) + 0.5) * (color + vec4(0, 0, 0, 0));
				//color = vec4(1,1,0,1);
				break;
			}
		}
	}
}