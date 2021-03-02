using PortalsRT.Logic;
using PortalsRT.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortalsRT.Output
{
    public class StatBar
    {
        public static void Write()
        {
            Console.SetCursorPosition(0, 0);

            // Bar: [60 FPS] [Basic] [GM: Player] [Camera: Perspective, P: [...], ...]

            List<string> entries = new List<string>() {
                "60 FPS",
                "Basic",
                "GM: " + Game.GameMode.Name,
                "Camera: " + (Camera.Instance.IsPerspective ? "Perspective" : "Ortho") + ", " + Camera.Instance.transform,
                "Vel: " + Camera.Instance.relativeVelocity,
            };

            foreach (var entry in entries)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write($"[{entry}]");

                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write("  ");
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

    }
}
