using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;

namespace PortalsRT.Math
{
    public class Transform
    {

        public Vector3 position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 scale = Vector3.One;

        public override string ToString()
        {
            return "P: [" + position + "], R: [" + rotation + "], S: [" + scale + "]";
        }

    }
}
