using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Framebuffer
    {
        public PixelInternalFormat ColorInternalFormat = PixelInternalFormat.Rgba16f;

        public PixelFormat ColorPixelFormat = PixelFormat.Rgba;

        public PixelType ColorPixelType = PixelType.HalfFloat;

        public PixelInternalFormat DepthInternalFormat = PixelInternalFormat.DepthComponent32f;

        public bool DepthOnly, MultiSample, ColorOnly;
        public PixelFormat DepthPixelFormat = PixelFormat.DepthComponent;

        public PixelType DepthPixelType = PixelType.Float;
        
        public bool Generated;

        public int TexColor, TexDepth = -1;

        public int Width, Height;

        private static Framebuffer DefaultF;

        private int FBO, RBO;

        private bool Empty = false;

        public Framebuffer(int width, int height, bool depthOnly = false)
        {
            Generated = false;
            ColorOnly = false;
            Width = width;
            Height = height;
            DepthOnly = depthOnly;
        }
        public Framebuffer()
        {
            Generated = false;
            ColorOnly = false;
            Empty = true;
            Width = 0;
            Height = 0;
            DepthOnly = true;
        }

        public static Framebuffer Default
        {
            get
            {
                if(DefaultF == null || DefaultF.Width != Game.Resolution.Width || DefaultF.Height != Game.Resolution.Height)
                    DefaultF = new Framebuffer(Game.Resolution.Width, Game.Resolution.Height)
                    {
                        Generated = true,
                        FBO = 0
                    };
                return DefaultF;
            }
        }

        public void GenerateMipMaps()
        {
            GL.BindTexture(TextureTarget.Texture2D, TexColor);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
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

        public void SetMultiSample(bool boolean)
        {
            if(Generated)
                throw new Exception("Framebuffer already generated, too late");
            MultiSample = boolean;
        }

        public void Use(bool setViewport = true, bool clearViewport = true)
        {
            if(!Generated)
                Generate();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            if(setViewport)
                GL.Viewport(0, 0, Width, Height);
            if(clearViewport)
            {
                if(ColorOnly) GL.Clear(ClearBufferMask.ColorBufferBit);
                else GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
        }

        public void BindWithPurpose(FramebufferTarget purpose)
        {
            GL.BindFramebuffer(purpose, FBO);
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
                    if(!ColorOnly)
                    {
                        GL.ActiveTexture(TextureUnit.Texture1 + startIndex);
                        GL.BindTexture(TextureTarget.Texture2D, TexDepth);
                    }
                }
            }
        }

        private void Generate()
        {
            Generated = true;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            if(!Empty)
            {
                if(DepthOnly)
                {
                    // GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Width, Height,
                    // 0, PixelFormat.Red, PixelType.Byte, (IntPtr)0);
                }
                else
                {
                    TexColor = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, TexColor);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, ColorInternalFormat, Width, Height, 0, ColorPixelFormat, ColorPixelType, (IntPtr)0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                if(!ColorOnly)
                {
                    TexDepth = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, TexDepth);
                    if(DepthOnly)
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, (IntPtr)0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                        //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
                    }
                    else
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, (IntPtr)0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    }
                }
                // RBO = GL.GenRenderbuffer(); GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer,
                // RBO); GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer,
                // RenderbufferStorage.DepthComponent, Width, Height);

                // GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                // FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO);

                if(!DepthOnly)
                    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexColor, 0);

                if(!ColorOnly)
                    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TexDepth, 0);

                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

                if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                {
                    throw new Exception("Framebuffer not complete");
                }
            }
        }

        public void FreeGPU()
        {
            if(TexColor > -1)
                GL.DeleteTexture(TexColor);
            if(TexDepth > -1)
                GL.DeleteTexture(TexDepth);
            if(FBO > 0)
                GL.DeleteFramebuffer(FBO);
        }

    }
}