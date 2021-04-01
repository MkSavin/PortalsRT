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
using PortalsRT.Particles;
using PortalsRT.UI;
using PortalsRT.UI.Elements;
using PortalsRT.Logic;

namespace PortalsRT
{
    public static class Program
    {
        public const string Version = "0.01 alpha";

        public static List<SceneObject> portals = new List<SceneObject>();
        public static List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        private static void LoadPortals()
        {
            Portal a = (Portal)new Portal().SetTransform(new Transform(new Vector3(2f, 1f, 2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));
            Portal b = (Portal)new Portal().SetTransform(new Transform(new Vector3(2f, 1f, -2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));

            Portal c = (Portal)new Portal().SetTransform(new Transform(new Vector3(6 - 2f, 1f, 2f), new Vector3((float)Math.PI, 0, (float)Math.PI / 2), new Vector3(2f)));
            Portal d = (Portal)new Portal().SetTransform(new Transform(new Vector3(6 - 2f, 1f, -2f), new Vector3((float)Math.PI, 0, (float)Math.PI / 2), new Vector3(2f)));

            Portal e = (Portal)new Portal().SetTransform(new Transform(new Vector3(12 + 2f, 1f, 2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));
            Portal f = (Portal)new Portal().SetTransform(new Transform(new Vector3(12 + 2f, 1f, -2f), new Vector3(0, 0, (float)Math.PI / 2), new Vector3(2f)));

            a.ConnectToPortal(c);
            b.ConnectToPortal(e);
            d.ConnectToPortal(f);

            portals.Add(a);
            portals.Add(b);
            portals.Add(c);
            portals.Add(d);
            portals.Add(e);
            portals.Add(f);
        }

        private static void LoadParticles()
        {
            particleSystems.Add(
                new ParticleSystem(
                    new ParticleEmitter()
                        .SetParticlesCount(3)
                        .SetScatterRadius(1f)
                        .SetShape(ParticleEmitterShape.Semisphere)
                        .SetLifetime(2f)
                        .SetStraightAcceleration(160f)
                        .SetRandomness(20000)
                        .SetCycled()
                    )
                    .SetPosition(new Vector3(0, 100, 0))
                    .Emit()
                );
        }

        private static void PassParticles(float deltaTick, ShaderProgram shader)
        {
            var particlesPositions = new List<Vector3>();

            foreach (var particleSystem in particleSystems)
            {
                particlesPositions.AddRange(
                    particleSystem
                        .Cycle()
                        .ProcessPhysics(deltaTick)
                        .RenderGizmos()
                        .ParticlesPositions()
                    );
            }

            for (int i = 0; i < 128; i++)
            {
                if (i >= particlesPositions.Count)
                {
                    break;
                }

                shader.SetVector3("sparksPositions[" + i + "]", particlesPositions[i]);
            }
        }
        private static void LoadAndProcessObjects()
        {
            LoadPortals();
            LoadParticles();
        }

        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1600, 1200),
                Title = "Portals RT v" + Version,
                Profile = ContextProfile.Compatability,
                APIVersion = new Version(3, 2),
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                LoadAndProcessObjects();

                List<UiElement> uiElements = new List<UiElement>();

                uiElements.Add(
                    new FramerateCounter()
                        .SetScreenPivot(new Vector2(0, 1))
                        .SetPivot(new Vector2(0, 1))
                        .SetPosition(new Vector2(80, 80))
                    );
                uiElements.Add(
                    new PositionView()
                        .SetScreenPivot(new Vector2(0, 1))
                        .SetPivot(new Vector2(0, 1))
                        .SetPosition(new Vector2(80, 220))
                    );
                uiElements.Add(
                    new GameStateView()
                        .SetScreenPivot(new Vector2(0, 1))
                        .SetPivot(new Vector2(0, 1))
                        .SetPosition(new Vector2(80, 260))
                    );
                //uiElements.Add(
                //    new ControlsTip()
                //        .SetScreenPivot(new Vector2(1, 0))
                //        .SetPivot(new Vector2(1, 0))
                //        .SetPosition(new Vector2(80, 80))
                //    );

                Vector2i halfWindowSize = window.Size / 2;

                window
                    .KeyboardKeyDown((helper) => {
                        helper
                            .KeyPress(Keys.F, () => { Game.RayMarchingEnabled = !Game.RayMarchingEnabled; })
                            .KeyPress(Keys.E, () => { Game.DenoisingEnabled = !Game.DenoisingEnabled; });
                    })
                    .Render(
                        renderCallback: (deltaTick, shader) => {

                            shader.SetInt("raymarchingEnabled", Game.RayMarchingEnabled ? 1 : 0);
                            shader.SetInt("denoisingEnabled", Game.DenoisingEnabled ? 1 : 0);
                        
                            // StatBar.Write();

                            PassParticles((float)deltaTick, shader);
                            Camera.Instance.UploadTransformToShader(shader);

                        },
                        updateCallback: (deltaTick) => {

                            Camera.Instance.ProcessInput(window.KeyboardState, window.MouseState.Delta);
                            Camera.Instance.ProcessPhysics(portals);

                        },
                        uiCallback: (deltaTick, shader) => {

                            foreach (var element in uiElements)
                            {
                                element.Render(shader);
                            }

                        }
                    );
            }
        }

    }
}
