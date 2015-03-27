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
    class PostProcessing
    {
        private int Width, Height;
        private ShaderProgram BloomShader, MSAAShader, SSAOShader, FogShader, LightPointsShader, LensBlurShader, HDRShader, WorldPosWriterShader, NormalsWriterShader, BlitShader, DeferredShader;
        private Framebuffer MSAAResolvingFrameBuffer, Pass1FrameBuffer, Pass2FrameBuffer, WorldPositionFrameBuffer, NormalsFrameBuffer;
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

            Pass1FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            Pass2FrameBuffer = new Framebuffer(initialWidth, initialHeight);
            WorldPositionFrameBuffer = new Framebuffer(initialWidth, initialHeight);
            NormalsFrameBuffer = new Framebuffer(initialWidth, initialHeight);

            WorldPosWriterShader = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"), Media.ReadAllText("WorldPosWriter.fragment.glsl"));
            NormalsWriterShader = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"), Media.ReadAllText("NormalsWriter.fragment.glsl"));

            BloomShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Bloom.fragment.glsl"));
            MSAAShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("MSAA.fragment.glsl"));
            SSAOShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("SSAO.fragment.glsl"));
            FogShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Fog.fragment.glsl"));
            LightPointsShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("LightPoints.fragment.glsl"));
            LensBlurShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("LensBlur.fragment.glsl"));
            HDRShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("HDR.fragment.glsl"));
            BlitShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Blit.fragment.glsl"));
            DeferredShader = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Deferred.fragment.glsl"));

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
        private void SwitchToFB1()
        {
            Pass1FrameBuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            LastFrameBuffer.UseTexture(0);
            LastFrameBuffer = Pass1FrameBuffer;
        }
        private void SwitchToFB2()
        {
            Pass2FrameBuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            LastFrameBuffer.UseTexture(0);
            LastFrameBuffer = Pass2FrameBuffer;
        }

        private void EnableFullBlend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void DisableFullBlend()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
        }

        private void Bloom()
        {
            BloomShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private void LightsPoints()
        {
            LightPointsShader.Use();
            LightPool.MapSimpleLightsToShader(LightPointsShader);
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
            LightPool.MapSimpleLightsToShader(LightPointsShader);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }

        private void Fog()
        {
            FogShader.Use();
            WorldPositionFrameBuffer.UseTexture(30);
            FogShader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;
        }
        private void HDR()
        {
            HDRShader.Use();
            HDRShader.SetUniform("Brightness", 1.0f);
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


        public void ExecutePostProcessing()
        {
            MSAAResolvingFrameBuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // and then draw the scene
            World.Root.Draw();
            ParticleSystem.DrawAll();
            if(Skybox.Current != null)
                Skybox.Current.Draw();
            // we dont need particles in normals and world pos passes so
            WorldPosWriterShader.Use();
            ShaderProgram.Lock = true;
            WorldPositionFrameBuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            World.Root.Draw();
            ShaderProgram.Lock = false;

            NormalsWriterShader.Use();
            ShaderProgram.Lock = true;
            NormalsFrameBuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            World.Root.Draw();
            ShaderProgram.Lock = false;
            LastFrameBuffer = MSAAResolvingFrameBuffer;

            // we are into the game! We have world pos and normals, and MSAA scene
            // Scene is already drawn into MSAA framebuffer so need to resolve it

            SwitchToFB1();
            MSAAShader.Use();
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;

            // now we have MSAA filetered img

            
            SwitchToFB2();
            SSAO();
            
            SwitchToFB1();
            LightsPoints();

            SwitchToFB2();
            Deferred();

            SwitchToFB1();
            Fog();

            SwitchToFB2();
            LensBlur();
                
            SwitchToFB0();
            HDR();
        }
    }
}
