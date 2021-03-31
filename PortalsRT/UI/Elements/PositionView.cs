using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using PortalsRT.Logic;
using PortalsRT.Scene;
using PortalsRT.Shaders;

namespace PortalsRT.UI.Elements
{
    public class PositionView : UiElement
    {
        public PositionView()
        {
            size = new Vector2(75, 20);
        }

        protected override void RenderElement(ShaderProgram shader, Vector2 position, Vector2 size, Vector2 renderScaleModifier)
        {
            var textRenderer = new TextRenderer(renderScaleModifier, position);
            var cameraPosition = Camera.Instance.transform.position;
            textRenderer.Draw(string.Format("x: {0}, y: {1}, z: {2}", Math.Round(cameraPosition.X * 100) / 100, Math.Round(cameraPosition.Y * 100) / 100, Math.Round(cameraPosition.Z * 100) / 100), 18);
        }
    }
}
