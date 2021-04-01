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
    public class GameStateView : UiElement
    {
        public GameStateView()
        {
            size = new Vector2(75, 20);
        }

        protected override void RenderElement(ShaderProgram shader, Vector2 position, Vector2 size, Vector2 renderScaleModifier)
        {
            var textRenderer = new TextRenderer(renderScaleModifier, position);
            textRenderer.Draw(string.Format("raymarching [{0}], denoising [{1}]", Game.RayMarchingEnabled ? "enabled" : "disabled", Game.DenoisingEnabled ? "enabled" : "disabled"));
        }
    }
}
