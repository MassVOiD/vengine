using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
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
        public List<TransformationManager> Transformations;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        private Object3dInfo ObjectInfo;
        private Random Randomizer;
        private const int MaxInstances = 1500;
        private List<Matrix4[]> ModelMatrices, RotationMatrices;
        class LodLevelData
        {
            public Object3dInfo Info3d;
            public IMaterial Material;
            public float Distance;
        }

        private List<LodLevelData> LodLevels;


        public void AddLodLevel(float distance, Object3dInfo info, IMaterial material)
        {
            if(LodLevels == null)
                LodLevels = new List<LodLevelData>();
            LodLevels.Add(new LodLevelData()
            {
                Info3d = info,
                Material = material,
                Distance = distance
            });
            LodLevels.Sort((a, b) => (int)((a.Distance - b.Distance) * 100.0)); // *100 to preserve precision
        }

        Dictionary<LodLevelData, List<Matrix4>> MMatrices;
        Dictionary<LodLevelData, List<Matrix4>> RMatrices;

        public void RecalculateLod()
        {
            if(Camera.MainDisplayCamera == null)
                return;
            lock(Randomizer)
            {
                MMatrices = new Dictionary<LodLevelData, List<Matrix4>>();
                RMatrices = new Dictionary<LodLevelData, List<Matrix4>>();
                Transformations.Sort((t1, t2) => (int)((t1.GetPosition() - Camera.MainDisplayCamera.GetPosition()).Length - (t2.GetPosition() - Camera.MainDisplayCamera.GetPosition()).Length));
                foreach(var t in Transformations)
                {
                    foreach(var l in LodLevels)
                    {
                        if(!MMatrices.ContainsKey(l))
                            MMatrices.Add(l, new List<Matrix4>());
                        if(!RMatrices.ContainsKey(l))
                            RMatrices.Add(l, new List<Matrix4>());
                        var td = (t.GetPosition() - Camera.MainDisplayCamera.GetPosition()).Length;
                        if(td < l.Distance)
                        {
                            var rmat = Matrix4.CreateFromQuaternion(t.GetOrientation());
                            var mmat = Matrix4.CreateScale(t.GetScale()) * rmat * Matrix4.CreateTranslation(t.GetPosition());
                            MMatrices[l].Add(mmat);
                            RMatrices[l].Add(rmat);
                            break;
                        }
                    }
                }
            }
        }

        public void Draw()
        {
            if(Instances < 1)
                return;

            if(Camera.Current == null)
                return;

            if(LodLevels == null)
            {

                SetUniforms(Material);
                for(int i = 0; i < ModelMatrices.Count; i++)
                {
                    Material.GetShaderProgram().SetUniformArray("ModelMatrixes", ModelMatrices[i]);
                    Material.GetShaderProgram().SetUniformArray("RotationMatrixes", RotationMatrices[i]);
                    ObjectInfo.DrawInstanced(ModelMatrices[i].Length);
                }
            }
            else
            {
                lock(Randomizer)
                {
                    if(MMatrices == null)
                        return;
                    foreach(var l in LodLevels)
                    {
                        if(!MMatrices.ContainsKey(l) || !RMatrices.ContainsKey(l))
                            continue;
                        SetUniforms(l.Material);
                        Material.GetShaderProgram().SetUniformArray("ModelMatrixes", MMatrices[l].ToArray());
                        Material.GetShaderProgram().SetUniformArray("RotationMatrixes", RMatrices[l].ToArray());
                        l.Info3d.DrawInstanced(MMatrices[l].Count);

                    }
                }
            }
            // GLThread.CheckErrors();
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

            //LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            // per world
            shader.SetUniform("Instances", Instances);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniform("LogEnchacer", 0.01f);

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
            List<Matrix4> Matrix, RotationMatrix;
            RotationMatrix = new List<Matrix4>();
            Matrix = new List<Matrix4>();
            if(Instances > Transformations.Count)
                Instances = Transformations.Count;
            for(int i = 0; i < Instances; i++)
            {
                RotationMatrix.Add(Matrix4.CreateFromQuaternion(Transformations[i].GetOrientation()));
                Matrix.Add(Matrix4.CreateScale(Transformations[i].GetScale()) * RotationMatrix[i] * Matrix4.CreateTranslation(Transformations[i].GetPosition()));
            }
            ModelMatrices = new List<Matrix4[]>();
            RotationMatrices = new List<Matrix4[]>();
            for(int i = 0; i < Instances; i += MaxInstances)
            {
                ModelMatrices.Add(Matrix.Skip(i).Take(MaxInstances).ToArray());
                RotationMatrices.Add(RotationMatrix.Skip(i).Take(MaxInstances).ToArray());
            }
        }

        // this is gonna be awesome
        public static InstancedMesh3d FromSimilarMesh3dList(List<Mesh3d> meshes)
        {
            var first = meshes[0];
            InstancedMesh3d result = new InstancedMesh3d(first.MainObjectInfo, first.MainMaterial);
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
            meshes.Sort((a, b) => a.MainObjectInfo.GetHash() - b.MainObjectInfo.GetHash());
            var first = meshes[0];
            InstancedMesh3d current = new InstancedMesh3d(first.MainObjectInfo, first.MainMaterial);
            int lastHash = first.MainObjectInfo.GetHash();
            foreach(var m in meshes)
            {
                if(lastHash == m.MainObjectInfo.GetHash())
                {
                    current.Transformations.Add(m.Transformation);
                    current.Instances++;
                }
                else
                {
                    current.UpdateMatrix();
                    result.Add(current);
                    current = new InstancedMesh3d(m.MainObjectInfo, m.MainMaterial);
                    current.Transformations.Add(m.Transformation);
                    current.Instances++;
                }
            }
            result.Add(current);
            return result;
        }
    }
}