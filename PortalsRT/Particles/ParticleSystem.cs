using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using PortalsRT.Mathematics;

namespace PortalsRT.Particles
{
    public class ParticleSystem
    {
        public float CurrentLifetime { get; private set; } = 0;
        private Particle[] particles;

        public Vector3 Position { get; private set; } = Vector3.Zero;
        public Vector3 Rotation { get; private set; } = Vector3.Zero;
        public ParticleEmitter Emitter { get; private set; }

        public Matrix3 RotationMatrix
        {
            get
            {
                return
                    Matrix3.CreateRotationX(Rotation.X)
                    * Matrix3.CreateRotationY(Rotation.Y)
                    * Matrix3.CreateRotationZ(Rotation.Z);
            }
        }

        public Matrix4 TranslationMatrix
        {
            get
            {
                return Matrix4.CreateTranslation(Position);
            }
        }

        public Matrix4 TransformMatrix
        {
            get
            {
                return
                    new Matrix4(RotationMatrix)
                    * TranslationMatrix;
            }
        }

        public ParticleSystem(ParticleEmitter emitter)
        {
            Emitter = emitter;
        }

        public ParticleSystem SetPosition(Vector3 _Position)
        {
            Position = _Position;
            return this;
        }

        public ParticleSystem SetRotation(Vector3 _Rotation)
        {
            Rotation = _Rotation;
            return this;
        }

        public bool EmissionCompleted()
        {
            return CurrentLifetime >= Emitter.Lifetime;
        }

        public ParticleSystem ClearCurrentLifetime()
        {
            CurrentLifetime = 0;
            return this;
        }

        public ParticleSystem Restart()
        {
            ClearCurrentLifetime();
            Emit();

            return this;
        }


        public ParticleSystem Cycle()
        {
            if (Emitter.Cycled && EmissionCompleted())
            {
                Restart();
            }

            return this;
        }

        public ParticleSystem Emit()
        {
            particles = Emitter.Emit();

            return this;
        }

        public ParticleSystem ProcessPhysics(float deltaTime)
        {
            if (!EmissionCompleted())
            {
                CurrentLifetime += deltaTime;
            }

            if (particles != null)
            {
                foreach (var particle in particles)
                {
                    particle.ProcessPhysics(deltaTime);
                }
            }

            return this;
        }

        public ParticleSystem RenderGizmos()
        {
            var transform = TransformMatrix;
            GL.LoadMatrix(ref transform);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(10, 0, 0);

            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 10, 0);

            GL.Color3(Color.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 10);
            GL.End();

            return this;
        }

        public List<Vector3> ParticlesPositions()
        {
            var positions = new List<Vector3>();

            if (particles != null && !EmissionCompleted())
            {
                foreach (var particle in particles)
                {
                    positions.Add(particle.GetShaderData(this));
                }
            }

            return positions;
        }

        public ParticleSystem Render()
        {
            if (particles != null && !EmissionCompleted())
            {
                foreach (var particle in particles)
                {
                    particle.Render(this);
                }
            }

            return this;
        }

    }
}
