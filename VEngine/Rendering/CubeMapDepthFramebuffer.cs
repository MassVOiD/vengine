using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class CubeMapDepthFramebuffer : ITransformable
    {
        public PixelInternalFormat DepthInternalFormat = PixelInternalFormat.DepthComponent32f;
        public PixelFormat DepthPixelFormat = PixelFormat.DepthComponent;
        public PixelType DepthPixelType = PixelType.Float;

        public int DrawBufferIndex = 0;
        public bool Generated;
        private int TexDepth;

        public int Width, Height;

        public TransformationManager Transformation;

        private Dictionary<TextureTarget, Camera> FacesCameras;

        private int FBO;

        public CubeMapDepthFramebuffer(int width, int height)
        {
            Generated = false;
            Width = width;
            Height = height;
            Transformation = new TransformationManager(Vector3.Zero);
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public void SwitchCamera(TextureTarget face)
        {
            if(!Generated)
                Generate();
            var proj = FacesCameras[face].GetProjectionMatrix();
            FacesCameras[face].Transformation.SetPosition(Transformation.GetPosition());
            FacesCameras[face].Update();
            Camera.Current = FacesCameras[face];
        }

        public void SwitchFace(TextureTarget face)
        {
            if(!Generated)
                Generate();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, face, TexDepth, 0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        }

        public void SwitchCameraAndFace(TextureTarget face)
        {
            SwitchCamera(face);
            SwitchFace(face);
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

        public void Clear()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            SwitchFace(TextureTarget.TextureCubeMapPositiveX);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwitchFace(TextureTarget.TextureCubeMapPositiveY);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwitchFace(TextureTarget.TextureCubeMapPositiveZ);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwitchFace(TextureTarget.TextureCubeMapNegativeX);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwitchFace(TextureTarget.TextureCubeMapNegativeY);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            SwitchFace(TextureTarget.TextureCubeMapNegativeZ);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void UseTexture(int startIndex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + startIndex);
            GL.BindTexture(TextureTarget.TextureCubeMap, TexDepth);
        }

        private void Generate()
        {
            Generated = true;
            TexDepth = GL.GenTexture();


            GL.BindTexture(TextureTarget.TextureCubeMap, TexDepth);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, IntPtr.Zero);
            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, DepthInternalFormat, Width, Height, 0, DepthPixelFormat, DepthPixelType, IntPtr.Zero);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
            
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);

            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.TextureCubeMapPositiveX, TexDepth, 0);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);

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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

        public void UpdateFarPlane(float far)
        {
            if(!Generated)
                Generate();
            foreach(var f in FacesCameras)
            {
                f.Value.UpdatePerspective(1, MathHelper.DegreesToRadians(90.0f), 0.01f, far);
            }
        }
    }
}
