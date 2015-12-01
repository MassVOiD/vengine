using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Texture3D
    {
        public int Handle = -1;

        public Texture3D(int handle)
        {
            Handle = handle;
            Generated = true;
        }

        public Texture3D(Vector3 size)
        {
            Size = size;
            Generated = false;
        }

        public void Clear()
        {
            GL.ClearTexImage(Handle, 0, PixelFormat.RedInteger, PixelType.UnsignedInt, new IntPtr(0));
        }

        public void Use(TextureUnit unit)
        {
            if(!Generated)
            {
                Handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture3D, Handle);

                GL.TexImage3D(TextureTarget.Texture3D, 0, PixelInternalFormat.R32ui, (int)Size.X, (int)Size.Y, (int)Size.Z, 0, OpenTK.Graphics.OpenGL4.PixelFormat.RedInteger, PixelType.UnsignedInt, new IntPtr(0));
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
                Generated = true;
            }
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture3D, Handle);
        }

        private bool Generated;

        private Vector3 Size;
    }
}