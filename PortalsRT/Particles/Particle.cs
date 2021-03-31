using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using PortalsRT.Mathematics.Vector;

namespace PortalsRT.Particles
{
    public class Particle
    {
        public Vector3 position;
        public Vector3 velocity = Vector3.Zero;

        public Vector2 size = Vector2.One;

        private Color startColor = Color.FromArgb(248, 253, 202);
        private Color endColor = Color.FromArgb(152, 30, 7);

        public Particle(Vector3 _position, Vector3 _velocity, int randomness)
        {
            position = _position.AddRandom(-randomness / 2, randomness / 2, 1e-3f);
            velocity = _velocity.AddRandom(-randomness / 2, randomness / 2, 1e-3f);
        }

        public Particle(Vector3 _position, float impulse, int randomness) : this(_position, _position.Normalized() * impulse, randomness) { }

        public void ProcessPhysics(float deltaTime)
        {
            velocity += Physics.Forces.Gravity * deltaTime * 100;
            position += velocity * deltaTime;
        }

        private float Lerp(float a, float b, float percentage)
        {
            return a + (b - a) * percentage;
        }

        private Color ColorLerp(Color a, Color b, float percentage)
        {
            return Color.FromArgb(
                (int)Lerp(a.R, b.R, percentage),
                (int)Lerp(a.G, b.G, percentage),
                (int)Lerp(a.B, b.B, percentage)
                );
        }

        public Vector3 GetShaderData(ParticleSystem system)
        {
            return (new Vector4(position, 1) * system.TransformMatrix).Xyz;
        }

        public void Render(ParticleSystem system)
        {
            GL.Color3(ColorLerp(startColor, endColor, system.CurrentLifetime / system.Emitter.Lifetime));

            var transformMatrix = 
                new Matrix4(system.RotationMatrix.Inverted())
                * Matrix4.CreateTranslation(position)
                * system.TransformMatrix;

            GL.LoadMatrix(ref transformMatrix);

            GL.Begin(PrimitiveType.Quads);

            GL.Vertex3(0, 0, 0);
            GL.Vertex3(size.X, 0, 0);
            GL.Vertex3(size.X, size.Y, 0);
            GL.Vertex3(0, size.Y, 0);

            GL.End();

            GL.Translate(-position);
        }
    }
}
