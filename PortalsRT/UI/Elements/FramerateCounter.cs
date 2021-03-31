using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using PortalsRT.Logic;
using PortalsRT.Shaders;

namespace PortalsRT.UI.Elements
{
    public class FramerateCounter : UiElement
    {
        public float radius;

        public FramerateCounter()
        {
            size = new Vector2(140f, 80f);
        }

        protected override void RenderElement(ShaderProgram shader, Vector2 position, Vector2 size, Vector2 renderScaleModifier)
        {
            var radius = 0.005f;

            GL.Begin(PrimitiveType.Polygon);
            Helpers.RoundedRectangle(size.X, size.Y, radius, Vertex2);
            GL.End();
            
            shader.SetVector3("color", new Vector3(0, 0, 0));

            var textPosition = position - new Vector2(32, 13) * renderScaleModifier.Y + new Vector2(radius);

            var textRenderer = new TextRenderer(renderScaleModifier, textPosition);
            textRenderer.Draw(Math.Round(1 / Game.DeltaTime) + "", 26);

            textPosition = position + new Vector2(size.X / 2, 0) - new Vector2(-5, 13) * renderScaleModifier + new Vector2(radius);

            textRenderer.SetPosition(textPosition);

            shader.SetVector3("color", new Vector3(1, 1, 1));
            textRenderer.Draw("fps", 26);
        }
    }
}
