using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using PortalsRT.Particles.EmitterShapes;

namespace PortalsRT.Particles
{
    public class ParticleEmitter
    {
        public int ParticlesCount { get; private set; } = 16;
        public float ScatterRadius { get; private set; } = 10;
        public ParticleEmitterShape Shape { get; private set; } = ParticleEmitterShape.Circle;
        public float Lifetime { get; private set; } = 10;
        public float StraightAcceleration { get; private set; } = 10f;
        public int Randomness { get; private set; } = 0;

        public bool Cycled { get; private set; } = false;

        public ParticleEmitter SetParticlesCount(int _ParticlesCount)
        {
            ParticlesCount = _ParticlesCount;
            return this;
        }

        public ParticleEmitter SetScatterRadius(float _ScatterRadius)
        {
            ScatterRadius = _ScatterRadius;
            return this;
        }

        public ParticleEmitter SetShape(ParticleEmitterShape _Shape)
        {
            Shape = _Shape;
            return this;
        }

        public ParticleEmitter SetLifetime(float _Lifetime)
        {
            Lifetime = _Lifetime;
            return this;
        }

        public ParticleEmitter SetStraightAcceleration(float _StraightAcceleration)
        {
            StraightAcceleration = _StraightAcceleration;
            return this;
        }

        public ParticleEmitter SetRandomness(int _Randomness)
        {
            Randomness = _Randomness;
            return this;
        }

        public ParticleEmitter SetCycled(bool _Cycled = true)
        {
            Cycled = _Cycled;
            return this;
        }

        public Particle[] Emit()
        {
            ShapedEmitter emitter = null;

            switch (Shape)
            {
                case ParticleEmitterShape.Semisphere:
                    emitter = new SemisphereShapedEmitter();
                    break;
                case ParticleEmitterShape.Sphere:
                    emitter = new SphereShapedEmitter();
                    break;
                case ParticleEmitterShape.Circle:
                    emitter = new CircleShapedEmitter();
                    break;
                default:
                    break;
            }

            return emitter?.Emit(this);
        }

    }
}
