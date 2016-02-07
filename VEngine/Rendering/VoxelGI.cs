using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class VoxelGI
    {
        /*
        What this class got to do:
        - Take voxel[] with aabbs as input and create buffers from it
        - compile GI shader - thats gonna be shiny diamond 
        - expose a method to update GI for every voxel - runs GI shader
        - expose a method to map voxels to shader to allow displaying results
        */

        public class VoxelContainer
        {
            public Vector3 Min, Max;
            public List<PassiveVoxelizer.Voxel> Voxels;
            public VoxelContainer(Vector3 min, Vector3 max, List<PassiveVoxelizer.Voxel> voxels)
            {
                Voxels = voxels;
                Min = min;
                Max = max;
            }
        }

        List<VoxelContainer> VoxelsContainers;
        ComputeShader GIShader;
        ShaderStorageBuffer ContainersBuffer;
        ShaderStorageBuffer VoxelsBuffer;
        ShaderStorageBuffer VoxelsResultsBuffer;

        public VoxelGI()
        {
            GIShader = new ComputeShader("VoxelGI.compute.glsl");
            ContainersBuffer = new ShaderStorageBuffer();
            ContainersBuffer.Type = BufferUsageHint.StaticDraw;
            VoxelsBuffer = new ShaderStorageBuffer();
            VoxelsBuffer.Type = BufferUsageHint.DynamicDraw;
            VoxelsResultsBuffer = new ShaderStorageBuffer();
        }

        int VoxelsCursor = 0;
        int ContainerCount = 0;
        int VoxelCount = 0;
        List<byte> VoxelBytes = new List<byte>();
        List<byte> VoxelResultsBytes = new List<byte>();
        List<byte> ContainerBytes = new List<byte>();
        void SerializeVoxel(PassiveVoxelizer.Voxel voxel)
        {
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Minimum.X));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Minimum.Y));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Minimum.Z));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Minimum.Z)); //align

            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Maximum.X));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Maximum.Y));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Maximum.Z));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Maximum.Z)); //align

            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Albedo.X));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Albedo.Y));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Albedo.Z));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Albedo.Z)); //align

            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Normal.X));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Normal.Y));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Normal.Z));
            VoxelBytes.AddRange(BitConverter.GetBytes(voxel.Normal.Z)); //align

            VoxelResultsBytes.AddRange(BitConverter.GetBytes(0.0f));
            VoxelResultsBytes.AddRange(BitConverter.GetBytes(0.0f));
            VoxelResultsBytes.AddRange(BitConverter.GetBytes(0.0f));
            VoxelResultsBytes.AddRange(BitConverter.GetBytes(0.0f)); //align
            
        }

        void SerializeContainer(VoxelContainer voxel)
        {
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Min.X));
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Min.Y));
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Min.Z));
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Min.Z)); //align
            
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Max.X));
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Max.Y));
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Max.Z));
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Max.Z)); //align
            
            ContainerBytes.AddRange(BitConverter.GetBytes(VoxelsCursor)); //align
            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Voxels.Count)); //align
            ContainerBytes.AddRange(BitConverter.GetBytes(1)); //align
            ContainerBytes.AddRange(BitConverter.GetBytes(1)); //align

            ContainerBytes.AddRange(BitConverter.GetBytes(voxel.Voxels.Count));
            ContainerBytes.AddRange(BitConverter.GetBytes(0.0f));
            ContainerBytes.AddRange(BitConverter.GetBytes(0.0f));
            ContainerBytes.AddRange(BitConverter.GetBytes(0.0f)); //align
            foreach(var v in voxel.Voxels)
            {
                SerializeVoxel(v);
            }
            VoxelsCursor += voxel.Voxels.Count;
            VoxelCount += voxel.Voxels.Count;
            ContainerCount += 1;
        }

        int BaseVoxel = 0;
        int RunCount = 1280;
        public void UpdateGI()
        {
            GIShader.Use();
            ContainersBuffer.Use(3);
            VoxelsBuffer.Use(4);
            VoxelsResultsBuffer.Use(5);
            GIShader.SetUniform("ContainersCount", ContainerCount);
            GIShader.SetUniform("VoxelsCount", VoxelCount);
            GIShader.SetUniform("BaseVoxel", BaseVoxel);
            GIShader.SetUniform("Time", (float)(DateTime.Now - Game.StartTime).TotalMilliseconds / 1000);
            GIShader.SetUniform("CameraPosition", new Vector4(Camera.MainDisplayCamera.GetPosition(), 1));
            Game.World.Scene.SetLightingUniforms(GIShader);
            int wgrX = VoxelCount;
            var before = DateTime.Now;

            int rc = RunCount;
            if(rc + BaseVoxel >= VoxelCount)
                rc = VoxelCount - BaseVoxel;

            GIShader.Dispatch(10, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            var after = DateTime.Now;
            Console.WriteLine("GI UPDATE TIME:: {0} ms", (after - before).TotalMilliseconds);
            BaseVoxel += rc;
            if(BaseVoxel >= VoxelCount)
                BaseVoxel = 0;
        }

        public void UseVoxelsBuffer(uint baseunitcontainers, uint baseunitvoxels)
        {
            ContainersBuffer.Use(3);
            VoxelsBuffer.Use(4);
            VoxelsResultsBuffer.Use(5);
        }

        public void UpdateVoxels(List<VoxelContainer> containers)
        {
            VoxelsContainers = containers;
            VoxelsCursor = 0;
            ContainerCount = 0;
            VoxelCount = 0;
            ContainerBytes = new List<byte>();
            VoxelBytes = new List<byte>();
            foreach(var c in containers)
                SerializeContainer(c);
            ContainersBuffer.MapData(ContainerBytes.ToArray());
            VoxelsBuffer.MapData(VoxelBytes.ToArray());
            VoxelsResultsBuffer.MapData(VoxelResultsBytes.ToArray());
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }
    }
}
