using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class MRTFramebuffer
    {
        public bool Generated;

        public int TexAlbedoRoughness, TexNormalsDistance, TexSpecularBump, DepthRenderBuffer;

        private int FBO, Width, Height, MSAASamples = 1;

        public MRTFramebuffer(int width, int height, int samples)
        {
            Generated = false;
            Width = width;
            Height = height;
            MSAASamples = samples;
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

        public void UseTextures(int indexAlbedoRoughness, int indexNormalsDistance, int indexSpecularBump)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + indexAlbedoRoughness);
            GL.BindTexture(MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexAlbedoRoughness);

            GL.ActiveTexture(TextureUnit.Texture0 + indexNormalsDistance);
            GL.BindTexture(MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexNormalsDistance);

            GL.ActiveTexture(TextureUnit.Texture0 + indexSpecularBump);
            GL.BindTexture(MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexSpecularBump);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void GenerateMipMaps()
        {
            GL.BindTexture(MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexAlbedoRoughness);
            GL.GenerateMipmap(MSAASamples > 1 ? GenerateMipmapTarget.Texture2DMultisample : GenerateMipmapTarget.Texture2D);
            
            GL.BindTexture(MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexNormalsDistance);
            GL.GenerateMipmap(MSAASamples > 1 ? GenerateMipmapTarget.Texture2DMultisample : GenerateMipmapTarget.Texture2D);

            GL.BindTexture(MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, TexSpecularBump);
            GL.GenerateMipmap(MSAASamples > 1 ? GenerateMipmapTarget.Texture2DMultisample : GenerateMipmapTarget.Texture2D);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        public void FreeGPU()
        {

            if(TexAlbedoRoughness > -1)
                GL.DeleteTexture(TexAlbedoRoughness);
            if(TexNormalsDistance > -1)
                GL.DeleteTexture(TexNormalsDistance);
            if(TexSpecularBump > -1)
                GL.DeleteTexture(TexSpecularBump);
            if(DepthRenderBuffer > -1)
                GL.DeleteRenderbuffer(DepthRenderBuffer);
            if(FBO > 0)
                GL.DeleteFramebuffer(FBO);
        }

        private int GenerateSingleTexture(PixelInternalFormat pif, PixelFormat pf, PixelType pt)
        {
            var ttarget = MSAASamples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D;
            int id = GL.GenTexture();
            GL.BindTexture(ttarget, id);
            if(MSAASamples > 1)
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, MSAASamples, pif, Width, Height, true);
            else
                GL.TexImage2D(TextureTarget.Texture2D, 0, pif, Width, Height, 0, pf, pt, IntPtr.Zero);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(ttarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(ttarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(ttarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            return id;
        }
        
        private void Generate()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            // generating textures
            TexAlbedoRoughness = GenerateSingleTexture(PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte);
            TexNormalsDistance = GenerateSingleTexture(PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            TexSpecularBump = GenerateSingleTexture(PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte);

            // generating rbo for depth
            DepthRenderBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthRenderBuffer);
            if(MSAASamples > 1)
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, MSAASamples, RenderbufferStorage.DepthComponent24, Width, Height);
            else
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, Width, Height);

            // attaching
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexAlbedoRoughness, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TexNormalsDistance, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TexSpecularBump, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthRenderBuffer);

            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });

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