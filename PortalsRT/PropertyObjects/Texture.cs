using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using PortalsRT.Input;

namespace PortalsRT.PropertyObjects
{
    public class Texture
    {
        public Asset Asset { get; private set; }

        public Texture(string location) : this(new Asset(location)) { }

        public int ID = 0;

        public Texture(Asset asset)
        {
            Asset = asset;
        }

        public BitmapData BitmapData()
        {
            var location = Asset.Location;

            if (!File.Exists(location))
            {
                location = new Asset("textures/notex.bmp").Location;
            }

            if (!File.Exists(location))
            {
                return null;
            }

            var bmp = new Bitmap(location);
            var bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmp.UnlockBits(bmpdata);

            return bmpdata;
        }

    }
}
