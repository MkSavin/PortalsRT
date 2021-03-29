using OpenTK.Mathematics;

namespace PortalsRT.Mathematics.Vector
{
    public static class Randomization
    {
        public static Vector3 AddRandom(this Vector3 vector, int start, int end, float scale = 1)
        {
            return vector
                + (
                start == end ?
                    new Vector3(start) :
                    new Vector3(
                        Global.random.Next(start, end),
                        Global.random.Next(start, end),
                        Global.random.Next(start, end)
                    )
                )
                * scale;
        }
    }
}
