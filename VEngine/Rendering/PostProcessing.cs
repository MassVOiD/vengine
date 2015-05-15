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

namespace VEngine
{
    public class PostProcessing
    {
        private int Width, Height;

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
            ScreenSpaceNormalsWriterShader;

        private Framebuffer
            MSAAResolvingFrameBuffer,
            Pass1FrameBuffer,
            Pass2FrameBuffer,
            LightPointsFrameBuffer,
            BloomFrameBuffer,
            FogFramebuffer,
            SmallFrameBuffer,
            GlobalIlluminationFrameBuffer,
            LastWorldPositionFramebuffer;

        private MRTFramebuffer MRT;

        private Mesh3d PostProcessingMesh;

        private static uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

        private static float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        public PostProcessing(int initialWidth, int initialHeight)
        {
            Width = initialWidth;
            Height = initialHeight;
            MSAAResolvingFrameBuffer = new Framebuffer(initialWidth, initialHeight);
            MSAAResolvingFrameBuffer.SetMultiSample(true);

            MRT = new MRTFramebuffer(initialWidth , initialHeight);

            Pass1FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            Pass2FrameBuffer = new Framebuffer(initialWidth, initialHeight);

            LightPointsFrameBuffer = new Framebuffer(initialWidth / 6, initialHeight / 6);
            BloomFrameBuffer = new Framebuffer(initialWidth / 4, initialHeight / 4);
            FogFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1);
            SmallFrameBuffer = new Framebuffer(initialWidth / 10, initialHeight / 10);
            LastWorldPositionFramebuffer = new Framebuffer(initialWidth / 1, initialHeight / 1);

            GlobalIlluminationFrameBuffer = new Framebuffer(initialWidth / 1, initialHeight /1);
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
            GlobalIlluminationShaderX = ShaderProgram.Compile("PostProcess.vertex.glsl", "GlobalIllumination.fragment.glsl");
            GlobalIlluminationShaderX.SetGlobal("SEED", "gl_FragCoord.x");
            GlobalIlluminationShaderY = ShaderProgram.Compile("PostProcess.vertex.glsl", "GlobalIllumination.fragment.glsl");
            GlobalIlluminationShaderY.SetGlobal("SEED", "gl_FragCoord.y");
            //ReflectShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Reflect.fragment.glsl"));

            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices, postProcessingPlaneIndices);
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, new SolidColorMaterial(Color.Pink));
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
            MRT.UseTextureWorldPosition(30);
            MRT.UseTextureNormals(31);
            LightPool.UseTextures(2);
            LightPool.MapSimpleLightsToShader(DeferredShader);
            SetLightingUniforms(DeferredShader);
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
        private void HDR()
        {
            HDRShader.Use();
            HDRShader.SetUniform("Brightness", Camera.Current.Brightness);
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
            CombinerShader.SetUniform("UseSimpleGI", UseSimpleGI);
            CombinerShader.SetUniform("UseFog", UseFog);
            CombinerShader.SetUniform("UseLightPoints", UseLightPoints);
            CombinerShader.SetUniform("UseDepth", UseDepth);
            CombinerShader.SetUniform("UseBloom", UseBloom);
            CombinerShader.SetUniform("UseDeferred", UseDeferred);
            CombinerShader.SetUniform("UseBilinearGI", UseBilinearGI);
            ShaderProgram.Lock = true;
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

        public bool UseSimpleGI = true;
        public bool UseFog = true;
        public bool UseLightPoints = true;
        public bool UseDepth = false;
        public bool UseBloom = true;
        public bool UseDeferred = true;
        public bool UseBilinearGI = false;
        public bool UseMSAA = false;

        public void ExecutePostProcessing()
        {
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

            //WriteBackDepth();

            LastWorldPositionFramebuffer.Use();
            MRT.UseTextureWorldPosition(0);
            Blit();

            DisableBlending();
            Mesh3d.PostProcessingUniformsOnly = false;
            MRT.Use();
            World.Root.Draw();
            Mesh3d.PostProcessingUniformsOnly = true;

            if(UseLightPoints)
            {
                SwitchToFB(LightPointsFrameBuffer);
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                LightsPoints();
            }

            if(UseFog)
            {
                SwitchToFB(FogFramebuffer);
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                Fog();
            }

            //SwitchBetweenFB();
            //SSAO();

            if(UseDeferred)
            {
                SwitchBetweenFB();
                MRT.UseTextureDiffuseColor(0);
                MRT.UseTextureDepth(1);
                MRT.UseTextureWorldPosition(30);
                MRT.UseTextureNormals(31);
                //BloomFrameBuffer.UseTexture(29);
                Deferred();
            }

            if(UseBloom)
            {
                SwitchToFB(BloomFrameBuffer);
                LastFrameBuffer.UseTexture(0);
                Bloom();
            }


            var p1 = SwitchBetweenFB();
            var p2 = p1 == Pass1FrameBuffer ? Pass2FrameBuffer : Pass1FrameBuffer;
            p2.UseTexture(0);
            Blit();
            SwitchBetweenFB();
            if(UseBilinearGI || UseSimpleGI)
            {
                GlobalIlluminationFrameBuffer.UseTexture(0);
                Blit();
                /*
                SwitchToFB(BackDiffuseFrameBuffer);
                BackDepthWriterShader.Use();
                ShaderProgram.Lock = true;
                World.Root.Draw();
                ShaderProgram.Lock = false;*/

                SwitchToFB(GlobalIlluminationFrameBuffer);
                p1.UseTexture(0);
                MRT.UseTextureDepth(1);
                MRT.UseTextureDiffuseColor(2);
                MRT.UseTextureWorldPosition(3);
                MRT.UseTextureNormals(4);
                p2.UseTexture(5);
                //ScreenSpaceMRT.UseTextureNormals(7);
                MRT.UseTextureMeshData(8);
                //BackDiffuseFrameBuffer.UseTexture(7);
                //BackMRT.UseTextureNormals(9);
                GlobalIllumination();
                SwitchToFB(p2);
            }
            /*SwitchToFB(GlobalIlluminationFrameBuffer);

            ReflectShader.Use();
            LastFrameBuffer.UseTexture(0);
            ShaderProgram.Lock = true;
            //MRT.UseTextureWorldPosition(3);
            //MRT.UseTextureNormals(4);
            ScreenSpaceMRT.UseTextureNormals(5);
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;*/


            p1.UseTexture(0);
            MRT.UseTextureDepth(1);
            FogFramebuffer.UseTexture(2);
            LightPointsFrameBuffer.UseTexture(4);
            BloomFrameBuffer.UseTexture(5);
            GlobalIlluminationFrameBuffer.UseTexture(6);
            MRT.UseTextureDiffuseColor(7);
            MRT.UseTextureNormals(8);
            MRT.UseTextureWorldPosition(9);
            LastWorldPositionFramebuffer.UseTexture(10);
            //BackDepthFrameBuffer.UseTexture(6);

            

            Combine();


            SwitchToFB(SmallFrameBuffer);
            MRT.UseTextureDiffuseColor(0);
            MRT.UseTextureDepth(1);
            Blit();

           // SwitchBetweenFB();
           // if(World.Root.SkyDome != null) World.Root.SkyDome.Draw();
            //LensBlur();

            SwitchToFB0();
            
            HDR();
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
