using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalsRT.Particles.EmitterShapes
{
    interface ShapedEmitter
    {
        Particle[] Emit(ParticleEmitter emitter);
    }
}
