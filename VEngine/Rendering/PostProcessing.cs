using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Renderer
    {
        public CubeMapTexture CubeMap;
        
        public MRTFramebuffer MRT;
        
        public int Width, Height;

        private ShaderProgram
            HDRShader;

        private bool DisablePostEffects = false;

        private Framebuffer
            DistanceFramebuffer;

        private Object3dInfo PostProcessingMesh;
        
        private float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
        
        public Renderer(int initialWidth, int initialHeight)
        {
            

            CubeMap = new CubeMapTexture(Media.Get("posx.jpg"), Media.Get("posy.jpg"), Media.Get("posz.jpg"),
                Media.Get("negx.jpg"), Media.Get("negy.jpg"), Media.Get("negz.jpg"));

            Width = initialWidth;
            Height = initialHeight;
            //   initialWidth *= 4; initialHeight *= 4;
            MRT = new MRTFramebuffer(initialWidth, initialHeight);
            
            DistanceFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = false,
                ColorInternalFormat = PixelInternalFormat.R32f,
                ColorPixelFormat = PixelFormat.Red,
                ColorPixelType = PixelType.Float
            };

            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            
            PostProcessingMesh = new Object3dInfo(VertexInfo.FromFloatArray(postProcessingPlaneVertices));
        }
        
        public void RenderToCubeMapFramebuffer(CubeMapFramebuffer framebuffer)
        {
            Game.World.Scene.RecreateSimpleLightsSSBO();
            Width = framebuffer.Width;
            Height = framebuffer.Height;

            // framebuffer.Use(true, true);
            var cam = Camera.Current;
            DisablePostEffects = true;
            FaceRender(framebuffer, TextureTarget.TextureCubeMapPositiveX);
            FaceRender(framebuffer, TextureTarget.TextureCubeMapPositiveY);
            FaceRender(framebuffer, TextureTarget.TextureCubeMapPositiveZ);

            FaceRender(framebuffer, TextureTarget.TextureCubeMapNegativeX);
            FaceRender(framebuffer, TextureTarget.TextureCubeMapNegativeY);
            FaceRender(framebuffer, TextureTarget.TextureCubeMapNegativeZ);
            DisablePostEffects = false;
            GL.Enable(EnableCap.DepthTest);
            Camera.Current = cam;
        }

        enum BlitMode
        {
            Color,
            Depth,
            ColorAndDepth
        }

        public void RenderToFramebuffer(Framebuffer framebuffer)
        {
            if(Camera.Current == null)
                return;
            Game.World.Scene.RecreateSimpleLightsSSBO();
            Width = framebuffer.Width;
            Height = framebuffer.Height;

            RenderPrepareToBlit();

            framebuffer.Use(false, false);
            GL.Viewport(0, 0, framebuffer.Width, framebuffer.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            HDR();
        }

        public void SetUniformsShared()
        {
            var shader = ShaderProgram.Current;
            //shader.SetUniform("ViewMatrix", Camera.Current.GetViewMatrix());
            //shader.SetUniform("ProjectionMatrix", Camera.Current.GetProjectionMatrix());
            shader.SetUniform("VPMatrix", Matrix4.Mult(Camera.Current.GetViewMatrix(), Camera.Current.GetProjectionMatrix()));
            Camera.Current.SetUniforms();

            shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
            shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
            shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
            shader.SetUniform("resolution", new Vector2(Width, Height));
            shader.SetUniform("DisablePostEffects", DisablePostEffects);
            shader.SetUniform("MSAASamples", Game.MSAASamples);
            shader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
        }
        
        
        private void DisableBlending()
        {
            GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
            GL.DepthMask(true);
        }

        private void DrawPPMesh()
        {
            SetUniformsShared();
            PostProcessingMesh.Draw();
        }

        private void EnableBlending()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthMask(false);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void FaceRender(CubeMapFramebuffer framebuffer, TextureTarget target)
        {
            GL.Enable(EnableCap.DepthTest);
            framebuffer.SwitchCamera(target);
            RenderPrepareToBlit();

            framebuffer.Use(true, false);
            framebuffer.SwitchFace(target);
            GL.Viewport(0, 0, framebuffer.Width, framebuffer.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            HDR();
            framebuffer.GenerateMipMaps();
        }
        
        private void HDR()
        {
            HDRShader.Use();
            HDRShader.SetUniform("UseBloom", Game.GraphicsSettings.UseBloom);
            HDRShader.SetUniform("InputFocalLength", Camera.Current.FocalLength);
            if(Camera.MainDisplayCamera != null)
            {
                HDRShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                HDRShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            //MSAAEdgeDetectFramebuffer.UseTexture(28);
            MRT.UseTextureForwardColor(1);
            DrawPPMesh();
        }
        
        private void RenderPrepareToBlit()
        {
            DistanceFramebuffer.Use();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.DistanceOnly;
            GL.ColorMask(true, false, false, false);
            InternalRenderingState.PassState = InternalRenderingState.State.DistancePass;
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GL.ColorMask(true, true, true, true);
            MRT.Use();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.DepthOnly;
            GL.ColorMask(false, false, false, false);
            InternalRenderingState.PassState = InternalRenderingState.State.EarlyZPass;
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
            CubeMap.Use(TextureUnit.Texture8);
            DistanceFramebuffer.UseTexture(9);
            //EnableBlending();
            GL.ColorMask(true, true, true, true);
            InternalRenderingState.PassState = InternalRenderingState.State.ForwardOpaquePass;
            DisableBlending();
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.ForwardTransparentPass;
            EnableBlending();
            Game.World.Draw();
            DisableBlending();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            // DisableBlending();
            
        }
        
        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
        }
        
    }
}