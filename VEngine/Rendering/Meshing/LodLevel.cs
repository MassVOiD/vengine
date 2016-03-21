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
        public Object3dInfo BoundingBoxInfo3d;
        public GenericMaterial Material;

        private const int matbsize = 12 * 4;

        private const int uintbsize = 4;

        private int instancesFiltered = 0;

        private int InstancesFiltered = 0;

        private byte[] ModelInfosData = new byte[0];

        public bool ForceFrustumCull = false;

        public bool DisableFaceCulling = false;

        private ShaderStorageBuffer ModelInfosBuffer;

        public LodLevel(Object3dInfo o3i, GenericMaterial gm, float distStart, float distEnd)
        {
            DistanceStart = distStart;
            DistanceEnd = distEnd;
            Info3d = o3i;
            if(Info3d != null)
                BoundingBoxInfo3d = Generators.Object3dGenerator.CreateCube(o3i.BoundingBoxMin, o3i.BoundingBoxMax, Vector2.One).AsObject3dInfo();
            else
                BoundingBoxInfo3d = Object3dInfo.Empty;
            Material = gm;
            ModelInfosBuffer = new ShaderStorageBuffer();
        }

        public void SetInfo3d(Object3dInfo o3i)
        {
            Info3d = o3i;
            BoundingBoxInfo3d = Generators.Object3dGenerator.CreateCube(o3i.BoundingBoxMin, o3i.BoundingBoxMax, Vector2.One).AsObject3dInfo();
        }

        public void DrawBoundingBoxes(Mesh3d container, int instances)
        {
            Material.Use();
            container.SetUniforms();
            //ShaderProgram.Current.SetUniform("ForwardPass", InternalRenderingState.PassState == InternalRenderingState.State.ForwardTransparentBlendingAlphaPass ? 0 : 1);
            ModelInfosBuffer.Use(0);

            //BoundingBoxInfo3d.DrawInstanced(InstancesFiltered);
            if(DisableFaceCulling)
                GL.Disable(EnableCap.CullFace);
            Info3d.DrawInstanced(InstancesFiltered);
            if(DisableFaceCulling)
                GL.Enable(EnableCap.CullFace);
        }

        public void Draw(Mesh3d container, int instances)
        {
            /*if(instances < 3 || ForceFrustumCull) // special case optimization
            {
                var inst = container.GetInstances();
                bool visibility = true;
                for(int i = 0; i < instances; i++)
                {
                    var pos = inst[i].Transformation.Position;
                    visibility = visibility && Camera.Current.IsAABBVisible(Info3d.BoundingBoxMin + pos, Info3d.BoundingBoxMax + pos);
                }
                if(!visibility)
                    return;
            }*/

            if(InternalRenderingState.PassState == InternalRenderingState.State.ForwardOpaquePass)
            {
                if(Material.UseForwardRenderer)
                    return;
            }
            if(InternalRenderingState.PassState == InternalRenderingState.State.ForwardTransparentBlendingAdditivePass
                || InternalRenderingState.PassState == InternalRenderingState.State.ForwardTransparentBlendingAlphaPass)
            {
                if(!Material.UseForwardRenderer)
                    return;
            }
            if(Material.Blending == GenericMaterial.BlendingEnum.Alpha
                && InternalRenderingState.PassState == InternalRenderingState.State.ForwardTransparentBlendingAdditivePass)
                return;
            if(Material.Blending == GenericMaterial.BlendingEnum.AdditiveNormalized
                && InternalRenderingState.PassState == InternalRenderingState.State.ForwardTransparentBlendingAlphaPass)
                return;
            Material.Use();
            container.SetUniforms();
            ShaderProgram.Current.SetUniform("ForwardPass", InternalRenderingState.PassState == InternalRenderingState.State.ForwardTransparentBlendingAlphaPass ? 0 : 1);
            ModelInfosBuffer.Use(0);

            if(DisableFaceCulling)
                GL.Disable(EnableCap.CullFace);
            Info3d.DrawInstanced(InstancesFiltered);
            if(DisableFaceCulling)
                GL.Enable(EnableCap.CullFace);
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
                    if(ModelInfosData.Length < instances.Count * matbsize)
                    {
                        ModelInfosData = new byte[instances.Count * matbsize];
                    }
                    // MeshColoredID[instancesFiltered] = (instances[i].Id);
                    var rot = Matrix4.CreateFromQuaternion(instances[i].GetOrientation());
                    
                    memset(ref ModelInfosData, instances[i].GetOrientation(), instances[i].GetPosition(), instances[i].Id, instances[i].GetScale(), instancesFiltered * matbsize);

                    instancesFiltered++;
                }
            }
            if(!instantRebuffer)
            {
                Game.Invoke(() =>
                {
                    //GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                    ModelInfosBuffer.MapData(ModelInfosData);
                    //GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                    //GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                    InstancesFiltered = instancesFiltered;
                });
            }
            else
            {
                GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
                ModelInfosBuffer.MapData(ModelInfosData);
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

        private static void memset(ref byte[] array, Quaternion rotation, Vector3 translation, uint Id, Vector3 scale, int starti)
        {
            int ind = starti;

            bset(BitConverter.GetBytes(rotation.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(rotation.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(rotation.Z), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(rotation.W), ref array, ind, 4);
            ind += 4;

            bset(BitConverter.GetBytes(translation.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(translation.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(translation.Z), ref array, ind, 4);
            ind += 4;

            bset(BitConverter.GetBytes(Id), ref array, ind, 4);
            ind += 4;

            bset(BitConverter.GetBytes(scale.X), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(scale.Y), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(scale.Z), ref array, ind, 4);
            ind += 4;
            bset(BitConverter.GetBytes(0.0f), ref array, ind, 4);
            //ind += 4;
        }
    }
}