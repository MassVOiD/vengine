using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class LodLevel
    {
        public float DistanceStart, DistanceEnd;
        public Object3dInfo Info3d;
        public GenericMaterial Material;

        private const int matbsize = 16 * 4;

        private const int uintbsize = 4;

        private int instancesFiltered = 0;

        private int InstancesFiltered = 0;

        private byte[] Matrix = new byte[0];

        private byte[] MeshColoredID = new byte[0];

        private ShaderStorageBuffer ModelMatricesBuffer, RotationMatricesBuffer, Ids;

        private byte[] RotationMatrix = new byte[0];

        public LodLevel(Object3dInfo o3i, GenericMaterial gm, float distStart, float distEnd)
        {
            DistanceStart = distStart;
            DistanceEnd = distEnd;
            Info3d = o3i;
            Material = gm;
            ModelMatricesBuffer = new ShaderStorageBuffer();
            RotationMatricesBuffer = new ShaderStorageBuffer();
            Ids = new ShaderStorageBuffer();
        }

        public void Draw(Mesh3d container, int instances)
        {
            Material.Use();
            container.SetUniforms();
            SetUniforms();
            ModelMatricesBuffer.Use(0);
            RotationMatricesBuffer.Use(1);
            Ids.Use(2);
            Info3d.DrawInstanced(InstancesFiltered);
        }

        public void SetUniforms()
        {
            ShaderProgram shader = ShaderProgram.Current;
            shader.SetUniform("LodDistanceStart", DistanceStart);
            shader.SetUniform("LodDistanceEnd", DistanceEnd);
        }

        public void UpdateMatrix(List<Mesh3dInstance> instances, bool instantRebuffer = false)
        {
            var cameraPos = Camera.Current.GetPosition();
            instancesFiltered = 0;
            // instances.Sort((a, b) => (int)(((a.GetPosition() - cameraPos).Length - (b.GetPosition() - cameraPos).Length)*10));
            for(int i = 0; i < instances.Count; i++)
            {
                float dst = (instances[i].GetPosition() - cameraPos).Length;
                if(dst >= DistanceStart && dst < DistanceEnd)
                {
                    if(Matrix.Length < instances.Count * matbsize)
                    {
                        Matrix = new byte[instances.Count * matbsize];
                        RotationMatrix = new byte[instances.Count * matbsize];
                        MeshColoredID = new byte[instances.Count * uintbsize];
                    }
                    // MeshColoredID[instancesFiltered] = (instances[i].Id);
                    var rot = Matrix4.CreateFromQuaternion(instances[i].GetOrientation());
                    memset(ref RotationMatrix, rot, instancesFiltered * matbsize);
                    memset(ref Matrix, (Matrix4.CreateScale(instances[i].GetScale()) * rot * Matrix4.CreateTranslation(instances[i].GetPosition())), instancesFiltered * matbsize);
                    memset(ref MeshColoredID, instances[i].Id, instancesFiltered * uintbsize);
                    // Matrix[instancesFiltered] = (Matrix4.CreateScale(instances[i].GetScale()) * RotationMatrix[instancesFiltered] * Matrix4.CreateTranslation(instances[i].GetPosition()));
                    instancesFiltered++;
                }
            }
            if(!instantRebuffer)
            {
                Game.Invoke(() =>
                {
                    GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                    ModelMatricesBuffer.MapData(Matrix);
                    RotationMatricesBuffer.MapData(RotationMatrix);
                    Ids.MapData(MeshColoredID);
                    GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                    GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                    InstancesFiltered = instancesFiltered;
                });
            }
            else
            {
                GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                ModelMatricesBuffer.MapData(Matrix);
                RotationMatricesBuffer.MapData(RotationMatrix);
                Ids.MapData(MeshColoredID);
                GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                InstancesFiltered = instancesFiltered;
            }
        }

        private static void bset(byte[] src, ref byte[] dest, int start, int count)
        {
            Array.Copy(src, 0, dest, start, count);
        }

        private static void memset(ref byte[] array, Matrix4 matrix, int starti)
        {
            int ind = starti;
            bset(BitConverter.GetBytes(matrix.Row0.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row0.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row0.Z), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row0.W), ref array, ind, 4);
            ind += 4;

            bset(BitConverter.GetBytes(matrix.Row1.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row1.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row1.Z), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row1.W), ref array, ind, 4);
            ind += 4;

            bset(BitConverter.GetBytes(matrix.Row2.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row2.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row2.Z), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row2.W), ref array, ind, 4);
            ind += 4;

            bset(BitConverter.GetBytes(matrix.Row3.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row3.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row3.Z), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(matrix.Row3.W), ref array, ind, 4);
            ind += 4;
        }

        private static void memset(ref byte[] array, uint value, int starti)
        {
            bset(BitConverter.GetBytes(value), ref array, starti, 4);
        }
    }
}