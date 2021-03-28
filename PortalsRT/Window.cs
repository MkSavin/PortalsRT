﻿using System;
using System.Drawing;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;

using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

using PortalsRT.Shaders;
using PortalsRT.Input;
using PortalsRT.Logic;
using PortalsRT.PropertyObjects;

namespace PortalsRT
{
    public class Window : GameWindow
    {
        Stopwatch watch;
        public int Frame { get; private set; } = 0;

        private int framebufferTexture = 0;

        // NDC, (0, 0) at center
        private readonly float[] vertices =
        {
             -1f, -1f, 0.0f,
             1f, -1f, 0.0f,
             1f,  1f, 0.0f,
             -1f, 1f, 0.0f,
             -1f, -1f, 0.0f
        };

        private int vertexBufferObject;
        private int vertexArrayObject;

        private ShaderProgram shader;

        private Texture noise;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            watch = Stopwatch.StartNew();
        }

        protected override void OnLoad()
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            var layoutLocation = 0;
            GL.VertexAttribPointer(layoutLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(layoutLocation);

            shader = new ShaderProgram()
              .AddShader(new Shader(ShaderType.VertexShader, new Asset("shaders/raytracing.vert")))
              .AddShader(new Shader(ShaderType.FragmentShader, new Asset("shaders/raytracing.frag")))
              .FullLink();

            shader.Use();

            CursorGrabbed = true;

            framebufferTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Size.X, Size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            noise = new Texture(new Asset("textures/noise.bmp"));
            AssetLoader.LoadTexture(noise);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, noise.ID);

            shader.SetTexture("bluenoiseMask", noise, TextureUnit.Texture1, 1);

            base.OnLoad();
        }

        public void Render(Action<double, ShaderProgram> callback)
        {
            RenderFrame += (e) =>
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);

                watch.Stop();
                Game.DeltaTime = watch.ElapsedMilliseconds / 1000F;
                Title = $"Portals RT. FPS: {1 / Game.DeltaTime}";
                watch.Restart();

                Frame++;
                shader.SetInt("frameNumber", Frame);
                shader.SetTexture("accumTexture", framebufferTexture, TextureUnit.Texture0, 0);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);

                callback.Invoke(Game.DeltaTime, shader);

                GL.BindVertexArray(vertexArrayObject);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 5);

                GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, 0, 0, Size.X, Size.Y, 0);

                SwapBuffers();
            };

            Run();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            if (shader != null)
            {
                shader.SetVector3("screenSize", new Vector3(Size.X, Size.Y, 0));
            }

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);

            shader.DeleteProgram();
            base.OnUnload();
        }
    }
}
