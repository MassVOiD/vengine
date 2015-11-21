using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VEngine.PathTracing;

namespace VEngine
{
    public class PostProcessing
    {
        public class AAB
        {
            public Vector4 Color;
            public Vector3 Maximum;
            public Vector3 Minimum;
            public AABContainers Container = null;

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

        public static PathTracing.PathTracer Tracer;
        public ShaderStorageBuffer AABoxesBuffer;
        public ShaderStorageBuffer AABoxesContainersBuffer;
        public int AABoxesCount;
        public int AABoxesContainersCount;
        public ShaderStorageBuffer DensityPoints;
        public int DensityPointsCount = 0;
        public MRTFramebuffer MRT;
        public int Width, Height;
        public bool ShowSelected = false;
        public bool UnbiasedIntegrateRenderMode = false;

        public float LastDeferredTime = 0;
        public float LastSSAOTime = 0;
        public float LastIndirectTime = 0;
        public float LastCombinerTime = 0;
        public float LastFogTime = 0;
        public float LastMRTTime = 0;
        public float LastHDRTime = 0;
        public float LastTotalFrameTime = 0;

        private Texture BokehTexture;

        public float VDAOGlobalMultiplier = 1.0f, RSMGlobalMultiplier = 1.0f, AOGlobalModifier = 1.0f;

        private static uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

        private static float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        private static Random Rand = new Random();

        private ShaderProgram
                                    BloomShader,
            FogShader,
            HDRShader,
            BlitShader,
            DeferredShader,
            CombinerShader,
            PathTracerOutputShader,
            MotionBlurShader,
            RayTraceShadowsShader,
            SSAOShader,
            IndirectShader;

        private ComputeShader CShader, BlurShader;

        private CubeMapTexture CubeMap;

        private Framebuffer LastFrameBuffer;

        private long lastTime = 0;

        private Texture NumbersTexture;

        private Framebuffer
                                                    Pass1FrameBuffer,
            Pass2FrameBuffer,
            HelperFullResFrameBuffer,
           // LastWorldPosFramebuffer,
           LastDeferredFramebuffer,
           RayTracedShadowsFramebuffer,
            BloomFrameBuffer,
            IndirectFramebuffer,
            SSAOFramebuffer,
            FogFramebuffer;

        private Mesh3d PostProcessingMesh;
        private ShaderStorageBuffer RandomsSSBO = new ShaderStorageBuffer();
        // public static uint RandomIntFrame = 1;

        // public static Texture3D FullScene3DTexture;

        private Stopwatch stopwatch = new Stopwatch(), separatestowatch = new Stopwatch();

        private void StartMeasureMS()
        {
            separatestowatch.Reset();
            separatestowatch.Start();
        }

        public float StopMeasureMS()
        {
            separatestowatch.Stop();
            long ticks = separatestowatch.ElapsedTicks;
            double ms = 1000.0 * (double)ticks / Stopwatch.Frequency;
            return (float)ms;
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
            AABoxesBuffer = new ShaderStorageBuffer();
            AABoxesContainersBuffer = new ShaderStorageBuffer();
            DensityPoints = new ShaderStorageBuffer();
            NumbersTexture = new Texture(Media.Get("numbers.png"));
            CShader = new ComputeShader("Blur.compute.glsl");
            CubeMap = new CubeMapTexture(Media.Get("posx.jpg"), Media.Get("posy.jpg"), Media.Get("posz.jpg"),
                Media.Get("negx.jpg"), Media.Get("negy.jpg"), Media.Get("negz.jpg"));
            BokehTexture = new Texture(Media.Get("bokeh.png"));

            Width = initialWidth;
            Height = initialHeight;
          //   initialWidth *= 2; initialHeight *= 2;
            RandomsSSBO.Type = BufferUsageHint.StreamRead;
            RandomsSSBO.MapData(JitterRandomSequenceGenerator.Generate(1, 16 * 16 * 16, true).ToArray());

            MRT = new MRTFramebuffer(initialWidth, initialHeight);

            Pass1FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            Pass2FrameBuffer = new Framebuffer(initialWidth, initialHeight);

            BloomFrameBuffer = new Framebuffer(initialWidth / 6, initialHeight / 6)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.UnsignedByte
            };
            RayTracedShadowsFramebuffer = new Framebuffer(initialWidth / 3, initialHeight / 3)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rg8,
                ColorPixelFormat = PixelFormat.Rg,
                ColorPixelType = PixelType.UnsignedByte
            };
            FogFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.UnsignedByte
            };
            //LastWorldPosFramebuffer = new Framebuffer(initialWidth / 5, initialHeight / 5)
            HelperFullResFrameBuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba8,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.UnsignedByte
            };
             LastDeferredFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1)
             {
                 ColorOnly = true,
                 ColorInternalFormat = PixelInternalFormat.Rgba,
                 ColorPixelFormat = PixelFormat.Rgba,
                 ColorPixelType = PixelType.UnsignedByte
             };
            IndirectFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1);
            SSAOFramebuffer = new Framebuffer((int)(initialWidth / 1), (int)(initialHeight / 1))
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.Rgba8,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.UnsignedByte
            };

            BloomShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Bloom.fragment.glsl");
            FogShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Fog.fragment.glsl");
            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            BlitShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Blit.fragment.glsl");
            DeferredShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Deferred.fragment.glsl");
            CombinerShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Combiner.fragment.glsl");
            PathTracerOutputShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Output.fragment.glsl");
            MotionBlurShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "MotionBlur.fragment.glsl");
            SSAOShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "SSAO.fragment.glsl");
            IndirectShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Indirect.fragment.glsl");
            RayTraceShadowsShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "RayTraceShadows.fragment.glsl");

            BlurShader = new ComputeShader("Blur.compute.glsl");

            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices, postProcessingPlaneIndices);
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, new GenericMaterial(Color.Pink));
        }

        private enum BlurMode
        {
            Linear, Gaussian, Temporal, Additive
        }

        private Matrix4 LastVP = Matrix4.Identity;

        public void ExecutePostProcessing()
        {
            if(UnbiasedIntegrateRenderMode)
            {
                RandomsSSBO.MapData(JitterRandomSequenceGenerator.Generate(1, 16 * 16 * 16, true).ToArray());
            }
            // okay lets do it
            if(Tracer != null)
            {
                //FullScene3DTexture.Clear();
                // FullScene3DTexture.Use(0);
                //GL.BindImageTexture(22u, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.Disable(EnableCap.CullFace);
                //GL.CullFace(CullFaceMode.Back);
                // GL.DepthFunc(DepthFunction.Always);
                DisableBlending();
                Mesh3d.PostProcessingUniformsOnly = false;
                MRT.Use();
                World.Root.Draw();
                Mesh3d.PostProcessingUniformsOnly = true;
                //GL.DepthFunc(DepthFunction.Always);
                //GL.Disable(EnableCap.CullFace);
                SwitchToFB1();
                /*if(VDAOFramebuffer.TexColor <= 0)
                {
                    VDAOFramebuffer.Use();
                    RSMFramebuffer.Use();
                    SwitchToFB0();
                }
                Tracer.PathTraceToImage(MRT, VDAOFramebuffer.TexColor, RSMFramebuffer.TexColor, VDAOFramebuffer.Width, VDAOFramebuffer.Height);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
               // SwitchToFB0();
              //  VDAOFramebuffer.UseTexture(0);
                //PathTracerOutputShader.Use();
               // ShaderProgram.Lock = true;
               // PostProcessingMesh.Draw();
               // ShaderProgram.Lock = false;
                SwitchToFB(RSMFramebuffer);// not really related
                VDAOFramebuffer.UseTexture(0);
                Blit();*/
                // return;
            }
            stopwatch.Stop();
            lastTime = (lastTime * 20 + stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L))) / 21;
            stopwatch.Reset();
            stopwatch.Start();

            DisableBlending();
            // RandomsSSBO.MapData(JitterRandomSequenceGenerator.Generate(1, 16 * 16 * 16, true).ToArray());

            Mesh3d.PostProcessingUniformsOnly = false;
            GL.CullFace(CullFaceMode.Back);
            StartMeasureMS();
            GL.BlendFunc(0, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            MRT.Use();
            LastMRTTime = StopMeasureMS();
            World.Root.Draw();
            Mesh3d.PostProcessingUniformsOnly = true;

            MRT.UseTextureDiffuseColor(14);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureNormals(16);
            MRT.UseTextureMeshData(17);
            MRT.UseTextureId(18);
           // LastWorldPosFramebuffer.UseTexture(21);

            if(GLThread.GraphicsSettings.UseFog)
            {
                SwitchToFB(FogFramebuffer);
                MRT.UseTextureDepth(1);
                Fog();
            }
            if(GLThread.GraphicsSettings.UseHBAO)
            {
                SwitchToFB(SSAOFramebuffer);
                SSAO();
                
               // SwitchToFB(HelperFullResFrameBuffer);
              //  Blur(HelperFullResFrameBuffer, HelperFullResFrameBuffer, SSAOFramebuffer, 14);
               // HelperFullResFrameBuffer.UseTexture(23);
            }
            // Blur(IndirectFramebuffer.TexColor, 8, IndirectFramebuffer.Width,
            // IndirectFramebuffer.Height, BlurMode.Temporal); Blur(SSAOFramebuffer.TexColor, 8,
            // SSAOFramebuffer.Width, SSAOFramebuffer.Height, BlurMode.Temporal);
            // Blur(IndirectFramebuffer.TexColor, 8, IndirectFramebuffer.Width,
            // IndirectFramebuffer.Height, BlurMode.Temporal);
            if(GLThread.GraphicsSettings.UseDeferred)
            {
                SwitchToFB(RayTracedShadowsFramebuffer);
                MRT.UseTextureDepth(1);
                RaytraceShadows();
                SwitchToFB(Pass2FrameBuffer);
                MRT.UseTextureDepth(1);
                Deferred();
            }
            else
                SwitchToFB(Pass2FrameBuffer);

            if(GLThread.GraphicsSettings.UseRSM)
            {
                SwitchToFB(IndirectFramebuffer);
                Indirect();
                FogFramebuffer.UseTexture(24);
                IndirectFramebuffer.UseTexture(22);
            }

            // SwitchToFB(Pass2FrameBuffer);

            //  Blur(SSAOFramebuffer.TexColor, Pass1FrameBuffer.TexColor, 28, SSAOFramebuffer.Width, SSAOFramebuffer.Height, Pass1FrameBuffer.Width, Pass1FrameBuffer.Height, BlurMode.Temporal);
            //  Blur(IndirectFramebuffer.TexColor, Pass1FrameBuffer.TexColor, 28, IndirectFramebuffer.Width, IndirectFramebuffer.Height, Pass1FrameBuffer.Width, Pass1FrameBuffer.Height, BlurMode.Temporal);
              

            SwitchToFB(Pass1FrameBuffer);

            Pass2FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            RandomsSSBO.Use(0);
            SSAOFramebuffer.UseTexture(23);
            FogFramebuffer.UseTexture(24);
            Combine();

            if(GLThread.GraphicsSettings.UseBloom)
            {
                SwitchToFB(BloomFrameBuffer);
                Pass1FrameBuffer.UseTexture(0);
                Bloom();
                BloomFrameBuffer.UseTexture(26);
            }

            /*   SwitchToFB(Pass2FrameBuffer);
               Pass1FrameBuffer.UseTexture(0);
               MRT.UseTextureDepth(1);
               LastWorldPosFramebuffer.UseTexture(21);
               MRT.UseTextureWorldPosition(15);

               MotionBlur();*/

            SwitchToFB(Pass2FrameBuffer);

            Pass1FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            NumbersTexture.Use(TextureUnit.Texture25);

            HDR(lastTime == 0 ? 0 : 1000000 / lastTime);

            SwitchToFB(LastDeferredFramebuffer);
            if(UnbiasedIntegrateRenderMode)
            {
                Pass2FrameBuffer.UseTexture(0);
                MRT.UseTextureDepth(1);
                Blit();
            }
            SwitchToFB0();
            Pass2FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            Blit();

           // SwitchToFB(LastWorldPosFramebuffer);
          //  MRT.UseTextureWorldPosition(0);
           // Blit();
            EnableFullBlend();
            

            if(World.Root != null && World.Root.UI != null)
                World.Root.UI.DrawAll();
            Mesh3d.PostProcessingUniformsOnly = false;
        }

        public void SetAABoxes(List<AABContainers> containers, List<AAB> boxes)
        {
            boxes.Sort((a, b) => a.Container.GetHashCode() - b.Container.GetHashCode());
            List<byte> buffer = new List<byte>(boxes.Count * 4 * 4 * 4);
            foreach(var b in boxes)
            {
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.X));
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.Y));
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.Z));
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.Z));

                buffer.AddRange(BitConverter.GetBytes(b.Maximum.X));
                buffer.AddRange(BitConverter.GetBytes(b.Maximum.Y));
                buffer.AddRange(BitConverter.GetBytes(b.Maximum.Z));
                buffer.AddRange(BitConverter.GetBytes(b.Maximum.Z));

                buffer.AddRange(BitConverter.GetBytes(b.Color.X));
                buffer.AddRange(BitConverter.GetBytes(b.Color.Y));
                buffer.AddRange(BitConverter.GetBytes(b.Color.Z));
                buffer.AddRange(BitConverter.GetBytes(b.Color.W));

                buffer.AddRange(BitConverter.GetBytes(containers.IndexOf(b.Container)));
                buffer.AddRange(BitConverter.GetBytes(containers.IndexOf(b.Container)));
                buffer.AddRange(BitConverter.GetBytes(containers.IndexOf(b.Container)));
                buffer.AddRange(BitConverter.GetBytes(containers.IndexOf(b.Container)));
            }
            AABoxesCount = boxes.Count;
            AABoxesBuffer.MapData(buffer.ToArray());

            buffer = new List<byte>(containers.Count * 4 * 3 * 4);
            foreach(var b in containers)
            {
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.X));
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.Y));
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.Z));
                buffer.AddRange(BitConverter.GetBytes(b.Minimum.Z));

                buffer.AddRange(BitConverter.GetBytes(b.Maximum.X));
                buffer.AddRange(BitConverter.GetBytes(b.Maximum.Y));
                buffer.AddRange(BitConverter.GetBytes(b.Maximum.Z));
                buffer.AddRange(BitConverter.GetBytes(b.Maximum.Z));

                var fi = boxes.FindIndex((a) => a.Container == b);
                var ic = boxes.Count((a) => a.Container == b);
                if(fi < 0)
                    fi = 0;
                buffer.AddRange(BitConverter.GetBytes(fi));
                buffer.AddRange(BitConverter.GetBytes(ic));
                buffer.AddRange(BitConverter.GetBytes(0));
                buffer.AddRange(BitConverter.GetBytes(0));
            }
            AABoxesContainersCount = containers.Count;
            AABoxesContainersBuffer.MapData(buffer.ToArray());
        }

        private void Blit()
        {
            BlitShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }

        private void Bloom()
        {
            BloomShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }

        private void Blur(Framebuffer output, Framebuffer helper, Framebuffer source, int length)
        {
            // GL.BindImageTexture(0, output.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);
            GL.BindImageTexture(1, helper.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, source.TexColor);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, MRT.TexDepth);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, MRT.TexNormals);

            BlurShader.Use();
            BlurShader.SetUniform("Length", length);
            BlurShader.SetUniform("Direction", 0);
            BlurShader.SetUniform("FarPlane", Camera.Current.Far);
            BlurShader.Dispatch(helper.Width / 32 + 1, helper.Height / 32 + 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);

            // GL.BindImageTexture(0, output.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);
            GL.BindImageTexture(1, helper.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);
            BlurShader.SetUniform("Length", length);
            BlurShader.SetUniform("Direction", 1);
            BlurShader.SetUniform("FarPlane", Camera.Current.Far);
            BlurShader.Dispatch(output.Width / 32 + 1, output.Height / 32 + 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
        }

        private float DrawPPMesh()
        {
            ShaderProgram.Lock = true;
            StartMeasureMS();
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
            return StopMeasureMS();
        }

        private void Combine()
        {
            CombinerShader.Use();
            CombinerShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(0);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureNormals(16);
            MRT.UseTextureMeshData(17);
            MRT.UseTextureId(18);
            LastDeferredFramebuffer.UseTexture(20);
            // World.Root.RootScene.SetLightingUniforms(CombinerShader, Matrix4.Identity, false);
            CombinerShader.SetUniform("UseFog", GLThread.GraphicsSettings.UseFog);
            CombinerShader.SetUniform("UseLightPoints", GLThread.GraphicsSettings.UseLightPoints);
            CombinerShader.SetUniform("UseDepth", GLThread.GraphicsSettings.UseDepth);
            CombinerShader.SetUniform("UseDeferred", GLThread.GraphicsSettings.UseDeferred);
            CombinerShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            CombinerShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            CombinerShader.SetUniform("UseSSReflections", GLThread.GraphicsSettings.UseSSReflections);
            CombinerShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            CombinerShader.SetUniform("Brightness", Camera.Current.Brightness);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            LastCombinerTime = DrawPPMesh();
        }

        private void Deferred()
        {
            DeferredShader.Use();
            World.Root.RootScene.SetLightingUniforms(DeferredShader, Matrix4.Identity);
            MRT.UseTextureDepth(1);
            DeferredShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            DeferredShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            DeferredShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            DeferredShader.SetUniform("VDAOGlobalMultiplier", VDAOGlobalMultiplier);
            CubeMap.Use(TextureUnit.Texture19);
            DeferredShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            AABoxesBuffer.Use(7);
            AABoxesContainersBuffer.Use(8);
            MRT.UseTextureId(18);
            RayTracedShadowsFramebuffer.UseTexture(22);
            SSAOFramebuffer.UseTexture(23);
            World.Root.RootScene.RecreateSimpleLightsSSBO();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            World.Root.RootScene.MapLightsSSBOToShader(DeferredShader);
            DeferredShader.SetUniform("AABoxesCount", AABoxesCount);
            DeferredShader.SetUniform("AABoxesContainersCount", AABoxesContainersCount);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            LastDeferredTime = DrawPPMesh();
            IndirectFramebuffer.UseTexture(22);
        }
        private void RaytraceShadows()
        {
            if(AABoxesCount == 0)
                return;
            RayTraceShadowsShader.Use();
            World.Root.RootScene.SetLightingUniforms(RayTraceShadowsShader, Matrix4.Identity);
            MRT.UseTextureDepth(1);
            RayTraceShadowsShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            RayTraceShadowsShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            RayTraceShadowsShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            RayTraceShadowsShader.SetUniform("VDAOGlobalMultiplier", VDAOGlobalMultiplier);
            CubeMap.Use(TextureUnit.Texture19);
            RayTraceShadowsShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            AABoxesBuffer.Use(7);
            AABoxesContainersBuffer.Use(8);
            MRT.UseTextureId(18);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            World.Root.RootScene.MapLightsSSBOToShader(RayTraceShadowsShader);
            RayTraceShadowsShader.SetUniform("AABoxesCount", AABoxesCount);
            RayTraceShadowsShader.SetUniform("AABoxesContainersCount", AABoxesContainersCount);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }

        private void DisableBlending()
        {
            GL.Disable(EnableCap.Blend);
        }

        private void EnableFullBlend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void Fog()
        {
            FogShader.Use();
            World.Root.RootScene.SetLightingUniforms(FogShader, Matrix4.Identity);
            FogShader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            LastFogTime = DrawPPMesh();
        }

        private void HDR(long time)
        {
            string lastTimeSTR = time.ToString();
            //Console.WriteLine(time);
            int[] nums = lastTimeSTR.ToCharArray().Select<char, int>((a) => a - 48).ToArray();
            HDRShader.Use();
            HDRShader.SetUniform("UseBloom", GLThread.GraphicsSettings.UseBloom);
            HDRShader.SetUniform("NumbersCount", nums.Length);
            HDRShader.SetUniform("ShowSelected", ShowSelected);
            HDRShader.SetUniformArray("Numbers", nums);
            HDRShader.SetUniform("UnbiasedIntegrateRenderMode", UnbiasedIntegrateRenderMode);
            HDRShader.SetUniform("InputFocalLength", Camera.Current.FocalLength);
            MRT.UseTextureDepth(1);
            BokehTexture.Use(TextureUnit.Texture0 + 28);
            MRT.UseTextureId(18);
            LastDeferredFramebuffer.UseTexture(20);
            if(Camera.MainDisplayCamera != null)
            {
                HDRShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                HDRShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            MRT.UseTextureDepth(1);
            LastHDRTime = DrawPPMesh();
        }

        private void Indirect()
        {
            IndirectShader.Use();
            World.Root.RootScene.SetLightingUniforms(IndirectShader, Matrix4.Identity);
            MRT.UseTextureDepth(1);
            IndirectShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            IndirectShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            DeferredShader.SetUniform("RSMGlobalMultiplier", RSMGlobalMultiplier);
            IndirectShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            SSAOFramebuffer.UseTexture(23);
            ProjectionLight.RSMBuffer.Use(9);
            LastIndirectTime = DrawPPMesh();
        }

        private void MotionBlur()
        {
            MotionBlurShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }

        private void SSAO()
        {
            SSAOShader.Use();
            RandomsSSBO.Use(6);
            MRT.UseTextureDepth(1);
            MRT.UseTextureId(18);
            CubeMap.Use(TextureUnit.Texture19);
            SSAOShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            SSAOShader.SetUniform("AOGlobalModifier", AOGlobalModifier);
            SSAOShader.SetUniform("RandomsCount", 16 * 16 * 16);
            LastSSAOTime = DrawPPMesh();
        }

        private Framebuffer SwitchBetweenFB()
        {
            if(LastFrameBuffer == Pass1FrameBuffer)
                return SwitchToFB2();
            else
                return SwitchToFB1();
        }

        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
            if((buffer == Pass1FrameBuffer || buffer == Pass2FrameBuffer) && LastFrameBuffer != null)
            {
                LastFrameBuffer.UseTexture(0);
                LastFrameBuffer = buffer;
            }
        }

        private void SwitchToFB0()
        {
            Pass2FrameBuffer.RevertToDefault();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //LastFrameBuffer.UseTexture(0);
        }

        private Framebuffer SwitchToFB1()
        {
            Pass1FrameBuffer.Use();
            if(LastFrameBuffer != null)
                LastFrameBuffer.UseTexture(0);
            LastFrameBuffer = Pass1FrameBuffer;
            return Pass1FrameBuffer;
        }

        private Framebuffer SwitchToFB2()
        {
            Pass2FrameBuffer.Use();
            if(LastFrameBuffer != null)
                LastFrameBuffer.UseTexture(0);
            LastFrameBuffer = Pass2FrameBuffer;
            return Pass2FrameBuffer;
        }
    }
}