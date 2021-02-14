using System;
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

namespace PortalsRT
{
    public class Window : GameWindow
    {
        Stopwatch watch;

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

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            watch = Stopwatch.StartNew();
        }

        protected override void OnLoad()
        {
            GL.ClearColor(Color.Black);

            // VBO = ArrayBuffer
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);

            // There are three different BufferUsageHints for drawing:
            //   StaticDraw: This buffer will rarely, if ever, update after being initially uploaded.
            //   DynamicDraw: This buffer will change frequently after being initially uploaded.
            //   StreamDraw: This buffer will change on every frame.
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            var layoutLocation = 0;
            GL.VertexAttribPointer(layoutLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(layoutLocation);

            shader = new ShaderProgram()
              .AddShader(new Shader(ShaderType.VertexShader, "assets/shaders/raytracing.vert"))
              .AddShader(new Shader(ShaderType.FragmentShader, "assets/shaders/raytracing.frag"))
              .FullLink();

            shader.Use();

            CursorGrabbed = true;
            CursorVisible = false;

            base.OnLoad();
        }

        public void Render(Action<double, ShaderProgram> callback)
        {
            RenderFrame += (e) =>
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);

                watch.Stop();
                Game.DeltaTime = watch.ElapsedMilliseconds / 1000F;
                watch.Restart();

                callback.Invoke(Game.DeltaTime, shader);

                GL.BindVertexArray(vertexArrayObject);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 5);

                SwapBuffers();

                // base.OnRenderFrame(e);
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

            Controls.KeyboardState = KeyboardState;
            Controls.MouseState = MouseState;

            base.OnUpdateFrame(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            if (shader != null)
            {
                shader.SetVector3("screenSize", new Vector3(Size.X, Size.Y, 0));
            }

            GL.Viewport(0, 0, Size.X, Size.Y);
            base.OnResize(e);
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
