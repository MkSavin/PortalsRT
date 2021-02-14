using System;

using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

using PortalsRT.Shaders;
using PortalsRT.Scene;
using PortalsRT.Input;

namespace PortalsRT
{
    public static class Program
    {
        public const string Version = "0.01 alpha";

        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1600, 1200),
                Title = "Portals RT v" + Version,
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                // window.VSync = VSyncMode.Off;
                window.Render((deltaTick, shader) => {

                    ProcessKeyboardControls(window);
                    ProcessMouseControls(window);

                    Camera.Instance.ProcessGameMode();

                    shader.SetVector3("cameraRotation", Camera.Instance.transform.rotation);
                    shader.SetVector3("cameraPosition", Camera.Instance.transform.position);

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Current position: " + Camera.Instance.transform.position);
                    Console.WriteLine("Current rotation: " + Camera.Instance.transform.rotation);
                });

                // window.Run();
            }
        }


        private static void ProcessKeyboardControls(Window window)
        {
            var camera = Camera.Instance;

            camera.ProcessControls();

            if (Controls.IsWindowStateChanged())
            {
                window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
            }
        }

        private static void ProcessMouseControls(Window window)
        {
            var camera = Camera.Instance;
            var mouseInput = Controls.MouseInput();

            camera.AddYawInput(mouseInput.X);
            camera.AddPitchInput(mouseInput.Y);
        }

    }
}
