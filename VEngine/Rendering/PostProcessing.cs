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
            FogShader,
            AOShader,
            HDRShader,
            BlitShader;

        private bool DisablePostEffects = false;
        
        private Framebuffer
            DistanceFramebuffer,
            FogFramebuffer;

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
            
            FogFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            DistanceFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2)
            {
                ColorOnly = false,
                ColorInternalFormat = PixelInternalFormat.R32f,
                ColorPixelFormat = PixelFormat.Red,
                ColorPixelType = PixelType.Float
            };

            AOShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "AO.fragment.glsl");
            FogShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Fog.fragment.glsl");
            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            BlitShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Blit.fragment.glsl");
            
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
        

        // public static uint RandomIntFrame = 1;
        private void Blit(BlitMode mode)
        {
            BlitShader.Use();
            BlitShader.SetUniform("BlitMode", (int)mode);
            DrawPPMesh();
        }
        
        private void DisableBlending()
        {
            GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
        }

        private void DrawPPMesh()
        {
            SetUniformsShared();
            PostProcessingMesh.Draw();
        }

        private void EnableBlending()
        {
            GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
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

        private void Fog()
        {
            SwitchToFB(FogFramebuffer);
            FogShader.Use();
            Game.World.Scene.SetLightingUniforms(FogShader);
            FogShader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
           // MRT.UseTextureForwardColor(30);
            MRT.UseTextureNormals(2);
            DrawPPMesh();
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
            MRT.UseTextureForwardColor(30);
            MRT.UseTextureNormals(2);
            DrawPPMesh();
        }
        
        private void RenderPrepareToBlit()
        {
            DistanceFramebuffer.Use();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.DistanceOnly;
            GL.ColorMask(true, false, false, false);
            Game.World.Draw();
            GL.ColorMask(true, true, true, true);
            MRT.Use();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.DepthOnly;
            GL.ColorMask(false, false, false, false);
            Game.World.Draw();
            GenericMaterial.OverrideShaderPack = null;
            CubeMap.Use(TextureUnit.Texture3);
            DistanceFramebuffer.UseTexture(27);
            //EnableBlending();
            GL.ColorMask(true, true, true, true);
            Game.World.Draw();
           // DisableBlending();
                        
            if(Game.GraphicsSettings.UseFog)
            {
                Fog();
            }
        }
        
        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
        }
        
    }
}