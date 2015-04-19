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
using VDGTech.Particles;
using System.Drawing;

namespace VDGTech
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
            WorldPosWriterShader,
            NormalsWriterShader,
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
            WorldPositionFrameBuffer,
            NormalsFrameBuffer,
            ScreenSpaceNormalsFrameBuffer,
            SmallFrameBuffer,
            GlobalIlluminationFrameBuffer,
            DiffuseColorFrameBuffer;

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

            DiffuseColorFrameBuffer = new Framebuffer(initialWidth , initialHeight);

            Pass1FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            Pass2FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            WorldPositionFrameBuffer = new Framebuffer(initialWidth, initialHeight);
            NormalsFrameBuffer = new Framebuffer(initialWidth, initialHeight);
            ScreenSpaceNormalsFrameBuffer = new Framebuffer(initialWidth / 3, initialHeight / 3);

            LightPointsFrameBuffer = new Framebuffer(initialWidth / 6, initialHeight / 6);
            BloomFrameBuffer = new Framebuffer(initialWidth / 6, initialHeight / 3);
            FogFramebuffer = new Framebuffer(initialWidth / 2, initialHeight / 2);
            SmallFrameBuffer = new Framebuffer(initialWidth / 10, initialHeight / 10);

            GlobalIlluminationFrameBuffer = new Framebuffer(initialWidth /1, initialHeight /1);
            //BackDiffuseFrameBuffer = new Framebuffer(initialWidth / 2, initialHeight  / 2);
            //BackNormalsFrameBuffer = new Framebuffer(initialWidth / 2, initialHeight / 2); 

            WorldPosWriterShader = ShaderProgram.Compile("Generic.vertex.glsl", "WorldPosWriter.fragment.glsl");
            NormalsWriterShader = ShaderProgram.Compile("Generic.vertex.glsl", "NormalsWriter.fragment.glsl");
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
            LastFrameBuffer.UseTexture(0);
            LastFrameBuffer = Pass1FrameBuffer;
            return Pass1FrameBuffer;
        }
        private Framebuffer SwitchToFB2()
        {
            Pass2FrameBuffer.Use();
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
        private void GlobalIllumination()
        {
            GIPassOdd = !GIPassOdd;
            if(GIPassOdd)
                GlobalIlluminationShaderX.Use();
            else
                GlobalIlluminationShaderY.Use();
            ShaderProgram.Lock = true;
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

        private void SSAO()
        {
            SSAOShader.Use();
            WorldPositionFrameBuffer.UseTexture(30);
            NormalsFrameBuffer.UseTexture(31);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void Deferred()
        {
            DeferredShader.Use();
            WorldPositionFrameBuffer.UseTexture(30);
            NormalsFrameBuffer.UseTexture(31);
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
            WorldPositionFrameBuffer.UseTexture(30);
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

            DisableBlending();

            // we dont need particles in normals and world pos passes so
            // DRAWING WORLD POS
            WorldPosWriterShader.Use();
            ShaderProgram.Lock = true;
            WorldPositionFrameBuffer.Use();
            World.Root.Draw(true);
            ShaderProgram.Lock = false;

            // DRAWING NORMALS
            NormalsWriterShader.Use();
            ShaderProgram.Lock = true;
            NormalsFrameBuffer.Use();
            World.Root.Draw();
            ShaderProgram.Lock = false;

            // DRAWING MESH OPTIONS
            ScreenSpaceNormalsWriterShader.Use();
            ShaderProgram.Lock = true;
            ScreenSpaceNormalsFrameBuffer.Use();
            World.Root.Draw();
            ShaderProgram.Lock = false;

            EnableFullBlend();
            if(UseMSAA)
            {
                // DRAWING MSAA
                MSAAResolvingFrameBuffer.Use();
                World.Root.Draw();
                //ParticleSystem.DrawAll();

                LastFrameBuffer = MSAAResolvingFrameBuffer;

                // we are into the game! We have world pos and normals, and MSAA scene
                // Scene is already drawn into MSAA framebuffer so need to resolve it

                SwitchToFB1();
                MSAAShader.Use();
                ShaderProgram.Lock = true;
                PostProcessingMesh.Draw();
                ShaderProgram.Lock = false;
            }
            else
            {
                LastFrameBuffer = WorldPositionFrameBuffer;
                SwitchToFB1();
                World.Root.Draw();
            }

            SwitchToFB(DiffuseColorFrameBuffer);
            LastFrameBuffer.UseTexture(0);
            Blit();

            if(UseLightPoints)
            {
                SwitchToFB(LightPointsFrameBuffer);
                LastFrameBuffer.UseTexture(0);
                LightsPoints();
            }

            if(UseFog)
            {
                SwitchToFB(FogFramebuffer);
                DiffuseColorFrameBuffer.UseTexture(0);
                Fog();
            }

            //SwitchBetweenFB();
            //SSAO();

            if(UseDeferred)
            {
                WorldPosWriterShader.Use();
                ShaderProgram.Lock = true;
                BloomFrameBuffer.Use();
                GL.CullFace(CullFaceMode.Front);
                World.Root.Draw(true);
                GL.CullFace(CullFaceMode.Back);
                ShaderProgram.Lock = false;

                SwitchBetweenFB();
                DiffuseColorFrameBuffer.UseTexture(0);
                BloomFrameBuffer.UseTexture(29);
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
                DiffuseColorFrameBuffer.UseTexture(2);
                WorldPositionFrameBuffer.UseTexture(3);
                NormalsFrameBuffer.UseTexture(4);
                p2.UseTexture(5);
                ScreenSpaceNormalsFrameBuffer.UseTexture(7);
                //BackDiffuseFrameBuffer.UseTexture(8);
                //BackDiffuseFrameBuffer.UseTexture(7);
                //BackNormalsFrameBuffer.UseTexture(9);
                GlobalIllumination();
                SwitchToFB(p2);
            }
            /*SwitchToFB(GlobalIlluminationFrameBuffer);

            ReflectShader.Use();
            LastFrameBuffer.UseTexture(0);
            ShaderProgram.Lock = true;
            //WorldPositionFrameBuffer.UseTexture(3);
            //NormalsFrameBuffer.UseTexture(4);
            ScreenSpaceNormalsFrameBuffer.UseTexture(5);
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;*/


            p1.UseTexture(0);
            FogFramebuffer.UseTexture(2);
            LightPointsFrameBuffer.UseTexture(4);
            BloomFrameBuffer.UseTexture(5);
            GlobalIlluminationFrameBuffer.UseTexture(6);
            DiffuseColorFrameBuffer.UseTexture(7);
            NormalsFrameBuffer.UseTexture(8);
            WorldPositionFrameBuffer.UseTexture(9);
            //BackDepthFrameBuffer.UseTexture(6);

            

            Combine();


            //SwitchToFB(SmallFrameBuffer);
           // Pass2FrameBuffer.UseTexture(0);
           // Blit();

           // SwitchBetweenFB();
           // if(World.Root.SkyDome != null) World.Root.SkyDome.Draw();
            //LensBlur();

            SwitchToFB0();
            HDR();
            if(World.Root != null && World.Root.UI != null)
                World.Root.UI.DrawAll();
        }

        private void SetLightingUniforms(ShaderProgram shader)
        {
            shader.SetUniformArray("LightsPs", LightPool.GetPMatrices());
            shader.SetUniformArray("LightsVs", LightPool.GetVMatrices());
            shader.SetUniformArray("LightsPos", LightPool.GetPositions());
            shader.SetUniformArray("LightsFarPlane", LightPool.GetFarPlanes());
            shader.SetUniformArray("LightsColors", LightPool.GetColors());
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
