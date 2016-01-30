using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class CubeMapFramebuffer : ITransformable
    {
        public PixelInternalFormat ColorInternalFormat = PixelInternalFormat.Rgba16f;
        public PixelFormat ColorPixelFormat = PixelFormat.Rgba;
        public PixelType ColorPixelType = PixelType.HalfFloat;
        public PixelInternalFormat DepthInternalFormat = PixelInternalFormat.DepthComponent32f;
        public PixelFormat DepthPixelFormat = PixelFormat.DepthComponent;
        public PixelType DepthPixelType = PixelType.Float;
        public int DrawBufferIndex = 0;
        public bool Generated;
        public int Resolution;
        public int TexColor, TexDepth;
        public TransformationManager Transformation;

        public int Width, Height;

        private Dictionary<TextureTarget, Camera> FacesCameras;

        private int FBO;

        public CubeMapFramebuffer(int width, int height, bool depthOnly = false)
        {
            Transformation = new TransformationManager(Vector3.Zero);
            Generated = false;
            Width = width;
            Height = height;
        }

        public void GenerateMipMaps()
        {
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            GL.BindTexture(TextureTarget.TextureCubeMap, TexColor);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public void RevertToDefault()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void SwitchCamera(TextureTarget face)
        {
            if(!Generated)
                Generate();
            var proj = FacesCameras[face].GetProjectionMatrix();
            FacesCameras[face].Transformation.SetPosition(Transformation.GetPosition());
            FacesCameras[face].SetProjectionMatrix(proj);
            Camera.Current = FacesCameras[face];
        }

        public void SwitchFace(TextureTarget face)
        {
            if(!Generated)
                Generate();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, face, TexColor, 0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
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

        public void UseTexture(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.TextureCubeMap, TexColor);
        }

        private void Generate()
        {
            Generated = true;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            GL.Enable(EnableCap.TextureCubeMapSeamless);
            TexColor = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, TexColor);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX, TexColor, 0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer not complete");
            }

            FacesCameras = new Dictionary<TextureTarget, Camera>();
            FacesCameras.Add(TextureTarget.TextureCubeMapPositiveX, new Camera(Vector3.Zero, new Vector3(1, 0, 0), -Vector3.UnitY, (float)Width / Height, MathHelper.DegreesToRadians(90.0f), 0.1f, 10000.0f));
            FacesCameras.Add(TextureTarget.TextureCubeMapPositiveY, new Camera(Vector3.Zero, new Vector3(0, 1, 0), Vector3.UnitZ, (float)Width / Height, MathHelper.DegreesToRadians(90.0f), 0.1f, 10000.0f));
            FacesCameras.Add(TextureTarget.TextureCubeMapPositiveZ, new Camera(Vector3.Zero, new Vector3(0, 0, 1), -Vector3.UnitY, (float)Width / Height, MathHelper.DegreesToRadians(90.0f), 0.1f, 10000.0f));

            FacesCameras.Add(TextureTarget.TextureCubeMapNegativeX, new Camera(Vector3.Zero, new Vector3(-1, 0, 0), -Vector3.UnitY, (float)Width / Height, MathHelper.DegreesToRadians(90.0f), 0.1f, 10000.0f));
            FacesCameras.Add(TextureTarget.TextureCubeMapNegativeY, new Camera(Vector3.Zero, new Vector3(0, -1, 0), -Vector3.UnitZ, (float)Width / Height, MathHelper.DegreesToRadians(90.0f), 0.1f, 10000.0f));
            FacesCameras.Add(TextureTarget.TextureCubeMapNegativeZ, new Camera(Vector3.Zero, new Vector3(0, 0, -1), -Vector3.UnitY, (float)Width / Height, MathHelper.DegreesToRadians(90.0f), 0.1f, 10000.0f));
        }
    }
}