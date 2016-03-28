using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Voxel3dTextureWriter
    {
        private Texture3D TextureRed, TextureGreen, TextureBlue, TextureCount, TextureNormalX, TextureNormalY, TextureNormalZ, TextureNormalResolved;
        private List<Texture3D> TextureResolvedMipMaps;
        private Framebuffer FBO;
        private Camera RenderingCamera;
        private ShaderPool.ShaderPack Shader;
        private ComputeShader TextureResolverShader, TextureResolverNormalShader, TextureMipmapShader;
        private Quaternion XForward, YForward, ZForward;
        private int GridSize;
        private float BoxSize;

        public Voxel3dTextureWriter(int gridsize, float boxsize)
        {
            GridSize = gridsize;
            BoxSize = boxsize;
            TextureResolverShader = new ComputeShader("Texture3DResolve.compute.glsl");
            TextureResolverNormalShader = new ComputeShader("Texture3DResolveNormal.compute.glsl");
            TextureMipmapShader = new ComputeShader("Texture3DMipmap.compute.glsl");

            TextureRed = new Texture3D(gridsize, gridsize, gridsize);
            TextureGreen = new Texture3D(gridsize, gridsize, gridsize);
            TextureBlue = new Texture3D(gridsize, gridsize, gridsize);

            TextureNormalX = new Texture3D(gridsize, gridsize, gridsize)
            {
                ColorInternalFormat = PixelInternalFormat.R32i,
                ColorPixelFormat = PixelFormat.RedInteger,
                ColorPixelType = PixelType.Int
            };
            TextureNormalY = new Texture3D(gridsize, gridsize, gridsize)
            {
                ColorInternalFormat = PixelInternalFormat.R32i,
                ColorPixelFormat = PixelFormat.RedInteger,
                ColorPixelType = PixelType.Int
            };
            TextureNormalZ = new Texture3D(gridsize, gridsize, gridsize)
            {
                ColorInternalFormat = PixelInternalFormat.R32i,
                ColorPixelFormat = PixelFormat.RedInteger,
                ColorPixelType = PixelType.Int
            };
            TextureNormalResolved = new Texture3D(gridsize, gridsize, gridsize)
            {
                ColorInternalFormat = PixelInternalFormat.Rgba16f,
                ColorPixelFormat = PixelFormat.Rgba,
                ColorPixelType = PixelType.HalfFloat
            };

            TextureCount = new Texture3D(gridsize, gridsize, gridsize);

            TextureResolvedMipMaps = new List<Texture3D>();
            int sz = gridsize;
            while(sz > 4)
            {
                TextureResolvedMipMaps.Add(new Texture3D(sz, sz, sz)
                {
                    ColorInternalFormat = PixelInternalFormat.Rgba16f,
                    ColorPixelFormat = PixelFormat.Rgba,
                    ColorPixelType = PixelType.HalfFloat
                });
                sz = sz / 2;
            }

            FBO = new Framebuffer(gridsize, gridsize, false)
            {
            };

            RenderingCamera = new Camera();
            float boxhalf = boxsize * 0.5f;
            RenderingCamera.UpdatePerspectiveOrtho(-boxhalf, boxhalf, -boxhalf, boxhalf, -boxhalf, boxhalf);

            Shader = new ShaderPool.ShaderPack("Voxel3dTextureWriter.fragment.glsl");

            XForward = Matrix4.LookAt(Vector3.Zero, Vector3.UnitX, Vector3.UnitY).ExtractRotation();
            YForward = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ).ExtractRotation();
            ZForward = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY).ExtractRotation();
        }

        private Vector3 Floor(Vector3 input)
        {
            return new Vector3(
                (float)Math.Floor(input.X),
                (float)Math.Floor(input.Y),
                (float)Math.Floor(input.Z)
            );
        }

        public void Map()
        {
            var lastCamera = Camera.Current;

            var lights = Game.World.Scene.GetLights();

            var light = lights[0];

            for(int i = 0; i < Shader.ProgramsList.Length; i++)
            {
                var s = Shader.ProgramsList[i];
                s.Use();
                s.SetUniform("GridSize", GridSize);
                s.SetUniform("BoxSize", BoxSize);
                light.SetUniforms();
                light.BindShadowMap(20, 21);
            }

            TextureRed.Clear();
            TextureGreen.Clear();
            TextureBlue.Clear();
            TextureCount.Clear();
            //TextureResolved.Clear();

            FBO.Use();
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            TextureRed.BindImageUnit(6, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            TextureGreen.BindImageUnit(1, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            TextureBlue.BindImageUnit(2, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            TextureCount.BindImageUnit(3, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);

            TextureNormalX.BindImageUnit(7, TextureAccess.ReadWrite, SizedInternalFormat.R32i);
            TextureNormalY.BindImageUnit(8, TextureAccess.ReadWrite, SizedInternalFormat.R32i);
            TextureNormalZ.BindImageUnit(9, TextureAccess.ReadWrite, SizedInternalFormat.R32i);

            Camera.Current = RenderingCamera;
            var vstep = BoxSize / (float)GridSize;
            var opos = lastCamera.Transformation.Position;
            var rpos = new Vector3(vstep) * Floor(opos.Div(vstep));
            RenderingCamera.SetPosition(Vector3.Zero);

            RenderingCamera.SetOrientation(XForward);
            RenderingCamera.Update();
            RenderWorld();

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            RenderingCamera.SetOrientation(YForward);
            RenderingCamera.Update();
            RenderWorld();

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            RenderingCamera.SetOrientation(ZForward);
            RenderingCamera.Update();
            RenderWorld();

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            TextureResolvedMipMaps[0].BindImageUnit(4, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);

            TextureRed.Use(TextureUnit.Texture0);
            TextureGreen.Use(TextureUnit.Texture1);
            TextureBlue.Use(TextureUnit.Texture2);
            TextureCount.Use(TextureUnit.Texture3);

            TextureResolverShader.Use();
            TextureResolverShader.Dispatch(GridSize / 16, GridSize / 8, GridSize / 8);

            TextureNormalX.Use(TextureUnit.Texture0);
            TextureNormalY.Use(TextureUnit.Texture1);
            TextureNormalZ.Use(TextureUnit.Texture2);
            TextureCount.Use(TextureUnit.Texture3);
            TextureNormalResolved.BindImageUnit(10, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);

            TextureResolverNormalShader.Use();
            TextureResolverNormalShader.Dispatch(GridSize / 16, GridSize / 8, GridSize / 8);

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            TextureMipmapShader.Use();
            for(int i = 0; i < TextureResolvedMipMaps.Count - 1; i++)
            {
                TextureResolvedMipMaps[i].Use(TextureUnit.Texture0);
                TextureResolvedMipMaps[i + 1].BindImageUnit(4, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);
                TextureMipmapShader.Dispatch(TextureResolvedMipMaps[i + 1].SizeX / 4, TextureResolvedMipMaps[i + 1].SizeY / 4, TextureResolvedMipMaps[i + 1].SizeZ / 4);
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            }


            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);


            //TextureResolved.FreeImageUnit(4);


            Camera.Current = lastCamera;
        }

        public void BindTexture(TextureUnit index)
        {
            TextureNormalResolved.Use(TextureUnit.Texture24);
            for(int i = 0; i < TextureResolvedMipMaps.Count; i++)
            {
                TextureResolvedMipMaps[i].Use(index + i);
            }
        }

        public void BindTextureTest(int index)
        {
            FBO.UseTexture(index);
        }

        public void SetUniforms()
        {
            ShaderProgram.Current.SetUniform("Voxelize_GridSize", GridSize);
            ShaderProgram.Current.SetUniform("Voxelize_BoxSize", BoxSize);
            ShaderProgram.Current.SetUniform("Voxelize_MipmapLevels", TextureResolvedMipMaps.Count);
        }

        private void RenderWorld()
        {
            GenericMaterial.OverrideShaderPack = Shader;
            Game.World.SetUniforms(Game.DisplayAdapter.MainRenderer, Shader);
            InternalRenderingState.PassState = InternalRenderingState.State.ForwardOpaquePass;
            Game.World.Draw();
            InternalRenderingState.PassState = InternalRenderingState.State.Idle;
            GenericMaterial.OverrideShaderPack = null;
        }

    }
}
