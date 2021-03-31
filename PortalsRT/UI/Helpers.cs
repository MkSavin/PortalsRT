using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics.OpenGL;

namespace PortalsRT.UI
{
    class Helpers
    {

        public static void Rectangle(float width, float height, Action<float, float> drawer = null)
        {
            if (drawer == null)
            {
                drawer = (x, y) => { GL.Vertex2(x, y); };
            }

            drawer.Invoke(0, 0);
            drawer.Invoke(width, 0);
            drawer.Invoke(width, height);
            drawer.Invoke(0, height);
        }

        private static void RoundedPart(int sector, int steps, float radius, float offsetX, float offsetY, Action<float, float> drawer = null)
        {
            if (drawer == null)
            {
                drawer = (x, y) => { GL.Vertex2(x, y); };
            }

            var sectorAngle = Math.PI / 2;

            var angleStep = sectorAngle / 16;

            double angle;

            for (int i = 0; i < steps; i++)
            {
                angle = sector * sectorAngle + angleStep * i;
                drawer.Invoke(offsetX + (float)Math.Cos(angle) * radius, offsetY + (float)Math.Sin(angle) * radius);
            }
        }

        public static void RoundedRectangle(float width, float height, float radius, Action<float, float> drawer = null)
        {
            if (drawer == null)
            {
                drawer = (x, y) => { GL.Vertex2(x, y); };
            }

            RoundedPart(0, 16, radius, width / 2 - radius, height / 2 - radius, drawer);
            RoundedPart(1, 16, radius, -width / 2 + radius, height / 2 - radius, drawer);
            RoundedPart(2, 16, radius, -width / 2 + radius, -height / 2 + radius, drawer);
            RoundedPart(3, 16, radius, width / 2 - radius, -height / 2 + radius, drawer);
        }

    }
}
