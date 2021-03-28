using System;

using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

using PortalsRT.Shaders;
using PortalsRT.Scene;
using PortalsRT.Input;
using PortalsRT.Output;

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

                    // StatBar.Write();
                    
                    Camera.Instance.ProcessInput(window.KeyboardState, window.MouseState.Delta);
                    Camera.Instance.ProcessPhysics();
                    Camera.Instance.UploadTransformToShader(shader);

                });
            }
        }

    }
}
