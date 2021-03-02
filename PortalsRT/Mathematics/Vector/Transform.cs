using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;

namespace PortalsRT.Mathematics.Vector
{
    public class Transform
    {
        public static Transform Zero { get; } = new Transform();

        public Transform(Vector3? _position = null, Vector3? _rotation = null, Vector3? _scale = null)
        {
            position = _position ?? Vector3.Zero;
            rotation = _rotation ?? Vector3.Zero;
            scale = _scale ?? Vector3.One;
        }

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public override string ToString()
        {
            return "P: [" + position + "], R: [" + rotation + "], S: [" + scale + "]";
        }
    }
}
