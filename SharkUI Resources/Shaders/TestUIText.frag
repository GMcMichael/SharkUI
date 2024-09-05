#version 330 core
out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;

uniform vec2 CharMask;
uniform sampler2D fontAtlas;

flat in int TestIndex;

const vec3 Red = vec3(1.,0.,0.);
const vec3 Green = vec3(0.,1.,0.);
const vec3 Blue = vec3(0.,0.,1.);


void main()
{
	vec4 Color = vColor;
	if(TestIndex >= 256) {
		Color = vec4(Red,1.);
	} else if(TestIndex == 0) {
		Color = vec4(Blue,1.);
	} else {
		Color = vec4(Green,1.);
	}

	FragColor = Color * texture(fontAtlas, mix(vUV, vUV + CharMask.xy, gl_PointCoord));
}