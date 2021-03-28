using System;
using System.IO;

namespace PortalsRT.Input
{
    public class Asset
    {
        public string Location { get; private set; } = "";
        public string RelativeLocation { get; private set; } = "";

        public Asset(string location)
        {
            RelativeLocation = location;
            Location = GetLocation(location);
        }

        public bool Exists()
        {
            return File.Exists(Location);
        }

        public static string GetLocation(string asset)
        {
            return AppDomain.CurrentDomain.BaseDirectory + "assets/" + asset.Trim('/');
        }

        public string Text()
        {
            if (!Exists())
            {
                return null;
            }

            return File.ReadAllText(Location);
        }

    }
}
