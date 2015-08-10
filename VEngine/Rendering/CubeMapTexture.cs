using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OldGL = OpenTK.Graphics.OpenGL;

namespace VEngine
{
    public class CubeMapTexture
    {
        public bool UseNearestFilter = false;
        private byte[] BitmapPosX,
            BitmapPosY,
            BitmapPosZ, BitmapNegX, BitmapNegY, BitmapNegZ;
        private bool Generated;
        public int Handle = -1;
        private Size Size;
        public CubeMapTexture(string posx, string posy, string posz, string negx, string negy, string negz)
        {
            Update(ref BitmapPosX, posx);
            Update(ref BitmapPosY, posy);
            Update(ref BitmapPosZ, posz);
            Update(ref BitmapNegX, negx);
            Update(ref BitmapNegY, negy);
            Update(ref BitmapNegZ, negz);
            Generated = false;
        }
        
        public void Update(ref byte[] Bitmap, string file)
        {
            var bitmap = new Bitmap(Image.FromFile(file));
            Generated = false;
            if(Handle >= 0)
            {
                GL.DeleteTexture(Handle);
            }
            Size = bitmap.Size;
            Bitmap = BitmapToByteArray(bitmap);
            bitmap.Dispose();
            GC.Collect();
        }
        
        public void Use(TextureUnit unit)
        {
            if(!Generated)
            {
                Handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapPosX);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapPosY);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapPosZ);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapNegX);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapNegY);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapNegZ);
                if(UseNearestFilter)
                {
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
                else
                {
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)TextureMagFilter.Linear);
                    GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
                }
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                Generated = true;
            }
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
        }

        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int numbytes = bmpdata.Stride * bitmap.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);
            //bitmap.UnlockBits(bmpdata);
            return bytedata;
        }
        

        public void FreeGPU() {
            GL.DeleteTexture(Handle);
        }

        public void FreeCPU()
        {
            BitmapPosX = new byte[0];
            BitmapPosY = new byte[0];
            BitmapPosZ = new byte[0];
            BitmapNegX = new byte[0];
            BitmapNegY = new byte[0];
            BitmapNegZ = new byte[0];
        }
    }
}