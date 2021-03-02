using System;
using System.Collections.Generic;
using System.Text;

namespace PortalsRT.Mathematics
{
    public static class Helpers
    {
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
    }
}
