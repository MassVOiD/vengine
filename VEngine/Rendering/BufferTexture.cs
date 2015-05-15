using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class BufferTexture
    {
        public BufferTexture(byte[] data, int width, int height)
        {
            Update(data, width, height);
        }

        private byte[] Data;
        private bool Generated;
        private int Handle = -1;
        private int Height, Width;

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