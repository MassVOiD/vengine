using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
namespace VEngine
{
    public class InstancedMesh3d : IRenderable
    {
        public InstancedMesh3d(Object3dInfo objectInfo, GenericMaterial material)
        {
            ModelMatricesBuffer = new ShaderStorageBuffer();
            RotationMatricesBuffer = new ShaderStorageBuffer();
            Ids = new ShaderStorageBuffer();
            Randomizer = new Random();
            Transformations = new List<TransformationManager>();
            Instances = 0;
            ObjectInfo = objectInfo;
            Material = material;
            UpdateMatrix();
        }

        public int Instances;
        public GenericMaterial Material;
        public List<TransformationManager> Transformations;
        public Object3dInfo ObjectInfo;
        private Random Randomizer;
        //private const int MaxInstances = 1500;
        //private List<Matrix4[]> ModelMatrices, RotationMatrices;
        public ShaderStorageBuffer ModelMatricesBuffer, RotationMatricesBuffer, Ids;
        private List<Vector4> MeshColoredID;

        class LodLevelData
        {
            public Object3dInfo Info3d;
            public GenericMaterial Material;
            public float Distance;
        }

        private List<LodLevelData> LodLevels;


        public void AddLodLevel(float distance, Object3dInfo info, GenericMaterial material)
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
        public void Draw(Matrix4 parentTransformation)
        {
            Draw(parentTransformation, false);
        }

        public void Draw(Matrix4 parentTransformation, bool ignoreDisableDepthWriteFlag = false)
        {
            if(Instances < 1)
                return;

            if(Camera.Current == null)
                return;

            if(LodLevels == null)
            {

                SetUniforms(Material);
                ShaderProgram.Current.SetUniform("InitialTransformation", parentTransformation);
                ShaderProgram.Current.SetUniform("InitialRotation", Matrix4.CreateFromQuaternion(parentTransformation.ExtractRotation()));
                if(DisableDepthWrite && !ignoreDisableDepthWriteFlag)
                    OpenTK.Graphics.OpenGL4.GL.DepthMask(false);

                ObjectInfo.DrawInstanced(Instances);

                if(DisableDepthWrite && !ignoreDisableDepthWriteFlag)
                    OpenTK.Graphics.OpenGL4.GL.DepthMask(true);
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
                        if(DisableDepthWrite && !ignoreDisableDepthWriteFlag)
                            OpenTK.Graphics.OpenGL4.GL.DepthMask(false);
                        l.Info3d.DrawInstanced(MMatrices[l].Count);
                        if(DisableDepthWrite && !ignoreDisableDepthWriteFlag)
                            OpenTK.Graphics.OpenGL4.GL.DepthMask(true);

                    }
                }
            }
            // GLThread.CheckErrors();
        }
        //static int LastMaterialHash = 0;
        public bool IgnoreLighting = false;
        public bool DisableDepthWrite = false;
        public void SetUniforms(GenericMaterial material)
        {
            ShaderProgram shader = material.GetShaderProgram();
            bool shaderSwitchResult = Material.Use();

            shader = ShaderProgram.Current;

            ModelMatricesBuffer.Use(0);
            RotationMatricesBuffer.Use(1);
            Ids.Use(2);

            // if(Sun.Current != null) Sun.Current.BindToShader(shader); per mesh

            shader.SetUniform("IgnoreLighting", IgnoreLighting);

            shader.SetUniform("Selected",  0); //magic
            shader.SetUniform("RandomSeed1", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed2", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed3", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed4", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed5", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed6", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed7", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed8", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed9", (float)Randomizer.NextDouble());
            shader.SetUniform("RandomSeed10", (float)Randomizer.NextDouble());

            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            
            //LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            // per world
            shader.SetUniform("Instances", Transformations.Count);
            shader.SetUniform("Instanced", 1);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniform("LogEnchacer", 0.01f);

            shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
            shader.SetUniform("FarPlane", Camera.Current.Far);
            shader.SetUniform("resolution", new Vector2(GLThread.Resolution.Width, GLThread.Resolution.Height));

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

        List<Matrix4> Matrix, RotationMatrix;

        public Mesh3d Merge()
        {
            if(Matrix == null)
                UpdateMatrix();

            Object3dInfo main = ObjectInfo.CopyDeep();
            for(int i = 0; i < Matrix.Count; i++)
            {
                Object3dInfo obj = ObjectInfo.CopyDeep();
                obj.Transform(Matrix[i], RotationMatrix[i]);
                main.Append(obj);
            }
            return new Mesh3d(main, Material);
        }

        public void UpdateMatrix()
        {
            RotationMatrix = new List<Matrix4>();
            Matrix = new List<Matrix4>();
            MeshColoredID = new List<Vector4>();
            Instances = Transformations.Count;
            for(int i = 0; i < Instances; i++)
            {
                MeshColoredID.Add(new Vector4((float)Randomizer.NextDouble(), (float)Randomizer.NextDouble(), (float)Randomizer.NextDouble(), 0));
                RotationMatrix.Add(Matrix4.CreateFromQuaternion(Transformations[i].GetOrientation()));
                Matrix.Add(Matrix4.CreateScale(Transformations[i].GetScale()) * RotationMatrix[i] * Matrix4.CreateTranslation(Transformations[i].GetPosition()));
            }
            GLThread.Invoke(() =>
            {
                while(true)
                {
                    try
                    {
                        ModelMatricesBuffer.MapData(Matrix.ToArray());
                        RotationMatricesBuffer.MapData(RotationMatrix.ToArray());
                        Ids.MapData(MeshColoredID.ToArray());
                        break;
                    }
                    catch
                    {
                    }
                }
            });
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