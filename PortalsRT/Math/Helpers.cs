using System;
using System.Collections.Generic;
using System.Text;

namespace PortalsRT.Math
{
    public static class Helpers
    {
        public static float Clamp(float value, float min, float max)
        {
            return System.Math.Max(System.Math.Min(value, max), min);
        }
    }
}
