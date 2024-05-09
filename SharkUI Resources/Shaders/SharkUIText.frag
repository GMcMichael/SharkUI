#version 330 core
out vec4 FragColor;

in vec2 vUV;

uniform sampler2D fontAtlas;
uniform vec2 gridDims;
uniform vec2 charPos;
//uniform float opacity;// make opacity a uniform

//uniform vec4 charMask;

void main()
{
    float opacity = 0.5;
    
    //vec3 color = vec3(0.);
    //vec2 charLeftBottom = vec2(charMask.x + charPos.x, charMask.z + charPos.y);
    //vec2 charRightTop = vec2(1. - charMask.y + charPos.x, 1. - charMask.w + charPos.y);
    //
    //vec2 uv = mix(vec2(0.), vec2(1.), vUV);
    //
    //color = texture(fontAtlas, uv).rgb;
    //
    //uv = fract(vUV * gridDims);
    //vec2 cell = floor(vUV * gridDims);
    //cell.y = (gridDims.y - 1.) - cell.y;
    //
    //uv = vec2(vUV.x * gridDims.x, gridDims.y - vUV.y * gridDims.y);
    //
    //if(all(greaterThanEqual(uv, charLeftBottom)) && all(lessThanEqual(uv, charRightTop))) {
    //    color = vec3(fract(uv), 0.);
    //    opacity = 1.;
    //}

    vec2 targetChar = charPos; // charPos is the target position
    vec2 stride = vec2(1.) / gridDims;
    vec2 tUV = vUV * stride;
    tUV += stride * targetChar;

    //modify tUV to use the charmask

    if(any(equal(targetChar, vec2(-1.)))) tUV = vUV;

    FragColor = vec4(texture(fontAtlas, tUV).rgb, opacity);
}