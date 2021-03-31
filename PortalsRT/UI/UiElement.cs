using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using PortalsRT.Shaders;

namespace PortalsRT.UI
{
    public abstract class UiElement
    {
        public Vector2 screenPivot = Vector2.Zero;
        public Vector2 pivot = Vector2.Zero;
        public Vector2 size = Vector2.One;
        public Vector2 position = Vector2.Zero;
        public float rotation = 0;

        protected Vector2 renderScaleModifier = Vector2.Zero;

        public UiElement SetPosition(Vector2 _position)
        {
            position = _position;
            return this;
        }

        public UiElement SetScreenPivot(Vector2 _screenPivot)
        {
            screenPivot = _screenPivot;
            return this;
        }

        public UiElement SetPivot(Vector2 _pivot)
        {
            pivot = _pivot;
            return this;
        }

        public UiElement SetRotation(float _rotation)
        {
            rotation = _rotation;
            return this;
        }

        protected void Vertex2(float x, float y)
        {
            GL.Vertex2(position.X + x, position.Y + y);
        }

        public void Render(ShaderProgram shader)
        {
            var oldPosition = position;
            var oldSize = size;

            renderScaleModifier = new Vector2(1f / Global.currentWindow.Size.X, 1f / Global.currentWindow.Size.Y);

            position *= (Vector2.One - screenPivot - new Vector2(0.5f)) * 2;
            position += Global.currentWindow.Size * ((screenPivot - new Vector2(0.5f)) * 2);
            position -= size * (pivot - new Vector2(0.5f)) * 2;

            position *= renderScaleModifier;

            size *= renderScaleModifier;
            // size *= Vector2.One - (pivot - new Vector2(0.5f));

            RenderElement(shader, position, size, renderScaleModifier);

            shader.SetVector3("color", new Vector3(255, 255, 255));

            position = oldPosition;
            size = oldSize;
        }

        protected abstract void RenderElement(ShaderProgram shader, Vector2 position, Vector2 size, Vector2 renderScaleModifier);

    }
}
