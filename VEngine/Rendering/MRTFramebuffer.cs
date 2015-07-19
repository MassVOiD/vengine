using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace VEngine
{
    public class MRTFramebuffer
    {
        public MRTFramebuffer(int width, int height)
        {
            Generated = false;
            Width = width;
            Height = height;
        }

        public int TexDiffuse, TexWorldPos, TexNormals, TexMeshData, TexDepth;

        private int FBO, RBO, Width, Height;
        public bool Generated;
        public PixelInternalFormat ColorInternalFormat = PixelInternalFormat.Rgba16f;
        public PixelFormat ColorPixelFormat = PixelFormat.Rgba;
        public PixelType ColorPixelType = PixelType.HalfFloat;
        public PixelInternalFormat DepthInternalFormat = PixelInternalFormat.DepthComponent32f;
        public PixelFormat DepthPixelFormat = PixelFormat.DepthComponent;
        public PixelType DepthPixelType = PixelType.Float;

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

        public void UseTextureDiffuseColor(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.Texture2D, TexDiffuse);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        public void UseTextureWorldPosition(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.Texture2D, TexWorldPos);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        public void UseTextureNormals(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.Texture2D, TexNormals);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        public void UseTextureMeshData(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.Texture2D, TexMeshData);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        public void UseTextureDepth(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.Texture2D, TexDepth);

            GL.ActiveTexture(TextureUnit.Texture0);
        }
        
        private void Generate()
        {
            Generated = true;

            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            // generating textures
            TexDiffuse = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexDiffuse);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            TexWorldPos = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexWorldPos);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.HalfFloat, (IntPtr)0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            TexNormals = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexNormals);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.HalfFloat, (IntPtr)0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            TexMeshData = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexMeshData);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            TexDepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TexDepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, (IntPtr)0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
            
            RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexDiffuse, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TexWorldPos, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TexNormals, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TexMeshData, 0);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TexDepth, 0);

            GL.DrawBuffers(4, new DrawBuffersEnum[]{DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3});

            if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer not complete");
            }
        }

    }
}