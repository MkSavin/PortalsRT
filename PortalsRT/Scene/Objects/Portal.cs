using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Mathematics;

namespace PortalsRT.Scene.Objects
{
    public class Portal : SceneObject
    {
        public Portal targetPortal;

        public Portal ConnectToPortal(Portal target)
        {
            targetPortal = target;
            target.targetPortal = this;

            return this;
        }

        public Vector3 NormalizedSize()
        {
            return new Vector3(transform.scale.X, 0, transform.scale.Y);
        }

        /// <summary>
        /// Checks if point in portal bounds (collides with portal)
        /// </summary>
        /// <param name="point">Position of point</param>
        /// <returns></returns>
        public bool IsPointInBounds(Vector3 point)
        {
             return point.X >= 0 && point.Z >= 0 && point.X <= transform.scale.X && point.Z <= transform.scale.Z && Math.Abs(point.Y) < 1e-1;
        }

    }
}
