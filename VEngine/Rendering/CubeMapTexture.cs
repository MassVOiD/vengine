using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class CubeMapTexture
    {
        public int Handle = -1;
        public bool UseNearestFilter = false;

        private byte[] BitmapPosX,
                                                            BitmapPosY,
            BitmapPosZ, BitmapNegX, BitmapNegY, BitmapNegZ;

        private bool Generated;

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

        public CubeMapTexture(int handle)
        {
            Handle = handle;
            Generated = true;
        }

        public void FreeCPU()
        {
            BitmapPosX = null;
            BitmapPosY = null;
            BitmapPosZ = null;
            BitmapNegX = null;
            BitmapNegY = null;
            BitmapNegZ = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void FreeGPU()
        {
            GL.DeleteTexture(Handle);
        }

        public void Update(ref byte[] Bitmap, string file)
        {
            using(var img = Image.FromFile(file))
            {
                using(var bitmap = new Bitmap(img))
                {
                    Generated = false;
                    if(Handle >= 0)
                    {
                        GL.DeleteTexture(Handle);
                    }
                    Size = bitmap.Size;
                    Bitmap = BitmapToByteArray(bitmap);
                }
            }
        }

        public void Use(TextureUnit unit)
        {
            if(!Generated)
            {
                Handle = GL.GenTexture();
                GL.Enable(EnableCap.TextureCubeMapSeamless);
                GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapPosX);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapPosY);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapPosZ);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapNegX);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapNegY);
                GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, Size.Width, Size.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapNegZ);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                FreeCPU();
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
            bitmap.UnlockBits(bmpdata);
            bitmap.Dispose();
            return bytedata;
        }
    }
}