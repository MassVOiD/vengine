using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class LodLevel
    {
        public float DistanceStart, DistanceEnd;
        public Object3dInfo Info3d;
        public GenericMaterial Material;

        ShaderStorageBuffer ModelMatricesBuffer, RotationMatricesBuffer, Ids;
        int InstancesFiltered = 0;

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

        public void Draw(Matrix4 parentTransformation, Mesh3d container, int instances)
        {
            Material.Use();
            container.SetUniforms();
            SetUniforms(parentTransformation);
            ModelMatricesBuffer.Use(0);
            RotationMatricesBuffer.Use(1);
            Ids.Use(2);
            Info3d.DrawInstanced(InstancesFiltered);
        }

        public void SetUniforms(Matrix4 parentTransformation)
        {
            ShaderProgram shader = ShaderProgram.Current;
            shader.SetUniform("LodDistanceStart", DistanceStart);
            shader.SetUniform("LodDistanceEnd", DistanceEnd);
            shader.SetUniform("InitialTransformation", parentTransformation);
            shader.SetUniform("InitialRotation", Matrix4.CreateFromQuaternion(parentTransformation.ExtractRotation()));
        }
        public void UpdateMatrix(List<Mesh3dInstance> instances, bool instantRebuffer = false)
        {
            var RotationMatrix = new List<Matrix4>();
            var Matrix = new List<Matrix4>();
            var MeshColoredID = new List<uint>();
            var cameraPos = Camera.Current.GetPosition();
            int instancesFiltered = 0;
           // instances.Sort((a, b) => (int)(((a.GetPosition() - cameraPos).Length - (b.GetPosition() - cameraPos).Length)*10));
            for(int i = 0; i < instances.Count; i++)
            {
                float dst = (instances[i].GetPosition() - cameraPos).Length;
                if(dst >= DistanceStart && dst < DistanceEnd)
                {
                    MeshColoredID.Add(instances[i].Id);
                    RotationMatrix.Add(Matrix4.CreateFromQuaternion(instances[i].GetOrientation()));
                    Matrix.Add(Matrix4.CreateScale(instances[i].GetScale()) * RotationMatrix[instancesFiltered] * Matrix4.CreateTranslation(instances[i].GetPosition()));
                    instancesFiltered++;
                }
            }
            if(!instantRebuffer)
            {
                GLThread.Invoke(() =>
                {
                    ModelMatricesBuffer.MapData(Matrix.ToArray());
                    RotationMatricesBuffer.MapData(RotationMatrix.ToArray());
                    Ids.MapData(MeshColoredID.ToArray());
                    GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                    InstancesFiltered = instancesFiltered;
                });
            }
            else
            {
                ModelMatricesBuffer.MapData(Matrix.ToArray());
                RotationMatricesBuffer.MapData(RotationMatrix.ToArray());
                Ids.MapData(MeshColoredID.ToArray());
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
                InstancesFiltered = instancesFiltered;
            }
        }
    }
}
