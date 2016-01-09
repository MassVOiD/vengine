using System;
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

        public int TexDiffuse, TexNormals, DepthRenderBuffer;

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

        public void UseTextureDiffuseColor(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(Game.MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexDiffuse);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        
        public void UseTextureNormals(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(Game.MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexNormals);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void Generate()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            // generating textures
            var ttarget = Game.MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D;
            TexDiffuse = GL.GenTexture();
            GL.BindTexture(ttarget, TexDiffuse);
            if(Game.MSAASamples > 1)
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Game.MSAASamples, PixelInternalFormat.Rgba8, Width, Height, true);
            else
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(ttarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(ttarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            TexNormals = GL.GenTexture();
            GL.BindTexture(ttarget, TexNormals);
            if(Game.MSAASamples > 1)
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, Game.MSAASamples, PixelInternalFormat.Rgba32f, Width, Height, true);
            else
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(ttarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(ttarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            
            // generating rbo for depth
            DepthRenderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRenderBuffer);
            if(Game.MSAASamples > 1)
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, Game.MSAASamples, RenderbufferStorage.DepthComponent32f, Width, Height);
            else
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, Width, Height);

            // attaching
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexDiffuse, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TexNormals, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthRenderBuffer);

            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

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