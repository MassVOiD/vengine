using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OldGL = OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace VDGTech
{
    public class BufferTexture
    {
        int Handle = -1;
        bool Generated;
        byte[] Data;
        int Height, Width;
        public BufferTexture(byte[] data, int width, int height)
        {
            Update(data, width, height);
        }

        public void Update(byte[] data, int width, int height)
        {
            Generated = false;
            if(Handle >= 0)
            {
                GL.DeleteTexture(Handle);
            }
            Data = data;
            Width = width;
            Height = height;
        }

        public void Use(TextureUnit unit)
        {
            if(!Generated)
            {
                Handle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, Handle);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, Data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
             
                Generated = true;
            }
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        /*public static BufferTexture CreateFromVector3List(List<Vector3> vectors)
        {

        }*/

    }
}
