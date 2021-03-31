using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;

namespace PortalsRT.Physics
{
    public static class Forces
    {
        public static Vector3 Gravity { get; set; } = -Vector3.UnitY * 2;
    }
}
