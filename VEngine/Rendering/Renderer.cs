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
            public CubeMapTexture Texture;
            public Vector3 Position;
            public float FalloffScale;
        }

        public List<CubeMapInfo> CubeMaps = new List<CubeMapInfo>();

        public MRTFramebuffer MRT;

        public int Width, Height, Samples;

        public GraphicsSettings GraphicsSettings = new GraphicsSettings();

        private ShaderProgram
            BloomShader,
            HDRShader,
            ScreenSpaceReflectionsShader,
            EnvLightShader,
            DeferredShader,
            AmbientOcclusionShader,
            LensDistortionShader,
            FogShader;

        // public VoxelGI VXGI;

        public static bool DisablePostEffects = false;

        public Framebuffer
            BloomXPass, BloomYPass,
            ScreenSpaceReflectionsFramebuffer,
            EnvLightFramebuffer,
            DeferredFramebuffer,
            AmbientOcclusionFramebuffer,
            ForwardPassFramebuffer,
            HelperFramebuffer,
            FogFramebuffer;

        private Object3dInfo PostProcessingMesh;

        private float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        private Object3dInfo CubeMapSphere;

        private Texture GlareTexture;

        public void Resize(int initialWidth, int initialHeight)
        {
            ScreenSpaceReflectionsFramebuffer.FreeGPU();
            DeferredFramebuffer.FreeGPU();
            BloomXPass.FreeGPU();
            BloomYPass.FreeGPU();
            EnvLightFramebuffer.FreeGPU();
            AmbientOcclusionFramebuffer.FreeGPU();
            ForwardPassFramebuffer.FreeGPU();
            FogFramebuffer.FreeGPU();

            MRT.FreeGPU();

            Width = initialWidth;
            Height = initialHeight;

            CreateBuffers();
        }

        public Renderer(int initialWidth, int initialHeight, int samples)
        {
            GlareTexture = new Texture(Media.Get("glaretex.png"));

            CubeMapSphere = new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get("cubemapsphere.obj")).Vertices);

            var cubeMapTexDefault = new CubeMapTexture(Media.Get("posx.jpg"), Media.Get("posy.jpg"), Media.Get("posz.jpg"),
                Media.Get("negx.jpg"), Media.Get("negy.jpg"), Media.Get("negz.jpg"));
            CubeMaps.Add(new CubeMapInfo()
            {
                Texture = cubeMapTexDefault,
                FalloffScale = 99999999.0f,
                Position = Vector3.Zero
            });

            Width = initialWidth;
            Height = initialHeight;


            Samples = samples;

            CreateBuffers();

            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            HDRShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                HDRShader.SetGlobal("USE_MSAA", "");

            ScreenSpaceReflectionsShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "ScreenSpaceReflections.fragment.glsl");
            ScreenSpaceReflectionsShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                ScreenSpaceReflectionsShader.SetGlobal("USE_MSAA", "");

            AmbientOcclusionShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "AmbientOcclusion.fragment.glsl");
            AmbientOcclusionShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                AmbientOcclusionShader.SetGlobal("USE_MSAA", "");

            FogShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Fog.fragment.glsl");
            FogShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                FogShader.SetGlobal("USE_MSAA", "");

            DeferredShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Deferred.fragment.glsl");
            DeferredShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                DeferredShader.SetGlobal("USE_MSAA", "");

            EnvLightShader = ShaderProgram.Compile("PostProcessPerspective.vertex.glsl", "EnvironmentLight.fragment.glsl");
            EnvLightShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                EnvLightShader.SetGlobal("USE_MSAA", "");

            LensDistortionShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "LensDistortion.fragment.glsl");
            LensDistortionShader.SetGlobal("MSAA_SAMPLES", samples.ToString());
            if(samples > 1)
                LensDistortionShader.SetGlobal("USE_MSAA", "");

            BloomShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Bloom.fragment.glsl");

            PostProcessingMesh = new Object3dInfo(VertexInfo.FromFloatArray(postProcessingPlaneVertices));
            PostProcessingMesh.DrawMode = PrimitiveType.TriangleStrip;
        }

        private void CreateBuffers()
        {

            MRT = new MRTFramebuffer(Width, Height, Samples);

            ScreenSpaceReflectionsFramebuffer = new Framebuffer(Width / 1, Height / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            DeferredFramebuffer = new Framebuffer(Width / 1, Height / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            HelperFramebuffer = new Framebuffer(Width / 1, Height / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            ForwardPassFramebuffer = new Framebuffer(Width / 1, Height / 1)
            {
                ColorOnly = false,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            EnvLightFramebuffer = new Framebuffer(Width / 1, Height / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            BloomXPass = new Framebuffer(Width / 4, Height / 4)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            BloomYPass = new Framebuffer(Width / 4, Height / 4)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            AmbientOcclusionFramebuffer = new Framebuffer(Width / 1, Height / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.R8,
                ColorPixelFormat = PixelFormat.Red,
                ColorPixelType = PixelType.UnsignedByte
            };
            FogFramebuffer = new Framebuffer(Width / 2, Height / 2)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba8,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.UnsignedByte
            };
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

        private void BlitFramebuffers(Framebuffer source, Framebuffer destination, BlitMode mode)
        {
            source.BindWithPurpose(FramebufferTarget.ReadFramebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, source.TexColor, 0);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            destination.BindWithPurpose(FramebufferTarget.DrawFramebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, destination.TexColor, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            if(mode == BlitMode.Color)
            {
                GL.BlitFramebuffer(0, 0, source.Width, source.Height, 0, 0, destination.Width, destination.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            }
            else if(mode == BlitMode.Depth)
            {
                GL.BlitFramebuffer(0, 0, source.Width, source.Height, 0, 0, destination.Width, destination.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Linear);
            }
            else if(mode == BlitMode.ColorAndDepth)
            {
                GL.BlitFramebuffer(0, 0, source.Width, source.Height, 0, 0, destination.Width, destination.Height, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Linear);
            }
        }

        private void BlitFramebuffers(MRTFramebuffer source, Framebuffer destination, BlitMode mode)
        {
            source.BindWithPurpose(FramebufferTarget.ReadFramebuffer);
            //GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, source.DepthRenderBuffer, 0);
            GL.ReadBuffer(ReadBufferMode.None);

            destination.BindWithPurpose(FramebufferTarget.DrawFramebuffer);
            //GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, destination.TexColor, 0);
            GL.DrawBuffer(DrawBufferMode.None);

            if(mode == BlitMode.Color)
            {
                GL.BlitFramebuffer(0, 0, source.Width, source.Height, 0, 0, destination.Width, destination.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            }
            else if(mode == BlitMode.Depth)
            {
                GL.BlitFramebuffer(0, 0, source.Width, source.Height, 0, 0, destination.Width, destination.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Linear);
            }
            else if(mode == BlitMode.ColorAndDepth)
            {
                GL.BlitFramebuffer(0, 0, source.Width, source.Height, 0, 0, destination.Width, destination.Height, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Linear);
            }
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

        private Matrix4 LastViewMatrix = Matrix4.Identity;

        public void SetUniformsShared()
        {
            var shader = ShaderProgram.Current;
            //shader.SetUniform("ViewMatrix", Camera.Current.GetViewMatrix());
            //shader.SetUniform("ProjectionMatrix", Camera.Current.GetProjectionMatrix());
            shader.SetUniform("VPMatrix", Camera.Current.GetVPMatrix());
            shader.SetUniform("ProjectionMatrix", Camera.Current.GetProjectionMatrix());
            shader.SetUniform("CurrentViewMatrix", Camera.Current.GetViewMatrix());
            shader.SetUniform("LastViewMatrix", LastViewMatrix);
            Camera.Current.SetUniforms();

            MRT.UseTextures(1, 2, 3, 15);

            DeferredFramebuffer.UseTexture(7);
            // DistanceFramebuffer.UseTexture(8);
            //ScreenSpaceReflectionsFramebuffer.GenerateMipMaps();
            ScreenSpaceReflectionsFramebuffer.UseTexture(9);
            EnvLightFramebuffer.UseTexture(10);
            BloomYPass.UseTexture(11);

            FogFramebuffer.UseTexture(13);
            AmbientOcclusionFramebuffer.UseTexture(14);

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

            shader.SetUniform("UseVDAO", GraphicsSettings.UseVDAO);
            shader.SetUniform("UseHBAO", GraphicsSettings.UseHBAO);
            shader.SetUniform("UseFog", GraphicsSettings.UseFog);
            shader.SetUniform("UseBloom", GraphicsSettings.UseBloom);
            shader.SetUniform("UseDeferred", GraphicsSettings.UseDeferred);
            shader.SetUniform("UseDepth", GraphicsSettings.UseDepth);
            shader.SetUniform("UseCubeMapGI", GraphicsSettings.UseCubeMapGI);
            shader.SetUniform("UseRSM", GraphicsSettings.UseRSM);
            shader.SetUniform("UseSSReflections", GraphicsSettings.UseSSReflections);

            shader.SetUniform("Brightness", Camera.MainDisplayCamera.Brightness);
            shader.SetUniform("VDAOGlobalMultiplier", 1.0f);
            shader.SetUniform("CurrentlyRenderedCubeMap", Game.World.CurrentlyRenderedCubeMap);
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
           // GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.DepthMask(true);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
        }
        private void EnableAdditiveBlending()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            GL.DepthMask(true);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void FaceRender(CubeMapFramebuffer framebuffer, TextureTarget target)
        {
            GL.Enable(EnableCap.DepthTest);
            framebuffer.SwitchCamera(target);
            RenderPrepareToBlit();
            GL.Disable(EnableCap.DepthTest);

            framebuffer.Use(true, false);
            framebuffer.SwitchFace(target);
            GL.Viewport(0, 0, framebuffer.Width, framebuffer.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            HDR();
            framebuffer.GenerateMipMaps();
            GL.Enable(EnableCap.DepthTest);
        }

        private void Deferred()
        {
            DeferredShader.Use();
            DeferredFramebuffer.Use();
            GlareTexture.Use(TextureUnit.Texture12);

            Game.World.Scene.SetLightingUniforms(DeferredShader);
            Game.World.Scene.MapLightsSSBOToShader(DeferredShader);
            Game.CascadeShadowMaps.SetUniforms();

            DrawPPMesh();
            Game.CheckErrors("Deferred pass");
        }

        private void EnvLight()
        {
            EnvLightShader.Use();
            EnvLightFramebuffer.Use();
            SetUniformsShared();
            EnableAdditiveBlending();
            GL.CullFace(CullFaceMode.Front);
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            for(int i = 0; i < CubeMaps.Count; i++)
            {
                if(i == Game.World.CurrentlyRenderedCubeMap)
                    continue;
                Matrix4 mat = Matrix4.CreateScale(CubeMaps[i].FalloffScale) * Matrix4.CreateTranslation(CubeMaps[i].FalloffScale > 99999.0f ? Camera.MainDisplayCamera.GetPosition() : CubeMaps[i].Position);
                CubeMaps[i].Texture.Use(TextureUnit.Texture12);
                EnvLightShader.SetUniform("ModelMatrix", mat);
                EnvLightShader.SetUniform("MapPosition", CubeMaps[i].FalloffScale > 99999.0f ? Camera.MainDisplayCamera.GetPosition() : CubeMaps[i].Position);
                EnvLightShader.SetUniform("CubeCutOff", CubeMaps[i].FalloffScale);
                CubeMapSphere.Draw();
            }
            GL.CullFace(CullFaceMode.Back);
            DisableBlending();
            Game.CheckErrors("Environment light pass");
        }

        private void HDR()
        {
            //HelperFramebuffer.Use();
            HDRShader.Use();
            HDRShader.SetUniform("InputFocalLength", Camera.Current.FocalLength);
            if(Camera.MainDisplayCamera != null)
            {
                HDRShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                HDRShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            ForwardPassFramebuffer.UseTexture(17);
            DrawPPMesh();
            Game.CheckErrors("HDR pass");
            LastViewMatrix = Camera.Current.GetViewMatrix();
        }

        private void LensDistortion()
        {
            LensDistortionShader.Use();
            HelperFramebuffer.UseTexture(4);
            DrawPPMesh();
            Game.CheckErrors("Lens Distort pass");
        }

        private void ScreenSpaceReflections()
        {
            ScreenSpaceReflectionsShader.Use();
            ScreenSpaceReflectionsFramebuffer.Use();
            DeferredFramebuffer.UseTexture(7);
            DrawPPMesh();
            Game.CheckErrors("SSR pass");
        }

        private void Fog()
        {
            FogShader.Use();
            FogFramebuffer.Use();
            Game.World.Scene.SetLightingUniforms(FogShader);
            DrawPPMesh();
            Game.CheckErrors("Fog pass");
        }

        private void AmbientOcclusion()
        {
            AmbientOcclusionShader.Use();
            AmbientOcclusionFramebuffer.Use();
            DrawPPMesh();
            Game.CheckErrors("AO pass");
        }

        private void Bloom()
        {
            BloomYPass.Use();
            BloomXPass.Use();

            BlitFramebuffers(DeferredFramebuffer, BloomYPass, BlitMode.Color);
            // here Y has deferred data, X is empty
            BloomShader.Use();
            SetUniformsShared();

            BloomShader.SetUniform("Pass", 0);
            BloomYPass.UseTexture(11);
            BloomXPass.Use();
            PostProcessingMesh.Draw();
            // here Y has deferred data, X has 1 pass result

            BloomYPass.Use();
            // here Y is empty, x has 1 pass
            BloomShader.SetUniform("Pass", 1);
            BloomXPass.UseTexture(11);
            PostProcessingMesh.Draw();
            // here y has 2 pass data
        }

        private void RenderPrepareToBlit()
        {
            Game.World.SetUniforms(this);
            MRT.Use();
            //DistanceFramebuffer.GenerateMipMaps();
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderDepth();
            GL.ColorMask(false, false, false, false);
            // Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
            //CubeMap.Use(TextureUnit.Texture8);
            //DistanceFramebuffer.UseTexture(8);
            //EnableBlending();
            GL.ColorMask(true, true, true, true);
            InternalRenderingState.PassState = InternalRenderingState.State.ForwardOpaquePass;
            DisableBlending();
            Game.World.Draw();
            //Game.World.RunOcclusionQueries();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            if(GraphicsSettings.UseCubeMapGI || GraphicsSettings.UseVDAO)
                EnvLight();
            if(GraphicsSettings.UseFog)
                Fog();
            if(GraphicsSettings.UseHBAO)
                AmbientOcclusion();
            ForwardPass();
            Deferred();
            if(GraphicsSettings.UseSSReflections)
                ScreenSpaceReflections();
            if(GraphicsSettings.UseBloom)
                Bloom();
           // HDR();
        }

        private void ForwardPass()
        {
            //GL.ClearColor(0, 1, 0, 0.5f);
            ForwardPassFramebuffer.Use(true, true);
            //Framebuffer.Default.Use();

            //BlitFramebuffers(MRT, ForwardPassFramebuffer, BlitMode.Depth);
            //ForwardPassFramebuffer.Use(true, false);
            GlareTexture.Use(TextureUnit.Texture12);

            var programs = Game.ShaderPool.ChooseShaderGenericMaterial(true).ProgramsList;
            for(int i = 0; i < programs.Length; i++)
            {
                programs[i].Use();
                Game.World.Scene.SetLightingUniforms(programs[i]);
                Game.World.Scene.MapLightsSSBOToShader(programs[i]);
                Game.CascadeShadowMaps.SetUniforms();
            }


            GL.Disable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Always);

            InternalRenderingState.PassState = InternalRenderingState.State.ForwardTransparentBlendingAlphaPass;
            EnableBlending();
            Game.World.Draw();

            InternalRenderingState.PassState = InternalRenderingState.State.ForwardTransparentBlendingAdditivePass;
            EnableAdditiveBlending();
            Game.World.Draw();

            GL.DepthFunc(DepthFunction.Lequal);
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            DisableBlending();
            GL.Enable(EnableCap.CullFace);
        }

        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
        }

    }
}