using System;

using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;

using PortalsRT.Shaders;
using PortalsRT.Scene;
using PortalsRT.Scene.Objects;
using PortalsRT.Mathematics.Vector;
using PortalsRT.Input;
using PortalsRT.Output;

namespace PortalsRT
{
    public static class Program
    {
        public const string Version = "0.01 alpha";

        public static List<SceneObject> portals = new List<SceneObject>();

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

                Portal a = (Portal)new Portal().SetTransform(new Transform(new Vector3(2f, 1f, 2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));
                Portal b = (Portal)new Portal().SetTransform(new Transform(new Vector3(2f, 1f, -2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));

                Portal c = (Portal)new Portal().SetTransform(new Transform(new Vector3(6 - 2f, 1f, 2f), new Vector3((float)Math.PI, 0, (float)Math.PI / 2), new Vector3(2f)));
                Portal d = (Portal)new Portal().SetTransform(new Transform(new Vector3(6 - 2f, 1f, -2f), new Vector3((float)Math.PI, 0, (float)Math.PI / 2), new Vector3(2f)));

                a.targetPortal = b;
                b.targetPortal = a;

                c.targetPortal = d;
                d.targetPortal = c;

                portals.Add(a);
                portals.Add(b);
                portals.Add(c);
                portals.Add(d);

                //Portal a = (Portal)new Portal().SetTransform(new Transform(new Vector3(2f, 1f, 2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));
                //Portal b = (Portal)new Portal().SetTransform(new Transform(new Vector3(2f, 1f, -2f), new Vector3((float)Math.PI, 0, (float)Math.PI / 2), new Vector3(2f)));

                //Portal c = (Portal)new Portal().SetTransform(new Transform(new Vector3(6 - 2f, 1f, 2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));
                //Portal d = (Portal)new Portal().SetTransform(new Transform(new Vector3(6 - 2f, 1f, -2f), new Vector3((float)Math.PI, 0, (float)Math.PI / 2), new Vector3(2f)));

                //a.targetPortal = c;
                //b.targetPortal = d;

                //c.targetPortal = a;
                //d.targetPortal = b;

                //portals.Add(a);
                //portals.Add(b);
                //portals.Add(c);
                //portals.Add(d);

                window.Render((deltaTick, shader) => {

                    // StatBar.Write();
                    
                    Camera.Instance.ProcessInput(window.KeyboardState, window.MouseState.Delta);
                    Camera.Instance.ProcessPhysics(portals);
                    Camera.Instance.UploadTransformToShader(shader);

                });
            }
        }

    }
}
