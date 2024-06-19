#version 330 core
out vec4 FragColor;

in vec2 vUV;
out int vStringIndex;
out int vStringLength;

uniform sampler2D fontAtlas;
uniform vec2 gridDims;
uniform vec2 charPos;//TODO remove this and use string index and length
uniform float opacity;

//uniform vec4 charMask;

void main()
{
    vec2 targetChar = charPos; // charPos is the target position
    vec2 stride = vec2(1.) / gridDims;
    vec2 tUV = vUV * stride;
    tUV += stride * targetChar;

    //TODO modify tUV to use the charmask?

    if(any(equal(targetChar, vec2(-1.)))) tUV = vUV;

    FragColor = vec4(texture(fontAtlas, tUV).rgb, opacity);
}