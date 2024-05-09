#version 330 core
out vec4 FragColor;

in vec2 vUV;

uniform sampler2D textureSlot;
uniform float opacity;

void main()
{
    if(opacity <= 0.) discard;
    FragColor = vec4(texture(textureSlot, vUV).rgb, opacity);
}