using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using PpmSharp.Models;

namespace PpmSharp;

public sealed class PpmViewerWindow : GameWindow
{
    private const int MaxWidth = 1600;
    private const int MaxHeight = 1200;
    private const int MinWidth = 800;
    private const int MinHeight = 600;
    
    
    private readonly PpmImage _image;

    private int _vao;
    private int _vbo;
    private int _shader;
    private int _texture;
    private int _transformLoc;

    private float _zoom = 1.0f;
    private const float ZoomStep = 0.1f;
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 10.0f;

    public PpmViewerWindow(PpmImage image)
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                Title = "PPM Viewer",
                Size = CalculateInitialWindowSize(image),
                MinimumSize = new Vector2i(400, 300)
            })
    {
        _image = image;
    }

    private static Vector2i CalculateInitialWindowSize(PpmImage image)
    {


        var width = image.Width;
        var height = image.Height;
        
        if (width > MaxWidth || height > MaxHeight)
        {
            var scale = Math.Min((float)MaxWidth / width, (float)MaxHeight / height);
            width = (int)(width * scale);
            height = (int)(height * scale);
        }
        
        width = Math.Max(width, MinWidth);
        height = Math.Max(height, MinHeight);

        return new Vector2i(width, height);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0f, 0f, 0f, 1f);

        CreateQuad();
        CreateShader();
        CreateTexture();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shader);
        GL.BindVertexArray(_vao);
        GL.BindTexture(TextureTarget.Texture2D, _texture);

        var transform = CalculateTransform();
        GL.UniformMatrix4(_transformLoc, false, ref transform);

        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        _zoom = e.OffsetY switch
        {
            > 0 => Math.Min(_zoom + ZoomStep, MaxZoom),
            < 0 => Math.Max(_zoom - ZoomStep, MinZoom),
            _ => _zoom
        };
    }

    protected override void OnUnload()
    {
        GL.DeleteTexture(_texture);
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shader);
        base.OnUnload();
    }

    private Matrix4 CalculateTransform()
    {
        var windowAspect = (float)Size.X / Size.Y;
        var imageAspect = (float)_image.Width / _image.Height;

        float scaleX, scaleY;

        if (imageAspect > windowAspect)
        {
            scaleX = 1.0f;
            scaleY = windowAspect / imageAspect;
        }
        else
        {
            scaleX = imageAspect / windowAspect;
            scaleY = 1.0f;
        }

        scaleX *= _zoom;
        scaleY *= _zoom;

        return Matrix4.CreateScale(scaleX, scaleY, 1.0f);
    }

    private void CreateQuad()
    {
        float[] vertices =
        {
            // pos      // uv
            -1f, -1f,   0f, 1f,
            1f, -1f,   1f, 1f,
            -1f,  1f,   0f, 0f,
            1f,  1f,   1f, 0f
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
    }

    private void CreateTexture()
    {
        _texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _texture);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.R8,
            _image.Width,
            _image.Height,
            0,
            PixelFormat.Red,
            PixelType.UnsignedByte,
            _image.Pixels
        );

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    }

    private void CreateShader()
    {
        const string vertexSrc = Generated.Shaders.TextureVert;
        const string fragmentSrc = Generated.Shaders.TextureFrag;

        var vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, vertexSrc);
        GL.CompileShader(vs);

        var fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, fragmentSrc);
        GL.CompileShader(fs);

        _shader = GL.CreateProgram();
        GL.AttachShader(_shader, vs);
        GL.AttachShader(_shader, fs);
        GL.LinkProgram(_shader);

        GL.DeleteShader(vs);
        GL.DeleteShader(fs);

        _transformLoc = GL.GetUniformLocation(_shader, "uTransform");
    }
}