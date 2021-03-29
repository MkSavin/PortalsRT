using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortalsRT
{
    public static class Global
    {
        public static Random random = new Random();
        public static Vector3 Gravity { get; set; } = -Vector3.UnitY * 2;
    }
}
