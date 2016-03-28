using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Texture3D
    {
        public int Handle = -1;

        private bool Generated;

        public int SizeX, SizeY, SizeZ;

        public Texture3D(int handle)
        {
            Handle = handle;
            Generated = true;
        }

        public Texture3D(int sizex, int sizey, int sizez)
        {
            SizeX = sizex;
            SizeY = sizey;
            SizeZ = sizez;
            Generated = false;
        }

        public void Clear()
        {
            GL.ClearTexImage(Handle, 0, ColorPixelFormat, ColorPixelType, IntPtr.Zero);
        }

        public PixelInternalFormat ColorInternalFormat = PixelInternalFormat.R32ui;
        public PixelFormat ColorPixelFormat = PixelFormat.RedInteger;
        public PixelType ColorPixelType = PixelType.UnsignedInt;

        public void Use(TextureUnit unit)
        {
            Generate();
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture3D, Handle);
        }
        
        public void BindImageUnit(int unit, TextureAccess access, SizedInternalFormat format)
        {
            Generate();
            GL.BindImageTexture(unit, Handle, 0, true, 0, access, format);
        }

        public void FreeImageUnit(int unit)
        {
            GL.BindImageTexture(unit, 0, 0, false, 0, 0, 0);
        }

        private void Generate()
        {
            if(!Generated)
            {
                Handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture3D, Handle);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);

                GL.TexImage3D(TextureTarget.Texture3D, 0, ColorInternalFormat, SizeX, SizeY, SizeZ, 0, ColorPixelFormat, ColorPixelType, IntPtr.Zero);
                
                Generated = true;
            }
        }
    }
}