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
            FogShader, 
            HDRShader,
            BlitShader, 
            DeferredShader, 
            CombinerShader,
            PathTracerOutputShader,
            MotionBlurShader,
            SSAOShader,
            IndirectShader;

        private Framebuffer
            Pass1FrameBuffer,
            Pass2FrameBuffer,
            HelperFullResFrameBuffer,
            LastWorldPosFramebuffer,
            LastDeferredFramebuffer,
            BloomFrameBuffer,
            IndirectFramebuffer,
            SSAOFramebuffer,
            FogFramebuffer,
            SSAOFramebufferFullRes,
            IndirectFramebufferFullRes;

        public MRTFramebuffer MRT;
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

        public ShaderStorageBuffer DensityPoints;
        public int DensityPointsCount = 0;

        private ComputeShader CShader, BlurShader;

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
            DensityPoints = new ShaderStorageBuffer();
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
            RandomsSSBO.MapData(JitterRandomSequenceGenerator.Generate(1, 16 * 16 * 16, true).ToArray());

            MRT = new MRTFramebuffer(initialWidth / 1, initialHeight / 1);

            Pass1FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            Pass2FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            
            BloomFrameBuffer = new Framebuffer(initialWidth / 3, initialHeight / 3);
            FogFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2);
            LastWorldPosFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1);
            HelperFullResFrameBuffer = new Framebuffer(initialWidth / 1, initialHeight / 1);
            LastDeferredFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2);
            IndirectFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2);
            SSAOFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2);

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

            BlurShader = new ComputeShader("Blur.compute.glsl");


            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices, postProcessingPlaneIndices);
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, new GenericMaterial(Color.Pink));
        }

        private Framebuffer LastFrameBuffer;

        enum BlurMode {
            Linear, Gaussian, Temporal, Additive
        }

        private void Blur(Framebuffer output, Framebuffer helper, Framebuffer source, int length)
        {
           // GL.BindImageTexture(0, output.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);
            GL.BindImageTexture(1, helper.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);
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
            GL.BindImageTexture(1, helper.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);
            BlurShader.SetUniform("Length", length);
            BlurShader.SetUniform("Direction", 1);
            BlurShader.SetUniform("FarPlane", Camera.Current.Far);
            BlurShader.Dispatch(output.Width / 32 + 1, output.Height / 32 + 1, 1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
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
            if((buffer == Pass1FrameBuffer || buffer == Pass2FrameBuffer) && LastFrameBuffer != null)
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
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        static Random Rand = new Random();


        private void Deferred()
        {
            DeferredShader.Use();
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureNormals(16);
            MRT.UseTextureMeshData(17);
            World.Root.RootScene.SetLightingUniforms(DeferredShader, Matrix4.Identity);
            MRT.UseTextureDiffuseColor(14);
            MRT.UseTextureDepth(1);
            DeferredShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            DeferredShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            DeferredShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            CubeMap.Use(TextureUnit.Texture19);
            LastDeferredFramebuffer.UseTexture(20);
            DeferredShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        private void SSAO()
        {
            SSAOShader.Use();
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureDepth(1);
            MRT.UseTextureNormals(16);
            MRT.UseTextureMeshData(17);
            SSAOShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            SSAOShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        private void Indirect()
        {
            IndirectShader.Use();
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureNormals(16);
            MRT.UseTextureMeshData(17);
            World.Root.RootScene.SetLightingUniforms(IndirectShader, Matrix4.Identity);
            MRT.UseTextureDiffuseColor(14);
            MRT.UseTextureDepth(1);
            IndirectShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            IndirectShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            CubeMap.Use(TextureUnit.Texture19);
            LastDeferredFramebuffer.UseTexture(20);
            IndirectShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
            // DeferredShader.SetUniform("FrameINT", (int)RandomIntFrame);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        private void Fog()
        {
            FogShader.Use();
            World.Root.RootScene.SetLightingUniforms(FogShader, Matrix4.Identity);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureNormals(16);
            FogShader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        private void HDR(long time)
        {
            string lastTimeSTR = time.ToString();
            //Console.WriteLine(time);
            int[] nums = lastTimeSTR.ToCharArray().Select<char, int>((a) => a - 48).ToArray();
            HDRShader.Use();
            HDRShader.SetUniform("UseBloom", GLThread.GraphicsSettings.UseBloom);
            HDRShader.SetUniform("NumbersCount", nums.Length);
            HDRShader.SetUniformArray("Numbers", nums);
            if(Camera.MainDisplayCamera != null)
            {
                HDRShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                HDRShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            MRT.UseTextureDiffuseColor(14);
            MRT.UseTextureDepth(1);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }


        private void Blit()
        {
            BlitShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        private void Combine()
        {
            CombinerShader.Use();
            World.Root.RootScene.SetLightingUniforms(CombinerShader, Matrix4.Identity);
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
            PostProcessingMesh.Draw(Matrix4.Identity);
            ShaderProgram.Lock = false;
        }
        private void MotionBlur()
        {
            MotionBlurShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw(Matrix4.Identity);
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
              //  return;
            }
            stopwatch.Stop();
            lastTime = (lastTime * 20 + stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L))) / 21;
            stopwatch.Reset();
            stopwatch.Start();
            
            DisableBlending();
           // RandomsSSBO.MapData(JitterRandomSequenceGenerator.Generate(1, 16 * 16 * 16, true).ToArray());

            Mesh3d.PostProcessingUniformsOnly = false;
            GL.CullFace(CullFaceMode.Back);
            MRT.Use();
            World.Root.Draw();
            Mesh3d.PostProcessingUniformsOnly = true;
            
            if(GLThread.GraphicsSettings.UseFog)
            {
                SwitchToFB(FogFramebuffer);
                MRT.UseTextureDiffuseColor(14);
                MRT.UseTextureWorldPosition(15);
                MRT.UseTextureDepth(1);
                Fog();
            }
            if(GLThread.GraphicsSettings.UseRSM || GLThread.GraphicsSettings.UseHBAO)
            {
                SwitchToFB(IndirectFramebuffer);
                Indirect();
                SwitchToFB(LastDeferredFramebuffer);
                IndirectFramebuffer.UseTexture(0);
                Blit();
                SwitchToFB(SSAOFramebuffer);
                SSAO();

                SwitchToFB(HelperFullResFrameBuffer);
                Blur(HelperFullResFrameBuffer, HelperFullResFrameBuffer, SSAOFramebuffer, 14);
            }
            // Blur(IndirectFramebuffer.TexColor, 8, IndirectFramebuffer.Width, IndirectFramebuffer.Height, BlurMode.Temporal);
            // Blur(SSAOFramebuffer.TexColor, 8, SSAOFramebuffer.Width, SSAOFramebuffer.Height, BlurMode.Temporal);
            // Blur(IndirectFramebuffer.TexColor, 8, IndirectFramebuffer.Width, IndirectFramebuffer.Height, BlurMode.Temporal);
            if(GLThread.GraphicsSettings.UseDeferred)
            {
                SwitchToFB(Pass2FrameBuffer);
                MRT.UseTextureDiffuseColor(14);
                MRT.UseTextureDepth(1);
                MRT.UseTextureWorldPosition(15);
                MRT.UseTextureNormals(16);
                MRT.UseTextureMeshData(17);
                MRT.UseTextureId(18);
                HelperFullResFrameBuffer.UseTexture(23);
                Deferred();
            }


            // SwitchToFB(Pass2FrameBuffer);

            /*  Blur(SSAOFramebuffer.TexColor, Pass1FrameBuffer.TexColor, 28, SSAOFramebuffer.Width, SSAOFramebuffer.Height, Pass1FrameBuffer.Width, Pass1FrameBuffer.Height, BlurMode.Temporal);
              Blur(IndirectFramebuffer.TexColor, Pass1FrameBuffer.TexColor, 28, IndirectFramebuffer.Width, IndirectFramebuffer.Height, Pass1FrameBuffer.Width, Pass1FrameBuffer.Height, BlurMode.Temporal);
              */


            SwitchToFB(Pass1FrameBuffer);

            Pass2FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            MRT.UseTextureDiffuseColor(14);
            MRT.UseTextureWorldPosition(15);
            MRT.UseTextureNormals(16);
            MRT.UseTextureMeshData(17);
            LastWorldPosFramebuffer.UseTexture(21);
            IndirectFramebuffer.UseTexture(22);
            HelperFullResFrameBuffer.UseTexture(23);
            FogFramebuffer.UseTexture(24);

            Combine();


            if(GLThread.GraphicsSettings.UseBloom)
            {
                SwitchToFB(BloomFrameBuffer);
                Pass1FrameBuffer.UseTexture(0);
                Bloom();
            }

         /*   SwitchToFB(Pass2FrameBuffer);
            Pass1FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            LastWorldPosFramebuffer.UseTexture(21);
            MRT.UseTextureWorldPosition(15);

            MotionBlur();*/


            SwitchToFB0();

            Pass1FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            BloomFrameBuffer.UseTexture(26);
            MRT.UseTextureWorldPosition(15);
            NumbersTexture.Use(TextureUnit.Texture25);

            HDR(lastTime == 0 ? 0 : 1000000 / lastTime);
            SwitchToFB(LastWorldPosFramebuffer);
            MRT.UseTextureWorldPosition(0);
            Blit();
            EnableFullBlend();


            if(World.Root != null && World.Root.UI != null)
                World.Root.UI.DrawAll();
            Mesh3d.PostProcessingUniformsOnly = false;
        }
        
    }
}
