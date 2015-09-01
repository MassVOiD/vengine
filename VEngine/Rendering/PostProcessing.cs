using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using VEngine.Particles;
using System.Drawing;
using VEngine.PathTracing;

namespace VEngine
{
    public class PostProcessing
    {
        public int Width, Height;

        private ShaderProgram 
            BloomShader, 
            MSAAShader, 
            SSAOShader, 
            FogShader, 
            LightPointsShader, 
            LensBlurShader, 
            HDRShader,
            GlobalIlluminationShaderX,
            GlobalIlluminationShaderY,
            BlitShader, 
            DeferredShader, 
            CombinerShader,
            BackDepthWriterShader,
            ScreenSpaceNormalsWriterShader,
            RSMShader,
            VDAOShader,
            PathTracerOutputShader,
            SSReflectionsShader;

        private Framebuffer
            MSAAResolvingFrameBuffer,
            Pass1FrameBuffer,
            Pass2FrameBuffer,
            LightPointsFrameBuffer,
            BloomFrameBuffer,
            FogFramebuffer,
            SmallFrameBuffer,
            GlobalIlluminationFrameBuffer,
            LastWorldPositionFramebuffer,
            RSMFramebuffer,
            VDAOFramebuffer,
            SSReflectionsFramebuffer;

        public MRTFramebuffer MRT, BackMRT;
        ShaderStorageBuffer RandomsSSBO = new ShaderStorageBuffer();

        private ShaderStorageBuffer TestBuffer;

        private Mesh3d PostProcessingMesh;

        private Texture NumbersTexture;
        private CubeMapTexture CubeMap;
      //  public static uint RandomIntFrame = 1;

       // public static Texture3D FullScene3DTexture;

        private Stopwatch stopwatch = new Stopwatch();

        private static uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

        private ComputeShader CShader;

        private static float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        public PostProcessing(int initialWidth, int initialHeight)
        {
            //FullScene3DTexture = new Texture3D(new Vector3(64, 64, 64));
            TestBuffer = new ShaderStorageBuffer();
            NumbersTexture = new Texture(Media.Get("numbers.png"));
            CShader = new ComputeShader("Blur.compute.glsl");
            CubeMap = new CubeMapTexture(Media.Get("posx.jpg"), Media.Get("posy.jpg"), Media.Get("posz.jpg"),
                Media.Get("negx.jpg"), Media.Get("negy.jpg"), Media.Get("negz.jpg"));
            GLThread.Invoke(() =>
            {
                TestBuffer.MapData(new Vector3[4]{
                    new Vector3(1, 0.25f, 1), new Vector3(0, 0.55f, 0.75f),
                    new Vector3(1, 0.25f, 0), new Vector3(0.55f, 0, 0.75f)
                });

            });
            Width = initialWidth;
            Height = initialHeight;
            MSAAResolvingFrameBuffer = new Framebuffer(initialWidth, initialHeight);
            MSAAResolvingFrameBuffer.SetMultiSample(true);
            RandomsSSBO.MapData(JitterRandomSequenceGenerator.Generate(8, 16 * 16 * 16, true).ToArray());

            MRT = new MRTFramebuffer(initialWidth / 1, initialHeight / 1);
            BackMRT = new MRTFramebuffer(initialWidth / 4, initialHeight / 4);

            Pass1FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            Pass2FrameBuffer = new Framebuffer(initialWidth, initialHeight);

            LightPointsFrameBuffer = new Framebuffer(initialWidth / 6, initialHeight / 6);
            BloomFrameBuffer = new Framebuffer(initialWidth / 5, initialHeight / 5);
            FogFramebuffer = new Framebuffer(initialWidth/6, initialHeight/6);
            SmallFrameBuffer = new Framebuffer(initialWidth / 10, initialHeight / 10);
            LastWorldPositionFramebuffer = new Framebuffer(initialWidth / 8, initialHeight / 8);
            RSMFramebuffer = new Framebuffer(initialWidth / 3, initialHeight / 3);
            SSReflectionsFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2);
            VDAOFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1);

            GlobalIlluminationFrameBuffer = new Framebuffer(initialWidth, initialHeight);
            //GlobalIlluminationFrameBuffer.ColorInternalFormat = PixelInternalFormat.R8;
            //GlobalIlluminationFrameBuffer.ColorPixelFormat = PixelFormat.Red;
            //GlobalIlluminationFrameBuffer.ColorPixelType = PixelType.UnsignedByte;
            GlobalIlluminationFrameBuffer.Use();  
            //BackDiffuseFrameBuffer = new Framebuffer(initialWidth / 2, initialHeight  / 2);
            //BackNormalsFrameBuffer = new Framebuffer(initialWidth / 2, initialHeight / 2); 

            ScreenSpaceNormalsWriterShader = ShaderProgram.Compile("Generic.vertex.glsl", "ScreenSpaceNormalsWriter.fragment.glsl");
            BackDepthWriterShader = ShaderProgram.Compile("Generic.vertex.glsl", "BackDepthWriter.fragment.glsl");

            BloomShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Bloom.fragment.glsl");
            MSAAShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "MSAA.fragment.glsl");
            SSAOShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "SSAO.fragment.glsl");
            FogShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Fog.fragment.glsl");
            LightPointsShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "LightPoints.fragment.glsl");
            LensBlurShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "LensBlur.fragment.glsl");
            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            BlitShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Blit.fragment.glsl");
            DeferredShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Deferred.fragment.glsl");
            CombinerShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Combiner.fragment.glsl");
            PathTracerOutputShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Output.fragment.glsl");
            GlobalIlluminationShaderX = ShaderProgram.Compile("PostProcess.vertex.glsl", "GlobalIllumination.fragment.glsl");
            GlobalIlluminationShaderX.SetGlobal("SEED", "gl_FragCoord.x");
            GlobalIlluminationShaderY = ShaderProgram.Compile("PostProcess.vertex.glsl", "GlobalIllumination.fragment.glsl");
            GlobalIlluminationShaderY.SetGlobal("SEED", "gl_FragCoord.y");
            RSMShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "RSM.fragment.glsl");
            SSReflectionsShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "SSReflections.fragment.glsl");
            VDAOShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "VDAO.fragment.glsl");
            //ReflectShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Reflect.fragment.glsl"));

            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices, postProcessingPlaneIndices);
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, new GenericMaterial(Color.Pink));
        }

        private Framebuffer LastFrameBuffer;

        private void SwitchToFB0()
        {
            Pass2FrameBuffer.RevertToDefault();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            LastFrameBuffer.UseTexture(0);
        }
        private Framebuffer SwitchToFB1()
        {
            Pass1FrameBuffer.Use();
            if(LastFrameBuffer != null) LastFrameBuffer.UseTexture(0);
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

        private void SwitchToFB(Framebuffer buffer)
        {
            buffer.Use();
            if(buffer == Pass1FrameBuffer || buffer == Pass2FrameBuffer)
            {
                LastFrameBuffer.UseTexture(0);
                LastFrameBuffer = buffer;
            }
        }

        private void EnableFullBlend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void DisableBlending()
        {
            GL.Disable(EnableCap.Blend);
        }

        private void Bloom()
        {
            BloomShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        bool GIPassOdd = false;
        static Random Rand = new Random();
        private void GlobalIllumination()
        {
            GlobalIlluminationShaderX.Use();
            ShaderProgram.Lock = true;
            float[] seeds = new float[256];
            for(int i = 0; i < 256; i++)
                seeds[i] = (float)Rand.NextDouble();
            GlobalIlluminationShaderX.SetUniformArray("Seeds", seeds);
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private void LightsPoints()
        {
            LightPointsShader.Use();
            LightPool.MapSimpleLightsToShader(LightPointsShader);
            SetLightingUniforms(LightPointsShader);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private void Deferred()
        {
            DeferredShader.Use();
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            MRT.UseTextureMeshData(33);
            BackMRT.UseTextureWorldPosition(32);
            LightPool.UseTextures(2);
            LightPool.MapSimpleLightsToShader(DeferredShader);
            SetLightingUniforms(DeferredShader);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void RSM()
        {
            RSMShader.Use();
            RSMShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            MRT.UseTextureMeshData(33);
            BackMRT.UseTextureDepth(32);
            LightPool.UseTextures(2);
            MRT.UseTextureDiffuseColor(0);
            MRT.UseTextureDepth(1);
            //RandomsSSBO.Use(6);
            LightPool.MapSimpleLightsToShader(RSMShader);
            SetLightingUniforms(RSMShader);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void VDAO()
        {
            VDAOShader.Use();
            VDAOShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            VDAOShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);

            VDAOShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            MRT.UseTextureMeshData(33);
            MRT.UseTextureDiffuseColor(0);
            MRT.UseTextureDepth(1);
            CubeMap.Use(TextureUnit.Texture29);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void SSReflections()
        {
            SSReflectionsShader.Use();
            SSReflectionsShader.SetUniform("UseSSReflections", GLThread.GraphicsSettings.UseSSReflections);
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            MRT.UseTextureMeshData(33);
            VDAOFramebuffer.UseTexture(34);
            BackMRT.UseTextureWorldPosition(32);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private void Fog()
        {
            FogShader.Use();
            LightPool.UseTextures(2);
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            FogShader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            SetLightingUniforms(FogShader);
            LightPool.MapSimpleLightsToShader(FogShader);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void HDR(long time)
        {
            string lastTimeSTR = time.ToString();
            //Console.WriteLine(time);
            int[] nums = lastTimeSTR.ToCharArray().Select<char, int>((a) => a - 48).ToArray();
            HDRShader.Use();
            CombinerShader.SetUniform("UseBloom", GLThread.GraphicsSettings.UseBloom);
            CombinerShader.SetUniform("NumbersCount", nums.Length);
            CombinerShader.SetUniformArray("Numbers", nums);
            if(Camera.MainDisplayCamera != null)
            {
                LensBlurShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                LensBlurShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            MRT.UseTextureDiffuseColor(2);
            MRT.UseTextureDepth(3);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void LensBlur()
        {
            LensBlurShader.Use();
            if(Camera.Current != null)
            {
                LensBlurShader.SetUniform("CameraCurrentDepth", Camera.Current.CurrentDepthFocus);
                LensBlurShader.SetUniform("LensBlurAmount", Camera.Current.LensBlurAmount);
            }
            MRT.UseTextureDepth(2);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private void Blit()
        {
            BlitShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void Combine()
        {
            CombinerShader.Use();
            LightPool.MapSimpleLightsToShader(CombinerShader);
            SetLightingUniforms(CombinerShader);
            CombinerShader.SetUniform("UseFog", GLThread.GraphicsSettings.UseFog);
            CombinerShader.SetUniform("UseLightPoints", GLThread.GraphicsSettings.UseLightPoints);
            CombinerShader.SetUniform("UseDepth", GLThread.GraphicsSettings.UseDepth);
            CombinerShader.SetUniform("UseDeferred", GLThread.GraphicsSettings.UseDeferred);
            CombinerShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            CombinerShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            CombinerShader.SetUniform("UseSSReflections", GLThread.GraphicsSettings.UseSSReflections);
            CombinerShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            CombinerShader.SetUniform("Brightness", Camera.Current.Brightness);
            ShaderProgram.Lock = true;
            //TestBuffer.Use(2);
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private Framebuffer SwitchBetweenFB()
        {
            if(LastFrameBuffer == Pass1FrameBuffer)
                return SwitchToFB2();
            else
                return SwitchToFB1();
        }

        long lastTime = 0;

        public static PathTracing.PathTracer Tracer;

        public void ExecutePostProcessing()
        {
            // okay
            // lets do it
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
                if(VDAOFramebuffer.TexColor <= 0)
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
                Blit();
              //  return;
            }
            /**
             * MSAA way:
             * - Draw world position fb
             * - Draw normals fb
             * - Draw MSAA fb
             * - Resolve MSAA
             * 
             * Non MSAA way
             * - Draw world position fb
             * - Draw normals fb
             * - Switch to FB1 and draw scene
             */
            //RandomIntFrame = (uint)Rand.Next(512);
            //WriteBackDepth();
            stopwatch.Stop();
            lastTime = (lastTime * 20 + stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L))) / 21;
            stopwatch.Reset();
            stopwatch.Start();

          //  LastWorldPositionFramebuffer.Use();
          //  MRT.UseTextureWorldPosition(0);
          //  Blit();

            DisableBlending();

            Mesh3d.PostProcessingUniformsOnly = false;
            //FullScene3DTexture.Clear();
           // FullScene3DTexture.Use(0);
            //GL.BindImageTexture(22u, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            GL.CullFace(CullFaceMode.Back);
            MRT.Use();
            World.Root.Draw();
            //GL.CullFace(CullFaceMode.Front);
           // BackMRT.Use();
           // World.Root.Draw();
          //  GL.CullFace(CullFaceMode.Back);
            Mesh3d.PostProcessingUniformsOnly = true;

            DisableBlending();

            if(GLThread.GraphicsSettings.UseLightPoints)
            {
                SwitchToFB(LightPointsFrameBuffer);
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
               // LightsPoints();
            }

            if(GLThread.GraphicsSettings.UseFog)
            {
                SwitchToFB(FogFramebuffer);
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                Fog();
            }

            //SwitchBetweenFB();
            //SSAO();

            if(GLThread.GraphicsSettings.UseDeferred)
            {
                SwitchBetweenFB();
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                MRT.UseTextureWorldPosition(30);
                MRT.UseTextureNormals(31);
                //BloomFrameBuffer.UseTexture(29);
                Deferred();
            }

            SwitchToFB(VDAOFramebuffer);
            if(GLThread.GraphicsSettings.UseVDAO || GLThread.GraphicsSettings.UseHBAO)
                VDAO();
            
            SwitchToFB(SSReflectionsFramebuffer);
            LastFrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            FogFramebuffer.UseTexture(2);
            LightPointsFrameBuffer.UseTexture(4);
            // BloomFrameBuffer.UseTexture(5);
            GlobalIlluminationFrameBuffer.UseTexture(6);
            MRT.UseTextureDiffuseColor(7);
            MRT.UseTextureNormals(8);
            MRT.UseTextureWorldPosition(9);
            LastWorldPositionFramebuffer.UseTexture(10);
            MRT.UseTextureMeshData(11);
            SSReflections();

            SwitchToFB(RSMFramebuffer);
            RSM();



            var p1 = SwitchBetweenFB();
            var p2 = p1 == Pass1FrameBuffer ? Pass2FrameBuffer : Pass1FrameBuffer;
            
            /*SwitchToFB(GlobalIlluminationFrameBuffer);

            ReflectShader.Use();
            LastFrameBuffer.UseTexture(0);
            ShaderProgram.Lock = true;
            //MRT.UseTextureWorldPosition(3);
            //MRT.UseTextureNormals(4);
            ScreenSpaceMRT.UseTextureNormals(5);
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;*/


            SwitchToFB(p1);
            p2.UseTexture(0);
            MRT.UseTextureDepth(1);
            FogFramebuffer.UseTexture(2);
            LightPointsFrameBuffer.UseTexture(4);
           // BloomFrameBuffer.UseTexture(5);
            GlobalIlluminationFrameBuffer.UseTexture(6);
            MRT.UseTextureDiffuseColor(7);
            MRT.UseTextureNormals(8);
            MRT.UseTextureWorldPosition(9);
            LastWorldPositionFramebuffer.UseTexture(10);
            MRT.UseTextureMeshData(11);
            RSMFramebuffer.UseTexture(12);
            SSReflectionsFramebuffer.UseTexture(13);
            VDAOFramebuffer.UseTexture(14);
            //BackDepthFrameBuffer.UseTexture(6);

            

            Combine();


            //SwitchToFB(GlobalIlluminationFrameBuffer);
            //p1.UseTexture(0);
            //Blit();

           // SwitchBetweenFB();
           // if(World.Root.SkyDome != null) World.Root.SkyDome.Draw();
            //LensBlur();

            if(GLThread.GraphicsSettings.UseBloom)
            {
                SwitchToFB(BloomFrameBuffer);
                p1.UseTexture(0);
                Bloom();
            }
            SwitchToFB0();
            /*MRT.UseTextureDiffuseColor(1);
            MRT.UseTextureDepth(2);
            MRT.UseTextureWorldPosition(3);
            MRT.UseTextureNormals(4);
            p1.UseTexture(5);
            MRT.UseTextureDepth(6);
            FogFramebuffer.UseTexture(7);
            LightPointsFrameBuffer.UseTexture(8);
            BloomFrameBuffer.UseTexture(9);
            GlobalIlluminationFrameBuffer.UseTexture(10);*/
            p1.UseTexture(0);
            MRT.UseTextureDepth(2);
            BloomFrameBuffer.UseTexture(4);
            MRT.UseTextureWorldPosition(5);
            NumbersTexture.Use(TextureUnit.Texture9);

            HDR(lastTime == 0 ? 0 : 1000000 / lastTime);
            EnableFullBlend();
            if(World.Root != null && World.Root.UI != null)
                World.Root.UI.DrawAll();
            Mesh3d.PostProcessingUniformsOnly = false;
        }

        private void SetLightingUniforms(ShaderProgram shader)
        {
            shader.SetUniformArray("LightsPs", LightPool.GetPMatrices());
            shader.SetUniformArray("LightsVs", LightPool.GetVMatrices());
            shader.SetUniformArray("LightsPos", LightPool.GetPositions());
            shader.SetUniformArray("LightsFarPlane", LightPool.GetFarPlanes());
            shader.SetUniformArray("LightsColors", LightPool.GetColors());
            shader.SetUniformArray("LightsRanges", LightPool.GetRanges());
            shader.SetUniformArray("LightsMixModes", LightPool.GetMixModes());
            shader.SetUniform("LightsCount", LightPool.GetPositions().Length);
        }

        public void UpdateCameraFocus(Camera camera)
        {
            GLThread.Invoke(() => camera.CurrentDepthFocus = (camera.CurrentDepthFocus * 4.0f + SmallFrameBuffer.GetDepth(0.5f, 0.5f)) / 5.0f);
        }

        public void UpdateCameraBrightness(Camera camera)
        {
            if(!SmallFrameBuffer.Generated)
                return;
            GLThread.Invoke(() =>
            {
                var pixels = SmallFrameBuffer.GetColorBuffer();
                GLThread.RunAsync(() =>
                {
                    float average = 0.0f;
                    for(int i = 0; i < pixels.Length; i += 4)
                    {
                        var l = pixels[i].ToVector3().LengthFast;
                        average += l / pixels.Length;
                    }
                    camera.Brightness = (camera.Brightness * 7.0f + (1.5f - average * 8.0f)) / 8.0f;
                    if(camera.Brightness < 0.6f)
                        camera.Brightness = 0.6f;
                    if(camera.Brightness > 1.0f)
                        camera.Brightness = 1.0f;
                });
            });
        }
    }
}
