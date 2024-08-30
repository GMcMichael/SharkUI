#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor;
layout (location = 2) in float aScale;
layout (location = 3) in int aCharIndex;

layout (std140, binding = 0) uniform Atlas {
    vec2 charUVs[256];
}

out vec2 vUV;
out vec4 vColor;

void main()
{
    vUV = charUVs[aCharIndex];
    vColor = aColor;
    gl_Position = vec4(aPosition, -1., 1.);
}