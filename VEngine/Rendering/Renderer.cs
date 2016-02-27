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
        public class CubeMapInfo
        {
            public CubeMapFramebuffer Framebuffer;
            public Vector3 Position;
            public float FalloffScale;
        }

        public List<CubeMapInfo> CubeMaps = new List<CubeMapInfo>();
        
        public MRTFramebuffer MRT;
        
        public int Width, Height;

        private ShaderProgram
         //   BloomShader,
            HDRShader,
            ScreenSpaceReflectionsShader,
            DeferredShader;

       // public VoxelGI VXGI;

        public static bool DisablePostEffects = false;

        public Framebuffer
          //  BloomXPass, BloomYPass,
            DistanceFramebuffer,
            ScreenSpaceReflectionsFramebuffer,
            DeferredFramebuffer;

        private Object3dInfo PostProcessingMesh;
        
        private float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        public void Resize(int initialWidth, int initialHeight)
        {
            DistanceFramebuffer.FreeGPU();
            ScreenSpaceReflectionsFramebuffer.FreeGPU();
            DeferredFramebuffer.FreeGPU();
            MRT.FreeGPU();
            MRT = new MRTFramebuffer(initialWidth, initialHeight, Game.MSAASamples);

            DistanceFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.R32f,
                ColorPixelFormat = PixelFormat.Red,
                ColorPixelType = PixelType.Float
            };
            ScreenSpaceReflectionsFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            DeferredFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };

            Width = initialWidth;
            Height = initialHeight;
        }

        public Renderer(int initialWidth, int initialHeight, int samples)
        {
            

            //CubeMap = new CubeMapTexture(Media.Get("posx.jpg"), Media.Get("posy.jpg"), Media.Get("posz.jpg"),
            //    Media.Get("negx.jpg"), Media.Get("negy.jpg"), Media.Get("negz.jpg"));

            Width = initialWidth;
            Height = initialHeight;
           // VXGI = new VoxelGI();
            //   initialWidth *= 4; initialHeight *= 4;
            MRT = new MRTFramebuffer(initialWidth, initialHeight, samples);

            DistanceFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = false,
                ColorInternalFormat = PixelInternalFormat.R32f,
                ColorPixelFormat = PixelFormat.Red,
                ColorPixelType = PixelType.Float
            };
            ScreenSpaceReflectionsFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            DeferredFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            /*BloomXPass = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            BloomYPass = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };*/

            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            HDRShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                HDRShader.SetGlobal("USE_MSAA", "");

            ScreenSpaceReflectionsShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "ScreenSpaceReflections.fragment.glsl");
            ScreenSpaceReflectionsShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                ScreenSpaceReflectionsShader.SetGlobal("USE_MSAA", "");

            DeferredShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Deferred.fragment.glsl");
            DeferredShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                DeferredShader.SetGlobal("USE_MSAA", "");
            // BloomShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Bloom.fragment.glsl");

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

        public void SetCubemapsUniforms()
        {
            var shader = ShaderProgram.Current;
            bool res = shader.SetUniformArray("CubeMapsAddrs", CubeMaps.Select<CubeMapInfo, long>((a) => a.Framebuffer.GetBindlessHandle()).ToArray());
            if(res)
            {
                shader.SetUniform("CubeMapsCount", CubeMaps.Count);
                shader.SetUniformArray("CubeMapsPositions", CubeMaps.Select<CubeMapInfo, Vector4>((a) => new Vector4(a.Position, 1.0f)).ToArray());
                shader.SetUniformArray("CubeMapsFalloffs", CubeMaps.Select<CubeMapInfo, Vector4>((a) => new Vector4(a.FalloffScale)).ToArray());
            }
            else
            {
                shader.SetUniform("CubeMapsCount", 0);
            }
           // for(int i = 0; i < CubeMaps.Count; i++)
           //     CubeMaps[i].Framebuffer.UseTexture(9 + i);
        }

        public void SetUniformsShared()
        {
            var shader = ShaderProgram.Current;
            //shader.SetUniform("ViewMatrix", Camera.Current.GetViewMatrix());
            //shader.SetUniform("ProjectionMatrix", Camera.Current.GetProjectionMatrix());
            shader.SetUniform("VPMatrix", Matrix4.Mult(Camera.Current.GetViewMatrix(), Camera.Current.GetProjectionMatrix()));
            Camera.Current.SetUniforms();

            MRT.UseTextures(1, 2, 3);

            DeferredFramebuffer.UseTexture(7);
            DistanceFramebuffer.UseTexture(8);
            ScreenSpaceReflectionsFramebuffer.GenerateMipMaps();
            ScreenSpaceReflectionsFramebuffer.UseTexture(9);

            GenericMaterial.UseBuffer(7);

            //SetCubemapsUniforms();

            shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
            shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
            shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
            shader.SetUniform("resolution", new Vector2(Width, Height));
            shader.SetUniform("DisablePostEffects", DisablePostEffects);
            shader.SetUniform("Brightness", Camera.MainDisplayCamera.Brightness);
            shader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
            shader.SetUniform("UseVDAO", Game.GraphicsSettings.UseVDAO);
            shader.SetUniform("UseHBAO", Game.GraphicsSettings.UseHBAO);
            shader.SetUniform("UseFog", Game.GraphicsSettings.UseFog);
            shader.SetUniform("Brightness", Camera.MainDisplayCamera.Brightness);
            shader.SetUniform("VDAOGlobalMultiplier", 1.0f);
            shader.SetUniform("CurrentlyRenderedCubeMap", Game.World.CurrentlyRenderedCubeMap);
            Game.World.Scene.SetLightingUniforms(shader);
            //RandomsSSBO.Use(0);
            SetCubemapsUniforms();
            Game.World.Scene.MapLightsSSBOToShader(shader);
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
            //framebuffer.GenerateMipMaps();
        }

        private void Deferred()
        {
            DeferredShader.Use();
            DeferredFramebuffer.Use();
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
            DrawPPMesh();
        }
        private void ScreenSpaceReflections()
        {
            ScreenSpaceReflectionsShader.Use();
            ScreenSpaceReflectionsFramebuffer.Use();
            DeferredFramebuffer.UseTexture(7);
            DrawPPMesh();
        }
        private void Bloom()
        {
        /*    BloomXPass.Use();
            BloomShader.Use();
            BloomShader.SetUniform("Pass", 0);
            MRT.UseTextureForwardColor(1);
            DrawPPMesh();
            BloomYPass.Use();
            BloomShader.SetUniform("Pass", 1);
            BloomXPass.UseTexture(2);
            DrawPPMesh();*/
        }

        private void RenderPrepareToBlit()
        {
            Game.World.SetUniforms(this);
            DistanceFramebuffer.Use();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderDistance();
            GL.ColorMask(true, false, false, false);
            DisableBlending();
            InternalRenderingState.PassState = InternalRenderingState.State.DistancePass;
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GL.ColorMask(true, true, true, true);
            MRT.Use();
            DistanceFramebuffer.GenerateMipMaps();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderDepth();
            GL.ColorMask(false, false, false, false);
            InternalRenderingState.PassState = InternalRenderingState.State.EarlyZPass;
           // Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
            //CubeMap.Use(TextureUnit.Texture8);
            DistanceFramebuffer.UseTexture(8);
            //EnableBlending();
            GL.ColorMask(true, true, true, true);
            InternalRenderingState.PassState = InternalRenderingState.State.ForwardOpaquePass;
            DisableBlending();
            Game.World.Draw();
            //InternalRenderingState.PassState = InternalRenderingState.State.ForwardTransparentPass;
            //EnableBlending();
            //Game.World.Draw();
            //DisableBlending();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            // DisableBlending();
            //Bloom();

            Deferred();
            ScreenSpaceReflections();

        }
        
        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
        }
        
    }
}