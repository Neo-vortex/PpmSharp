#version 330 core
in vec2 vUV;
out vec4 FragColor;

uniform sampler2D uTex;

void main()
{
    float g = texture(uTex, vUV).r;
    FragColor = vec4(g, g, g, 1.0);
}