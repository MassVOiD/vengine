using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class PostProcessing
    {
        public CubeMapTexture CubeMap;

        public float LastCombinerTime = 0;

        public float LastDeferredTime = 0;

        public float LastFogTime = 0;

        public float LastHDRTime = 0;

        public float LastIndirectTime = 0;

        public float LastMRTTime = 0;

        public float LastSSAOTime = 0;

        public float LastTotalFrameTime = 0;

        public MRTFramebuffer MRT;

        public bool ShowSelected = false;

        public bool UnbiasedIntegrateRenderMode = false;
        
        public float VDAOGlobalMultiplier = 1.0f, RSMGlobalMultiplier = 1.0f, AOGlobalModifier = 1.0f;

        public int Width, Height;

        private static Random Rand = new Random();

        private ShaderProgram
            FogShader,
            AOShader,
            HDRShader,
            BlitShader,
            EdgeDetectShader,
            CombinerShader;

        private bool DisablePostEffects = false;
        
        private Matrix4 LastVP = Matrix4.Identity;

        private Texture NumbersTexture;

        private Framebuffer
            Pass1Framebuffer,
            MSAAEdgeDetectFramebuffer,
            AOFramebuffer,
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

        private Stopwatch stopwatch = new Stopwatch(), separatestowatch = new Stopwatch();

        public class AAB
        {
            public Vector4 Color;
            public AABContainers Container = null;
            public Vector3 Maximum;
            public Vector3 Minimum;

            public AAB(Vector4 color, Vector3 min, Vector3 max, AABContainers container)
            {
                Color = color;
                Minimum = min;
                Maximum = max;
                Container = container;
            }
        }

        public class AABContainers
        {
            public Vector3 Maximum;
            public Vector3 Minimum;

            public AABContainers(Vector3 min, Vector3 max)
            {
                Minimum = min;
                Maximum = max;
            }
        }
        
        public PostProcessing(int initialWidth, int initialHeight)
        {/*
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, 0u);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 7, 0u);*/
            //FullScene3DTexture = new Texture3D(new Vector3(64, 64, 64));
            NumbersTexture = new Texture(Media.Get("numbers.png"));
            CubeMap = new CubeMapTexture(Media.Get("posx.jpg"), Media.Get("posy.jpg"), Media.Get("posz.jpg"),
                Media.Get("negx.jpg"), Media.Get("negy.jpg"), Media.Get("negz.jpg"));

            Width = initialWidth;
            Height = initialHeight;
           //    initialWidth *= 4; initialHeight *= 4;
            MRT = new MRTFramebuffer(initialWidth, initialHeight);

            Pass1Framebuffer = new Framebuffer(initialWidth, initialHeight)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            MSAAEdgeDetectFramebuffer = new Framebuffer(initialWidth, initialHeight)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.R8,
                ColorPixelFormat = PixelFormat.Red,
                ColorPixelType = PixelType.UnsignedByte
            };

            FogFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };
            AOFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 3)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };

            AOShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "AO.fragment.glsl");
            FogShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Fog.fragment.glsl");
            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            BlitShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Blit.fragment.glsl");
            CombinerShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Combiner.fragment.glsl");
            EdgeDetectShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "EdgeDetect.fragment.glsl");

            PostProcessingMesh = new Object3dInfo(VertexInfo.FromFloatArray(postProcessingPlaneVertices));
        }

        // public static Texture3D FullScene3DTexture;
        private enum BlurMode
        {
            Linear, Gaussian, Temporal, Additive
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

        public float StopMeasureMS()
        {
            separatestowatch.Stop();
            long ticks = separatestowatch.ElapsedTicks;
            double ms = 1000.0 * (double)ticks / Stopwatch.Frequency;
            return (float)ms;
        }

        // public static uint RandomIntFrame = 1;
        private void Blit(BlitMode mode)
        {
            BlitShader.Use();
            BlitShader.SetUniform("BlitMode", (int)mode);
            DrawPPMesh();
        }

        int cnt = 1;
        private void EdgeDetect()
        {
            MSAAEdgeDetectFramebuffer.Use();
            EdgeDetectShader.Use();
            MRT.UseTextureDiffuseColor(30);
            MRT.UseTextureNormals(2);
            if(cnt == 0)
                DrawPPMesh();
            else
                cnt = 0;
        }

        private void Combine()
        {
            CombinerShader.Use();
            Game.World.Scene.SetLightingUniforms(CombinerShader);
            //RandomsSSBO.Use(0);
            Game.World.Scene.MapLightsSSBOToShader(CombinerShader);
            CombinerShader.SetUniform("RandomsCount", 16 * 16 * 16);
            CombinerShader.SetUniform("UseFog", Game.GraphicsSettings.UseFog);
            CombinerShader.SetUniform("UseLightPoints", Game.GraphicsSettings.UseLightPoints);
            CombinerShader.SetUniform("UseDepth", Game.GraphicsSettings.UseDepth);
            CombinerShader.SetUniform("UseDeferred", Game.GraphicsSettings.UseDeferred);
            CombinerShader.SetUniform("UseVDAO", Game.GraphicsSettings.UseVDAO);
            CombinerShader.SetUniform("UseHBAO", Game.GraphicsSettings.UseHBAO);
            CombinerShader.SetUniform("UseRSM", Game.GraphicsSettings.UseRSM);
            CombinerShader.SetUniform("UseSSReflections", Game.GraphicsSettings.UseSSReflections);
            CombinerShader.SetUniform("UseHBAO", Game.GraphicsSettings.UseHBAO);
            CombinerShader.SetUniform("Brightness", Camera.Current.Brightness);
            CombinerShader.SetUniform("VDAOGlobalMultiplier", VDAOGlobalMultiplier);
            CombinerShader.SetUniform("DisablePostEffects", DisablePostEffects);
            // GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            MRT.UseTextureDiffuseColor(30);
            MRT.UseTextureNormals(2);
            CubeMap.Use(TextureUnit.Texture3);
            AOFramebuffer.UseTexture(4);
            FogFramebuffer.UseTexture(5);
            MSAAEdgeDetectFramebuffer.UseTexture(28);
            LastCombinerTime = DrawPPMesh();
        }

        private void DisableBlending()
        {
            // GL.Disable(EnableCap.Blend);
            // GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
        }

        private float DrawPPMesh()
        {
            StartMeasureMS();
            SetUniformsShared();
            PostProcessingMesh.Draw();
            return StopMeasureMS();
        }

        private void EnableFullBlend()
        {
            //  GL.Disable(EnableCap.Blend);
            //  GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            // GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void FaceRender(CubeMapFramebuffer framebuffer, TextureTarget target)
        {
            GL.Enable(EnableCap.DepthTest);
            framebuffer.SwitchCamera(target);
            RenderPrepareToBlit();

            framebuffer.Use(true, false);
            framebuffer.SwitchFace(target);
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
            MRT.UseTextureDiffuseColor(30);
            MRT.UseTextureNormals(2);
            MSAAEdgeDetectFramebuffer.UseTexture(28);
            LastFogTime = DrawPPMesh();
        }
        private void AO()
        {
            AOShader.Use();
            AOFramebuffer.Use();
            MRT.UseTextureDiffuseColor(30);
            MRT.UseTextureNormals(2);
            MSAAEdgeDetectFramebuffer.UseTexture(28);
            DrawPPMesh();
        }

        private void HDR()
        {
            string lastTimeSTR = ((int)Math.Round(Game.CurrentFrameTime)).ToString();
            //Console.WriteLine(time);
            int[] nums = lastTimeSTR.ToCharArray().Select<char, int>((a) => a - 48).ToArray();
            HDRShader.Use();
            HDRShader.SetUniform("UseBloom", Game.GraphicsSettings.UseBloom);
            HDRShader.SetUniform("NumbersCount", nums.Length);
            HDRShader.SetUniform("ShowSelected", ShowSelected);
            HDRShader.SetUniformArray("Numbers", nums);
            HDRShader.SetUniform("UnbiasedIntegrateRenderMode", UnbiasedIntegrateRenderMode);
            HDRShader.SetUniform("InputFocalLength", Camera.Current.FocalLength);
            if(Camera.MainDisplayCamera != null)
            {
                HDRShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                HDRShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            MSAAEdgeDetectFramebuffer.UseTexture(28);
            LastHDRTime = DrawPPMesh();
        }
        
        private void RenderPrepareToBlit()
        {
            MRT.Use();
            Game.World.Draw();
                        
            if(Game.GraphicsSettings.UseFog)
            {
                Fog();
            }
            if(Game.GraphicsSettings.UseHBAO)
            {
                AO();
            }
            
            EdgeDetect();

            SwitchToFB(Pass1Framebuffer);

            Combine();

            Pass1Framebuffer.GenerateMipMaps();


            Pass1Framebuffer.UseTexture(0);
            NumbersTexture.Use(TextureUnit.Texture27);


        }

        private void StartMeasureMS()
        {
            separatestowatch.Reset();
            separatestowatch.Start();
        }

        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
        }
        
    }
}