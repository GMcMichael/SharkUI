#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aUV;
layout (location = 2) in vec2 aOffset;
layout (location = 3) in int aStringIndex;
layout (location = 4) in int aStringLength;

uniform mat4 model;

out vec2 vUV;
out int vStringIndex;
out int vStringLength;

void main()
{
    vUV = aUV;
    vStringIndex = aStringIndex;
    vStringLength = aStringLength;
    gl_Position = vec4(aPosition + vec3(aOffset, 0.), 1.) * model;
}