#version 330 core
out vec4 FragColor;

in vec2 vUV;
in vec4 vColor;

uniform sampler2D fontAtlas;

void main()
{
   FragColor = vColor + texture(fontAtlas, vUV).rrrr;
}