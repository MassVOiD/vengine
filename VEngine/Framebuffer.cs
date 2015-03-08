using System;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class Framebuffer
    {
        public Framebuffer(int width, int height, bool depthOnly = false)
        {
            Generated = false;
            Width = width;
            Height = height;
            DepthOnly = depthOnly;
        }

        public int TexColor, TexDepth;
        private int FBO, RBO, Width, Height;
        private bool Generated, DepthOnly;

        public float GetDepth(float x, float y)
        {
            GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            float[] pixels = new float[Width * Height];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.DepthComponent, PixelType.Float, pixels);
            return pixels[(int)(Width * x) + (int)((Height * y) * Width)];
        }

        public void RevertToDefault()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Use()
        {
            if(!Generated)
                Generate();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        }

        public void UseTexture(int startIndex)
        {
            if(DepthOnly)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
                GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
                GL.BindTexture(TextureTarget.Texture2D, TexColor);
                GL.ActiveTexture(TextureUnit.Texture1 + startIndex);
                GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            }
            // this is because somebody recommended it on stackoverflow
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void Generate()
        {
            Generated = true;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            TexColor = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexColor);
            if(DepthOnly)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Width, Height, 0, PixelFormat.Red, PixelType.Byte, (IntPtr)0);
            }
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, (IntPtr)0);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            TexDepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            if(DepthOnly)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
            RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO);
            if(!DepthOnly)
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexColor, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TexDepth, 0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer not complete");
            }
        }
    }
}