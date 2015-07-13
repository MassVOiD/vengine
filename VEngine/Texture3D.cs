using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace VEngine
{
    public class Texture3D
    {


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

        private bool Generated;
        public int Handle = -1;
        private Vector3 Size;

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

        public void Clear()
        {
            GL.ClearTexImage(Handle, 0, PixelFormat.RedInteger, PixelType.UnsignedInt, new IntPtr(0));
        }

    }
}
