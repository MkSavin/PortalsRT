using System;
using System.Collections.Generic;
using System.Text;

using PortalsRT.Mathematics.Vector;

namespace PortalsRT.Scene
{
    public class SceneObject
    {
        public Transform transform = new Transform();

        public SceneObject SetTransform(Transform _transform)
        {
            transform = _transform;

            return this;
        }
    }
}
