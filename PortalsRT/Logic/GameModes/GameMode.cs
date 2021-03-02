namespace PortalsRT.Logic.GameModes
{
    public abstract class GameMode
    {

        public float Gravity { get; set; } = 0;

        public bool NoClip { get; set; } = true;

        public string Name { get; set; } = "None";

    }
}
