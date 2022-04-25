#version 330

out vec3 outputColor;

in vec3 vertexColor;
uniform vec3 ourColor;

void main(){
    outputColor = ourColor;
}