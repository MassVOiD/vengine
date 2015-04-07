using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

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
        private bool DepthOnly, MultiSample;
        public bool Generated;

        public float GetDepth(float x, float y)
        {
            GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            float[] pixels = new float[Width * Height];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.DepthComponent, PixelType.Float, pixels);
            return pixels[(int)(Width * x) + (int)((Height * y) * Width)];
        }
        public Vector3h GetColor(float x, float y)
        {
            GL.BindTexture(TextureTarget.Texture2D, TexColor);
            Vector3h[] pixels = new Vector3h[Width * Height];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.HalfFloat, pixels);
            return pixels[(int)(Width * x) + (int)((Height * y) * Width)];
        }
        public Vector3h[] GetColorBuffer()
        {
            GL.BindTexture(TextureTarget.Texture2D, TexColor);
            Vector3h[] pixels = new Vector3h[Width * Height];
            GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgb, PixelType.HalfFloat, pixels);
            return pixels;
        }

        public void SetMultiSample(bool boolean)
        {
            if(Generated)
                throw new Exception("Framebuffer already generated, too late");
            MultiSample = boolean;
        }

        public void RevertToDefault()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Use(bool setViewport = true, bool clearViewport = true)
        {
            if(!Generated)
                Generate();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            if(setViewport) GL.Viewport(0, 0, Width, Height);
            if(clearViewport) GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
                if(MultiSample)
                {

                    GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
                    GL.BindTexture(TextureTarget.Texture2DMultisample, TexColor);
                    GL.ActiveTexture(TextureUnit.Texture1 + startIndex);
                    GL.BindTexture(TextureTarget.Texture2DMultisample, TexDepth);
                }
                else
                {

                    GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
                    GL.BindTexture(TextureTarget.Texture2D, TexColor);
                    GL.ActiveTexture(TextureUnit.Texture1 + startIndex);
                    GL.BindTexture(TextureTarget.Texture2D, TexDepth);
                }
            }
            // this is because somebody recommended it on stackoverflow
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void Generate()
        {
            Generated = true;
            if(MultiSample)
            {
                GenerateMultisampled(4);
                return;
            }
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            if(DepthOnly)
            {
               // GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Width, Height, 0, PixelFormat.Red, PixelType.Byte, (IntPtr)0);
            }
            else
            {
                TexColor = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, TexColor);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.HalfFloat, (IntPtr)0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            TexDepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            if(DepthOnly)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            }
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
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

        void GenerateMultisampled(int Samples)
        {

            // Generate multisampled textures
            TexColor = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, TexColor);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.Rgba16f, Width, Height, false);

            TexDepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, TexDepth);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Samples, PixelInternalFormat.DepthComponent32f, Width, Height, false);

            //create fbo
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Samples, RenderbufferStorage.DepthComponent, Width, Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO);

            // attach
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