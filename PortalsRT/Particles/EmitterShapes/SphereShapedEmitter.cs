using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

using PortalsRT.Mathematics;

namespace PortalsRT.Particles.EmitterShapes
{
    class SphereShapedEmitter : ShapedEmitter
    {
        public Particle[] Emit(ParticleEmitter emitter)
        {
            var particles = new Particle[emitter.ParticlesCount * emitter.ParticlesCount];

            float stepAngle = 2 * (float)Math.PI / emitter.ParticlesCount;

            for (int j = 0; j < emitter.ParticlesCount; j++)
            {
                for (int i = 0; i < emitter.ParticlesCount; i++)
                {
                    particles[i + j * emitter.ParticlesCount] = new Particle(
                        new Vector3(
                            (float)Math.Cos(stepAngle * i) * (float)Math.Cos(stepAngle * j),
                            (float)Math.Sin(stepAngle * j),
                            (float)Math.Sin(stepAngle * i) * (float)Math.Cos(stepAngle * j)
                        ) * emitter.ScatterRadius,
                        emitter.StraightAcceleration,
                        emitter.Randomness
                    );
                }
            }

            return particles;
        }
    }
}
