using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class PassiveVoxelizer
    {
        ShaderStorageBuffer BoxesSSBO;
        ShaderProgram VoxelizerShader;
        Framebuffer DrawingFramebuffer;

        public PassiveVoxelizer()
        {
            VoxelizerShader = ShaderProgram.Compile("Voxelizer.vertex.glsl", "Voxelizer.fragment.glsl");
            BoxesSSBO = new ShaderStorageBuffer();
            DrawingFramebuffer = new Framebuffer(128, 128, false)
            {
                ColorOnly = true,
                ColorInternalFormat = PixelInternalFormat.R8,
                ColorPixelType = PixelType.Byte,
                ColorPixelFormat = PixelFormat.Red
            };
        }

        public class Voxel
        {
            public Vector3 Maximum, Minimum;
        }

        public List<Voxel> Voxelize(Object3dInfo objinfo, int gridSize)
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
                        bytes.AddRange(BitConverter.GetBytes((int)0));
                    }
                }
            }
            BoxesSSBO.MapData(bytes.ToArray());
            // yay
            Vector3 transFromCenter = objinfo.GetAverageTranslationFromZero();
            float divisor = objinfo.GetDivisorFromPoint(transFromCenter);
            DrawingFramebuffer.Use();
            GL.Disable(EnableCap.DepthTest);
            GL.ColorMask(true, false, false, false);
            VoxelizerShader.Use();
            VoxelizerShader.SetUniform("NormalizationDivisor", 1.0f / divisor);
            VoxelizerShader.SetUniform("CenterToZero", transFromCenter);
            VoxelizerShader.SetUniform("Grid", gridSize);
            BoxesSSBO.Use(4);
            objinfo.Draw();
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
                        int hit = BitConverter.ToInt32(resultBytes, cursor);
                        cursor += 4;
                        if(hit > 0)
                        {
                            voxels[i].Maximum *= divisor;
                            voxels[i].Minimum *= divisor;
                            newVoxels.Add(voxels[i]);
                        }
                        i++;
                    }
                }
            }
            GL.Enable(EnableCap.DepthTest);
            GL.ColorMask(true, true, true, true);
            return newVoxels;
        }
    }
}
