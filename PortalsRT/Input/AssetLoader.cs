using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;

using OpenTK.Graphics.OpenGL;

using PortalsRT.PropertyObjects;

namespace PortalsRT.Input
{
    public class AssetLoader
    {
        public static Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();

        public static int LoadTexture(Texture texture)
        {
            if (!LoadedTextures.ContainsKey(texture.Asset.Location))
            {
                // Console.WriteLine("Loading new texture");

                if (!File.Exists(texture.Asset.Location))
                {
                    return texture.ID;
                }

                BitmapData bitmapData;

                int textureId = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, textureId);

                LoadedTextures.Add(texture.Asset.Location, texture);

                texture.ID = textureId;
                bitmapData = texture.BitmapData();

                if (bitmapData == null)
                {
                    return texture.ID;
                }

                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bitmapData.Scan0);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            else
            {
                // Console.WriteLine("Loading already initialized texture");
                GL.BindTexture(TextureTarget.Texture2D, LoadedTextures[texture.Asset.Location].ID);
            }

            return texture.ID;
        }
    }
}
