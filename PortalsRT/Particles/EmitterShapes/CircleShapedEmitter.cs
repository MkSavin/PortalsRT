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
    class CircleShapedEmitter : ShapedEmitter
    {
        public Particle[] Emit(ParticleEmitter emitter)
        {
            var particles = new Particle[emitter.ParticlesCount];

            float stepAngle = 2 * (float)Math.PI / emitter.ParticlesCount;

            for (int i = 0; i < emitter.ParticlesCount; i++)
            {
                particles[i] = new Particle(
                    new Vector3(
                        (float)Math.Cos(stepAngle * i), 
                        0, 
                        (float)Math.Sin(stepAngle * i)
                    ) * emitter.ScatterRadius,
                    emitter.StraightAcceleration,
                    emitter.Randomness
                );
            }

            return particles;
        }
    }
}
