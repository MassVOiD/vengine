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
        private Texture3D TextureRed, TextureGreen, TextureBlue, TextureCount;//, TextureNormalX, TextureNormalY, TextureNormalZ, TextureNormalResolved;
        private List<Texture3D> TextureResolvedMipMaps;
        private Framebuffer FBO;
        private Camera RenderingCameraX, RenderingCameraY, RenderingCameraZ;
        private ShaderPool.ShaderPack Shader;
        private ComputeShader TextureResolverShader, TextureMipmapShader;//, TextureResolverNormalShader;
        private Quaternion XForward, YForward, ZForward;
        private int GridSizeX, GridSizeY, GridSizeZ;
        private Vector3 BoxSize, MapPosition;

        public Voxel3dTextureWriter(int gridsizeX, int gridsizeY, int gridsizeZ, Vector3 boxsize, Vector3 staticPosition)
        {
            GridSizeX = gridsizeX;
            GridSizeY = gridsizeY;
            GridSizeZ = gridsizeZ;
            MapPosition = staticPosition;
            BoxSize = boxsize;
            TextureResolverShader = new ComputeShader("Texture3DResolve.compute.glsl");
            //TextureResolverNormalShader = new ComputeShader("Texture3DResolveNormal.compute.glsl");
            TextureMipmapShader = new ComputeShader("Texture3DMipmap.compute.glsl");

            TextureRed = new Texture3D(gridsizeX, gridsizeY, gridsizeZ);
            TextureGreen = new Texture3D(gridsizeX, gridsizeY, gridsizeZ);
            TextureBlue = new Texture3D(gridsizeX, gridsizeY, gridsizeZ);
            /*
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
            */
            TextureCount = new Texture3D(gridsizeX, gridsizeY, gridsizeZ);

            /*
            TEXTURE LAYOUT:
            1 - Initial - RES / 1 - 256
            2 - Blurred 2 PX 1 res / 2 128
            3 - Blurred 2 PX 2 res / 2 64
            4 - Blurred 2 PX 3 res / 2 32
            */

            TextureResolvedMipMaps = new List<Texture3D>();
            int szx = gridsizeX;
            int szy = gridsizeY;
            int szz = gridsizeZ;
            //while(szx > 8 && szy > 8 && szz > 8)
            for(int i=0;i<4;i++)
            {
                TextureResolvedMipMaps.Add(new Texture3D(szx, szy, szz)
                {
                    ColorInternalFormat = PixelInternalFormat.Rgba16f,
                    ColorPixelFormat = PixelFormat.Rgba,
                    ColorPixelType = PixelType.HalfFloat
                });
                szx = szx / 2;
                szy = szy / 2;
                szz = szz / 2;
            }

            FBO = new Framebuffer(256, 256, true)
            {
            };

            XForward = Matrix4.LookAt(Vector3.Zero, Vector3.UnitX, Vector3.UnitY).ExtractRotation();
            YForward = Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ).ExtractRotation();
            ZForward = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY).ExtractRotation();

            Vector3 boxhalf = boxsize * 0.5f;


            RenderingCameraX = new Camera();
            RenderingCameraX.UpdatePerspectiveOrtho(-boxhalf.X, boxhalf.X, -boxhalf.Y, boxhalf.Y, boxhalf.Z, -boxhalf.Z);
            RenderingCameraX.SetOrientation(XForward);
            RenderingCameraX.SetPosition(MapPosition);
            RenderingCameraX.Update();

            RenderingCameraY = new Camera();
            RenderingCameraY.UpdatePerspectiveOrtho(-boxhalf.X, boxhalf.X, -boxhalf.Z, boxhalf.Z, boxhalf.Y, -boxhalf.Y);
            RenderingCameraY.SetOrientation(YForward);
            RenderingCameraY.SetPosition(MapPosition);
            RenderingCameraY.Update();

            RenderingCameraZ = new Camera();
            RenderingCameraZ.UpdatePerspectiveOrtho(-boxhalf.X, boxhalf.X, -boxhalf.Y, boxhalf.Y, boxhalf.Z, -boxhalf.Z);
            RenderingCameraZ.SetOrientation(ZForward);
            RenderingCameraZ.SetPosition(MapPosition);
            RenderingCameraZ.Update();

            Shader = new ShaderPool.ShaderPack("Voxel3dTextureWriter.fragment.glsl");
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
                if(!s.Compiled)
                    continue;
                s.Use();
                s.SetUniform("GridSizeX", GridSizeX);
                s.SetUniform("GridSizeY", GridSizeY);
                s.SetUniform("GridSizeZ", GridSizeZ);
                s.SetUniform("BoxSize", BoxSize);
                s.SetUniform("MapPosition", -MapPosition);
                light.SetUniforms();
                light.BindShadowMap(20, 21);
                SetUniforms();
                BindTexture(TextureUnit.Texture25);
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
            /*
            TextureNormalX.BindImageUnit(7, TextureAccess.ReadWrite, SizedInternalFormat.R32i);
            TextureNormalY.BindImageUnit(8, TextureAccess.ReadWrite, SizedInternalFormat.R32i);
            TextureNormalZ.BindImageUnit(9, TextureAccess.ReadWrite, SizedInternalFormat.R32i);
            */
            //  Camera.Current = RenderingCameraX;
            //  RenderWorld();

            Camera.Current = RenderingCameraX;
            RenderingCameraX.SetOrientation(XForward);
            RenderingCameraX.Update();
            RenderingCameraX.Transformation.ClearModifiedFlag();
            RenderWorld();

            Camera.Current = RenderingCameraY;
            RenderingCameraY.SetOrientation(YForward);
            RenderingCameraY.Update();
            RenderingCameraY.Transformation.ClearModifiedFlag();
            RenderWorld();

            Camera.Current = RenderingCameraZ;
            RenderingCameraZ.SetOrientation(ZForward);
            RenderingCameraZ.Update();
            RenderingCameraZ.Transformation.ClearModifiedFlag();
            RenderWorld();

            //  Camera.Current = RenderingCameraZ;
            // RenderWorld();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            TextureResolvedMipMaps[0].BindImageUnit(4, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);

            TextureRed.Use(TextureUnit.Texture0);
            TextureGreen.Use(TextureUnit.Texture1);
            TextureBlue.Use(TextureUnit.Texture2);
            TextureCount.Use(TextureUnit.Texture3);

            TextureResolverShader.Use();
            TextureResolverShader.Dispatch(GridSizeX / 16, GridSizeY / 8, GridSizeZ / 8);
            /*
            TextureNormalX.Use(TextureUnit.Texture0);
            TextureNormalY.Use(TextureUnit.Texture1);
            TextureNormalZ.Use(TextureUnit.Texture2);
            TextureCount.Use(TextureUnit.Texture3);
            TextureNormalResolved.BindImageUnit(10, TextureAccess.ReadWrite, SizedInternalFormat.Rgba16f);

            TextureResolverNormalShader.Use();
            TextureResolverNormalShader.Dispatch(GridSize / 16, GridSize / 8, GridSize / 8);
            */

            TextureMipmapShader.Use();
            for(int i = 0; i < TextureResolvedMipMaps.Count - 1; i++)
            {
                TextureResolvedMipMaps[i].Use(TextureUnit.Texture0);
                TextureResolvedMipMaps[i + 1].BindImageUnit(4, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);
                TextureMipmapShader.Dispatch(TextureResolvedMipMaps[i + 1].SizeX / 8, TextureResolvedMipMaps[i + 1].SizeY / 8, TextureResolvedMipMaps[i + 1].SizeZ / 8);
            }
            

            Camera.Current = lastCamera;
        }

        public void BindTexture(TextureUnit index)
        {
            //TextureNormalResolved.Use(TextureUnit.Texture24);
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
            ShaderProgram.Current.SetUniform("Voxelize_GridSizeX", GridSizeX);
            ShaderProgram.Current.SetUniform("Voxelize_GridSizeY", GridSizeY);
            ShaderProgram.Current.SetUniform("Voxelize_GridSizeZ", GridSizeZ);
            ShaderProgram.Current.SetUniform("Voxelize_BoxSize", BoxSize);
            ShaderProgram.Current.SetUniform("Voxelize_MipmapLevels", TextureResolvedMipMaps.Count);
            ShaderProgram.Current.SetUniform("Voxelize_MapPosition", MapPosition);
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
