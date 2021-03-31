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
    public class ControlsTip : UiElement
    {
        private float radius;
        private float padding;

        public ControlsTip()
        {
            size = new Vector2(120f * 3, 120f * 2);
            radius = padding = 0.005f;
        }

        private void DrawButton(ShaderProgram shader, Vector2 size, int posX, int posY, string text)
        {
            var buttonSize = new Vector2(size.X / 3, size.Y / 2) - new Vector2(padding);
            var buttonPosition = -size / 2 + (buttonSize + new Vector2(padding)) * new Vector2(posX, posY) + buttonSize / 2;

            shader.SetVector3("color", new Vector3(1, 1, 1));

            GL.Begin(PrimitiveType.Polygon);
            Helpers.RoundedRectangle(buttonSize.X, buttonSize.Y, radius, (x, y) => Vertex2(buttonPosition.X + x, buttonPosition.Y + y));
            GL.End();

            shader.SetVector3("color", new Vector3(0, 0, 0));

            var textRenderer = new TextRenderer(renderScaleModifier, position + buttonPosition - new Vector2(10, 11) * renderScaleModifier);
            textRenderer.Draw(text, 26);
        }

        protected override void RenderElement(ShaderProgram shader, Vector2 position, Vector2 size, Vector2 renderScaleModifier)
        {
            DrawButton(shader, size, 1, 1, "w");
            DrawButton(shader, size, 0, 0, "a");
            DrawButton(shader, size, 1, 0, "s");
            DrawButton(shader, size, 2, 0, "d");
        }
    }
}
