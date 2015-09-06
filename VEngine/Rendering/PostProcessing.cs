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
            PathTracerOutputShader;

        private Framebuffer
            Pass1FrameBuffer,
            Pass2FrameBuffer,
            BloomFrameBuffer,
            FogFramebuffer;

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
            
            BloomFrameBuffer = new Framebuffer(initialWidth / 5, initialHeight / 5);
            FogFramebuffer = new Framebuffer(initialWidth/2, initialHeight/2);
           
            BloomShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Bloom.fragment.glsl");
            FogShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Fog.fragment.glsl");
            HDRShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "HDR.fragment.glsl");
            BlitShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Blit.fragment.glsl");
            DeferredShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Deferred.fragment.glsl");
            CombinerShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Combiner.fragment.glsl");
            PathTracerOutputShader = ShaderProgram.Compile("PostProcess.vertex.glsl", "Output.fragment.glsl");
            
            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices, postProcessingPlaneIndices);
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, new GenericMaterial(Color.Pink));
        }

        private Framebuffer LastFrameBuffer;

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
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        static Random Rand = new Random();
       

        private void Deferred()
        {
            DeferredShader.Use();
            // GL.BindImageTexture(11, (uint)FullScene3DTexture.Handle, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            MRT.UseTextureMeshData(33);
            LightPool.UseTextures(2);
            LightPool.MapSimpleLightsToShader(DeferredShader);
            SetLightingUniforms(DeferredShader);
            MRT.UseTextureDiffuseColor(0);
            MRT.UseTextureDepth(1);
            DeferredShader.SetUniform("UseVDAO", GLThread.GraphicsSettings.UseVDAO);
            DeferredShader.SetUniform("UseHBAO", GLThread.GraphicsSettings.UseHBAO);
            DeferredShader.SetUniform("UseRSM", GLThread.GraphicsSettings.UseRSM);
            CubeMap.Use(TextureUnit.Texture29);

            DeferredShader.SetUniform("RandomsCount", 16 * 16 * 16);
            RandomsSSBO.Use(6);
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
            HDRShader.SetUniform("UseBloom", GLThread.GraphicsSettings.UseBloom);
            HDRShader.SetUniform("NumbersCount", nums.Length);
            HDRShader.SetUniformArray("Numbers", nums);
            if(Camera.MainDisplayCamera != null)
            {
                HDRShader.SetUniform("CameraCurrentDepth", Camera.MainDisplayCamera.CurrentDepthFocus);
                HDRShader.SetUniform("LensBlurAmount", Camera.MainDisplayCamera.LensBlurAmount);
            }
            MRT.UseTextureDiffuseColor(2);
            MRT.UseTextureDepth(3);
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
            CombinerShader.SetUniform("DensityPointsCount", DensityPointsCount);
            DensityPoints.Use(6);
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

            Mesh3d.PostProcessingUniformsOnly = false;
            GL.CullFace(CullFaceMode.Back);
            MRT.Use();
            World.Root.Draw();
            Mesh3d.PostProcessingUniformsOnly = true;
            
            if(GLThread.GraphicsSettings.UseFog)
            {
                SwitchToFB(FogFramebuffer);
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                Fog();
            }
            
            if(GLThread.GraphicsSettings.UseDeferred)
            {
                SwitchToFB(Pass2FrameBuffer);
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                MRT.UseTextureWorldPosition(30);
                MRT.UseTextureNormals(31);
                MRT.UseTextureMeshData(33);
                MRT.UseTextureId(34);
                Deferred();
            }
            

            SwitchToFB(Pass1FrameBuffer);
            Pass2FrameBuffer.UseTexture(0);
            MRT.UseTextureDepth(1);
            FogFramebuffer.UseTexture(2);
            MRT.UseTextureDiffuseColor(7);
            MRT.UseTextureNormals(8);
            MRT.UseTextureWorldPosition(9);
            MRT.UseTextureMeshData(11);

            Combine();

            if(GLThread.GraphicsSettings.UseBloom)
            {
                SwitchToFB(BloomFrameBuffer);
                Pass1FrameBuffer.UseTexture(0);
                Bloom();
            }
            SwitchToFB0();
            Pass1FrameBuffer.UseTexture(0);
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
        
    }
}
