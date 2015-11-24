using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    internal class CubeMapFramebuffer : ITransformable
    {
        public int Resolution;
        public TransformationManager Transformation;

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
        public PixelInternalFormat ColorInternalFormat = PixelInternalFormat.Rgba16f;

        public PixelFormat ColorPixelFormat = PixelFormat.Rgba;

        public PixelType ColorPixelType = PixelType.HalfFloat;

        public PixelInternalFormat DepthInternalFormat = PixelInternalFormat.DepthComponent32f;

        public PixelFormat DepthPixelFormat = PixelFormat.DepthComponent;

        public PixelType DepthPixelType = PixelType.Float;

        public int DrawBufferIndex = 0;

        public bool Generated;

        public int TexColor, TexDepth;

        public int Width, Height;

        public bool DepthOnly, MultiSample, ColorOnly;

        private int FBO, RBO;

        public CubeMapFramebuffer(int width, int height, bool depthOnly = false)
        {
            Generated = false;
            ColorOnly = false;
            Width = width;
            Height = height;
            DepthOnly = depthOnly;
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
            if(setViewport)
                GL.Viewport(0, 0, Width, Height);
           // if(clearViewport)
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void UseTexture(int startIndex)
        {
            if(DepthOnly)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
                GL.BindTexture(TextureTarget.TextureCubeMap, TexDepth);
            }
            else
            {
               
                GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
                GL.BindTexture(TextureTarget.TextureCubeMap, TexColor);
                if(!ColorOnly)
                {
                    GL.ActiveTexture(TextureUnit.Texture1 + startIndex);
                    GL.BindTexture(TextureTarget.TextureCubeMap, TexDepth);
                }
                
            }
        }

        private void Generate()
        {
            Generated = true;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            
                TexColor = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, TexColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX, TexColor, 0);

            GL.DrawBuffer( DrawBufferMode.ColorAttachment0);

            if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer not complete");
            }
        }

        void SwitchFace(TextureTarget face)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, face, TexColor, 0);
        }

    }
}