using PortalsRT.Logic.GameModes;

namespace PortalsRT.Logic
{
    public class Game
    {
        public static double DeltaTime { get; protected internal set; }
        public static GameMode GameMode { get; protected internal set; } = new PlayerMode();
        // public static Scene Scene { get; protected internal set; }
    }
}
