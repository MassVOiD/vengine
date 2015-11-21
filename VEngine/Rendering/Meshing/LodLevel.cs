using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    public class LodLevel
    {
        public float DistanceStart, DistanceEnd;
        public Object3dInfo Info3d;
        public GenericMaterial Material;

        public LodLevel(Object3dInfo o3i, GenericMaterial gm, float distStart, float distEnd)
        {
            DistanceStart = distStart;
            DistanceEnd = distEnd;
            Info3d = o3i;
            Material = gm;
        }

        public void Draw(Matrix4 parentTransformation, Mesh3d container, int instances)
        {
            Material.Use();
            container.SetUniforms();
            SetUniforms(parentTransformation);
            Info3d.DrawInstanced(instances);
        }

        public void SetUniforms(Matrix4 parentTransformation)
        {
            ShaderProgram shader = ShaderProgram.Current;
            shader.SetUniform("LodDistanceStart", DistanceStart);
            shader.SetUniform("LodDistanceEnd", DistanceEnd);
            shader.SetUniform("InitialTransformation", parentTransformation);
            shader.SetUniform("InitialRotation", Matrix4.CreateFromQuaternion(parentTransformation.ExtractRotation()));
        }
    }
}
