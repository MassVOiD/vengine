using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    public enum ShadowMapQuality
    {
        High, Medium, Low
    }

    public class ShadowMapper
    {
        private CubeMapDepthFramebuffer CubeHighQuality, CubeMediumQuality, CubeLowQuality;
        private Framebuffer SpotHighQuality, SpotMediumQuality, SpotLowQuality;
        private Camera SingleCamera;

        public ShadowMapper()
        {
            CubeHighQuality = new CubeMapDepthFramebuffer(1024, 1024);
            CubeMediumQuality = new CubeMapDepthFramebuffer(512, 512);
            CubeLowQuality = new CubeMapDepthFramebuffer(256, 256);

            SpotHighQuality = new Framebuffer(1024, 1024, true);
            SpotMediumQuality = new Framebuffer(1024, 1024, true);
            SpotLowQuality = new Framebuffer(512, 512, true);

            SingleCamera = new Camera(Vector3.Zero, Vector3.One, Vector2.One, 0.01f, 100.0f);
        }

        public Matrix4 MapSingle(ShadowMapQuality quality, TransformationManager transformation, float angle, float cutoff)
        {
            Camera last = Camera.Current;
            SingleCamera.UpdatePerspective(1, angle, 0.01f, cutoff);
            SingleCamera.Transformation = transformation;
            SingleCamera.Update();
            SingleCamera.Transformation.ClearModifiedFlag();

            Camera.Current = SingleCamera;
            if(quality == ShadowMapQuality.High)
                SpotHighQuality.Use();
            if(quality == ShadowMapQuality.Medium)
                SpotMediumQuality.Use();
            if(quality == ShadowMapQuality.Low)
                SpotLowQuality.Use();

            RenderWorld();

            Camera.Current = last;
            return SingleCamera.GetVPMatrix();
        }

        public void MapCube(ShadowMapQuality quality, TransformationManager transformation, float cutoff)
        {
            Camera last = Camera.Current;
            CubeMapDepthFramebuffer cfb = null;
            if(quality == ShadowMapQuality.High)
                cfb = CubeHighQuality;
            if(quality == ShadowMapQuality.Medium)
                cfb = CubeMediumQuality;
            if(quality == ShadowMapQuality.Low)
                cfb = CubeLowQuality;
            cfb.UpdateFarPlane(cutoff);

            cfb.Use();
            cfb.Clear();
            cfb.SetPosition(transformation.Position);

            cfb.SwitchCameraAndFace(OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapPositiveX);
            RenderWorld();
            cfb.SwitchCameraAndFace(OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapPositiveY);
            RenderWorld();
            cfb.SwitchCameraAndFace(OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapPositiveZ);
            RenderWorld();

            cfb.SwitchCameraAndFace(OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapNegativeX);
            RenderWorld();
            cfb.SwitchCameraAndFace(OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapNegativeY);
            RenderWorld();
            cfb.SwitchCameraAndFace(OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapNegativeZ);
            RenderWorld();
            
            Camera.Current = last;
        }

        private void RenderWorld()
        {
            GenericMaterial.OverrideShaderPack = Game.ShaderPool.ChooseShaderDepth();
            Game.World.SetUniforms(Game.DisplayAdapter.MainRenderer);
            InternalRenderingState.PassState = InternalRenderingState.State.ShadowMapPass;
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
        }

        public void UseTextureSingle(ShadowMapQuality quality, int index)
        {
            if(quality == ShadowMapQuality.High)
                SpotHighQuality.UseTexture(index);
            if(quality == ShadowMapQuality.Medium)
                SpotMediumQuality.UseTexture(index);
            if(quality == ShadowMapQuality.Low)
                SpotLowQuality.UseTexture(index);
        }

        public void UseTextureCube(ShadowMapQuality quality, int index)
        {
            if(quality == ShadowMapQuality.High)
                CubeHighQuality.UseTexture(index);
            if(quality == ShadowMapQuality.Medium)
                CubeMediumQuality.UseTexture(index);
            if(quality == ShadowMapQuality.Low)
                CubeLowQuality.UseTexture(index);
        }
    }
}
