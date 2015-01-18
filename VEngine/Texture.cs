using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace VDGTech
{
    public class Texture
    {
        int Handle;
        bool Generated;
        byte[] Bitmap;
        Size Size;

        public Texture(string file)
        {
            var bitmap = new Bitmap(Image.FromFile(file));
            Generated = false;
            Bitmap = BitmapToByteArray(bitmap);
            Size = bitmap.Size;
        }

        public void Use(TextureUnit unit)
        {
            if (!Generated)
            {
                Handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, Handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, Bitmap);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                Generated = true;
            }
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int numbytes = bmpdata.Stride * bitmap.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);
            bitmap.UnlockBits(bmpdata);
            return bytedata;

        }
    }
}
