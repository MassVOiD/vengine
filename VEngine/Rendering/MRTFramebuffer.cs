using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class MRTFramebuffer
    {
        public PixelInternalFormat ColorInternalFormat = PixelInternalFormat.Rgba16f;

        public PixelFormat ColorPixelFormat = PixelFormat.Rgba;

        public PixelType ColorPixelType = PixelType.HalfFloat;

        public PixelInternalFormat DepthInternalFormat = PixelInternalFormat.DepthComponent32f;

        public PixelFormat DepthPixelFormat = PixelFormat.DepthComponent;

        public PixelType DepthPixelType = PixelType.Float;

        public bool Generated;

        public int TexForward, DepthRenderBuffer;

        private int FBO, Width, Height;

        public MRTFramebuffer(int width, int height)
        {
            Generated = false;
            Width = width;
            Height = height;
        }

        public void RevertToDefault()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public Vector4h[] GetImageForward()
        {
            GL.BindTexture(TextureTarget.Texture2D, TexForward);
            var pixels = new OpenTK.Vector4h[Width * Height];
            GL.GetTexImage<Vector4h>(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.HalfFloat, pixels);
            return pixels;
        }

        public void Use(bool setViewport = true, bool clearViewport = true)
        {
            if(!Generated)
                Generate();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            if(setViewport)
                GL.Viewport(0, 0, Width, Height);
            if(clearViewport)
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void UseTextureForwardColor(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(Game.MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexForward);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        
        private void Generate()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            // generating textures
            var ttarget = Game.MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D;
            TexForward = GL.GenTexture();
            GL.BindTexture(ttarget, TexForward);
            if(Game.MSAASamples > 1)
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Game.MSAASamples, PixelInternalFormat.Rgba8, Width, Height, true);
            else
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(ttarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(ttarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            
            // generating rbo for depth
            DepthRenderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRenderBuffer);
            if(Game.MSAASamples > 1)
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Game.MSAASamples, RenderbufferStorage.DepthComponent24, Width, Height);
            else
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);

            // attaching
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexForward, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthRenderBuffer);

            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

            // check for fuckups
            var err = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if(err != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer not complete");
            }
            Generated = true;
        }
    }
}