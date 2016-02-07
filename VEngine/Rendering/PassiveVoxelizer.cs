using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class PassiveVoxelizer
    {
        private ShaderStorageBuffer BoxesSSBO;

        private Framebuffer DrawingFramebuffer;

        private ShaderProgram VoxelizerShader;

        public class Voxel
        {
            public float Density;
            public Vector3 Maximum, Minimum, Albedo, Normal;
        }

        public PassiveVoxelizer()
        {
            VoxelizerShader = ShaderProgram.Compile("Voxelizer.vertex.glsl", "Voxelizer.fragment.glsl");
            BoxesSSBO = new ShaderStorageBuffer();
            DrawingFramebuffer = new Framebuffer(256, 256, false)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.R8,
                ColorPixelType = PixelType.Byte,
                ColorPixelFormat = PixelFormat.Red
            };
        }

        public List<Voxel> Voxelize(Object3dManager objinfo, int gridSize)
        {
            List<Voxel> voxels = new List<Voxel>();
            List<byte> bytes = new List<byte>();
            var dv3 = new Vector3(gridSize, gridSize, gridSize);
            for(int x = 0; x < gridSize; x++)
            {
                for(int y = 0; y < gridSize; y++)
                {
                    for(int z = 0; z < gridSize; z++)
                    {
                        var voxel = new Voxel();
                        voxel.Minimum = (Vector3.Divide(new Vector3(x, y, z), (float)gridSize) - new Vector3(0.5f)) * 2.0f;
                        voxel.Maximum = (Vector3.Divide(new Vector3(x + 1, y + 1, z + 1), (float)gridSize) - new Vector3(0.5f)) * 2.0f;
                        voxels.Add(voxel);
                        bytes.AddRange(BitConverter.GetBytes((uint)0));
                        bytes.AddRange(BitConverter.GetBytes((int)0));
                        bytes.AddRange(BitConverter.GetBytes((int)0));
                        bytes.AddRange(BitConverter.GetBytes((int)0));
                    }
                }
            }
            BoxesSSBO.MapData(bytes.ToArray());
            // yay
            Vector3 transFromCenter = objinfo.GetAverageTranslationFromZero();
            objinfo.OriginToCenter();
            float divisor = objinfo.GetNormalizeDivisor();
            var v3aabb = objinfo.GetAxisAlignedBox();
            //objinfo.Normalize();
            objinfo.AxisDivide(v3aabb);
            DrawingFramebuffer.Use();
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.ColorMask(true, false, false, false);
            VoxelizerShader.Use();
            //VoxelizerShader.SetUniform("NormalizationDivisor", 1.0f / divisor);
            // VoxelizerShader.SetUniform("CenterToZero", transFromCenter);
            BoxesSSBO.Use(9);
            var ob3d = new Object3dInfo(objinfo.Vertices);
            VoxelizerShader.SetUniform("Grid", gridSize);
            VoxelizerShader.SetUniform("Pass", 0);
            ob3d.Draw();/*
            VoxelizerShader.SetUniform("Pass", 1);
            ob3d.Draw();
            VoxelizerShader.SetUniform("Pass", 2);
            ob3d.Draw();*/
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            var resultBytes = BoxesSSBO.Read(0, bytes.Count);
            int cursor = 0;
            int i = 0;
            var newVoxels = new List<Voxel>();

            for(int x = 0; x < gridSize; x++)
            {
                for(int y = 0; y < gridSize; y++)
                {
                    for(int z = 0; z < gridSize; z++)
                    {
                        uint hit = BitConverter.ToUInt32(resultBytes, cursor);
                        int nrx = BitConverter.ToInt32(resultBytes, cursor+4);
                        int nry = BitConverter.ToInt32(resultBytes, cursor+8);
                        int nrz = BitConverter.ToInt32(resultBytes, cursor+12);
                        cursor += 4*4;
                        if(hit > 0)
                        {
                            float fnrx = (float)nrx / (float)hit;
                            float fnry = (float)nry / (float)hit;
                            float fnrz = (float)nrz / (float)hit;
                            if(hit > 65536)
                                hit = 65536;
                            float dens = (float)hit / (float)65536;
                            voxels[i].Maximum *= v3aabb;
                            voxels[i].Minimum *= v3aabb;
                            voxels[i].Maximum += transFromCenter;
                            voxels[i].Minimum += transFromCenter;
                            voxels[i].Density = dens;
                            voxels[i].Normal = new Vector3(fnrx / 128.0f, fnry / 128.0f, fnrz / 128.0f);
                            newVoxels.Add(voxels[i]);
                        }
                        i++;
                    }
                }
            }
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.ColorMask(true, true, true, true);
            return newVoxels;
        }
    }
}