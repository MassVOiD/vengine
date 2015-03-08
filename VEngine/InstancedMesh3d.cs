using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VDGTech
{
    public class InstancedMesh3d : IRenderable
    {
        public InstancedMesh3d(Object3dInfo objectInfo, IMaterial material)
        {
            Randomizer = new Random();
            Transformations = new List<TransformationManager>();
            Instances = 0;
            ObjectInfo = objectInfo;
            Material = material;
            UpdateMatrix();
        }

        public int Instances;
        public IMaterial Material;
        public List<Matrix4> Matrix, RotationMatrix;
        public List<TransformationManager> Transformations;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        private Object3dInfo ObjectInfo;
        private Random Randomizer;

        public bool HasBeenModified
        {
            get;
            private set;
        }

        public void Draw()
        {
            if(Instances < 1)
                return;
            if(HasBeenModified)
            {
                UpdateMatrix();
                HasBeenModified = false;
            }
            if(Camera.Current == null)
                return;
            SetUniforms(Material);

            for(int i = 0; i < Instances; i += 2000)
            {
                Material.GetShaderProgram().SetUniformArray("ModelMatrixes", Matrix.Skip(i).ToArray());
                Material.GetShaderProgram().SetUniformArray("RotationMatrixes", RotationMatrix.Skip(i).ToArray());
                ObjectInfo.DrawInstanced(Math.Min(Instances - i, 2000));
            }
            GLThread.CheckErrors();
        }
        static int LastMaterialHash = 0;
        public void SetUniforms(IMaterial material)
        {
            ShaderProgram shader = material.GetShaderProgram();
            bool shaderSwitchResult = Material.Use();

            // if(Sun.Current != null) Sun.Current.BindToShader(shader); per mesh

            shader.SetUniform("SpecularComponent", SpecularComponent);
            shader.SetUniform("DiffuseComponent", DiffuseComponent);
            shader.SetUniform("SpecularSize", SpecularSize);
            shader.SetUniform("RandomSeed", (float)Randomizer.NextDouble());
            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);

            LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            // per world
            shader.SetUniform("Instances", Instances);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniform("LogEnchacer", 0.01f);
            shader.SetUniformArray("LightsPs", LightPool.GetPMatrices());
            shader.SetUniformArray("LightsVs", LightPool.GetVMatrices());
            shader.SetUniformArray("LightsPos", LightPool.GetPositions());
            shader.SetUniformArray("LightsFarPlane", LightPool.GetFarPlanes());
            shader.SetUniformArray("LightsColors", LightPool.GetColors());
            shader.SetUniform("LightsCount", LightPool.GetPositions().Length);

            shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            shader.SetUniform("FarPlane", Camera.Current.Far);
            shader.SetUniform("resolution", GLThread.Resolution);
            
        }

        public class SingleInstancedMesh3dElement : ITransformable
        {
            public TransformationManager Transformation;
            public TransformationManager GetTransformationManager()
            {
                return Transformation;
            }
        }

        public SingleInstancedMesh3dElement Get(int index)
        {
            return new SingleInstancedMesh3dElement()
            {
                Transformation = Transformations[index]
            };
        }
        
        public void UpdateMatrix()
        {
            RotationMatrix = new List<Matrix4>();
            Matrix = new List<Matrix4>();
            if(Instances > Transformations.Count)
                Instances = Transformations.Count;
            for(int i = 0; i < Instances; i++)
            {
                RotationMatrix.Add(Matrix4.CreateFromQuaternion(Transformations[i].GetOrientation()));
                Matrix.Add(RotationMatrix[i] * Matrix4.CreateScale(Transformations[i].GetScale()) * Matrix4.CreateTranslation(Transformations[i].GetPosition()));
            }
        }

        // this is gonna be awesome
        public static InstancedMesh3d FromSimilarMesh3dList(List<Mesh3d> meshes)
        {
            var first = meshes[0];
            InstancedMesh3d result = new InstancedMesh3d(first.ObjectInfo, first.Material);
            foreach(var m in meshes)
            {
                result.Transformations.Add(m.Transformation);
                result.Instances++;
            }
            result.UpdateMatrix();
            return result;
        }
        // this is gonna be awesome
        public static List<InstancedMesh3d> FromMesh3dList(List<Mesh3d> meshes)
        {
            List<InstancedMesh3d> result = new List<InstancedMesh3d>();
            meshes.Sort((a, b) => a.ObjectInfo.GetHash() - b.ObjectInfo.GetHash());
            var first = meshes[0];
            InstancedMesh3d current = new InstancedMesh3d(first.ObjectInfo, first.Material);
            int lastHash = first.ObjectInfo.GetHash();
            foreach(var m in meshes)
            {
                if(lastHash == m.ObjectInfo.GetHash())
                {
                    current.Transformations.Add(m.Transformation);
                    current.Instances++;
                }
                else
                {
                    current.UpdateMatrix();
                    result.Add(current);
                    current = new InstancedMesh3d(m.ObjectInfo, m.Material);
                    current.Transformations.Add(m.Transformation);
                    current.Instances++;
                }
            }
            result.Add(current);
            return result;
        }
    }
}