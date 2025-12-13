 #version 330 core
                                   in vec2 vUV;
                                   out vec4 FragColor;

                                   uniform sampler2D uTex;

                                   void main()
                                   {
                                       vec4 color = texture(uTex, vUV);
                                       // If grayscale (single channel), expand to RGB
                                       if (color.g == 0.0 && color.b == 0.0)
                                       {
                                           FragColor = vec4(color.r, color.r, color.r, 1.0);
                                       }
                                       else
                                       {
                                           FragColor = vec4(color.rgb, 1.0);
                                       }
                                   }