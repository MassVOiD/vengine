using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using OpenTK;

namespace VEngine
{
    public class Mesh3d : IRenderable, ITransformable
    {
        public Mesh3d(Object3dInfo objectInfo, IMaterial material)
        {
           // ModelMatricesBuffer = new ShaderStorageBuffer();
           // RotationMatricesBuffer = new ShaderStorageBuffer();
            DisableDepthWrite = false;
            Instances = 1;
            MainObjectInfo = objectInfo;
            MainMaterial = material;
            Transformation = new TransformationManager(Vector3.Zero, Quaternion.Identity, 1.0f);
            UpdateMatrix();
            MeshColoredID = new Vector3((float)Randomizer.NextDouble(), (float)Randomizer.NextDouble(), (float)Randomizer.NextDouble());
        }

        public int Instances;
        public IMaterial MainMaterial;
        public Matrix4 Matrix, RotationMatrix;
        public RigidBody PhysicalBody;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        public TransformationManager Transformation;
        public bool DisableDepthWrite;
        //public ShaderStorageBuffer ModelMatricesBuffer, RotationMatricesBuffer;

        private static int LastMaterialHash = 0;
        private float Mass = 1.0f;
        public Object3dInfo MainObjectInfo;
        private CollisionShape PhysicalShape;
        private static Random Randomizer = new Random();
        private Vector3 MeshColoredID;

        public float ReflectionStrength = 0;
        public float RefractionStrength = 0;

        public bool CastShadows = true;
        public bool ReceiveShadows = true;
        public bool IgnoreLighting = false;
        private Texture AlphaMask = null;
        public bool DrawOddOnly = false;
        public static bool IsOddframe = false;
        public static bool PostProcessingUniformsOnly = false;

        class LodLevelData{
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
            LodLevels.Sort((a, b) => (int)((b.Distance - a.Distance)*100.0)); // *100 to preserve precision
        }

        private IMaterial GetCurrentMaterial()
        {
            if(LodLevels == null)
                return MainMaterial;
            float distance = (Camera.Current.GetPosition() - Transformation.GetPosition()).Length;
            if(distance < LodLevels.Last().Distance)
                return MainMaterial;
            float d1 = float.MaxValue;
            foreach(var l in LodLevels)
            {
                if(l.Distance < distance)
                    return l.Material;
            }
            return LodLevels.Last().Material;
        }
        private Object3dInfo GetCurrent3dInfo()
        {
            if(LodLevels == null)
                return MainObjectInfo;
            float distance = (Camera.Current.GetPosition() - Transformation.GetPosition()).Length;
            if(distance < LodLevels.Last().Distance)
                return MainObjectInfo;
            float d1 = float.MaxValue;
            foreach(var l in LodLevels)
            {
                if(l.Distance < distance)
                    return l.Info3d;
            }
            return LodLevels.Last().Info3d;
        }

        public void UseAlphaMaskFromMedia(string key)
        {
            AlphaMask = new Texture(Media.Get(key));
        }

        public RigidBody CreateRigidBody(bool forceRecreate = false)
        {
            if(PhysicalBody != null && !forceRecreate)
                return PhysicalBody;
            bool isDynamic = (Mass != 0.0f);
            var shape = GetCollisionShape();

            Vector3 localInertia = Vector3.Zero;
            if(isDynamic)
                shape.CalculateLocalInertia(Mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateFromQuaternion(Transformation.GetOrientation()) * Matrix4.CreateTranslation(Transformation.GetPosition()));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            body.UserObject = this;

            PhysicalBody = body;

            return body;
        }
        public RigidBody CreateRigidBody(Vector3 massCenter, bool forceRecreate = false)
        {
            if(PhysicalBody != null && !forceRecreate)
                return PhysicalBody;
            bool isDynamic = (Mass != 0.0f);
            var shape = GetCollisionShape();

            Vector3 localInertia = massCenter;
            if(isDynamic)
                shape.CalculateLocalInertia(Mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateFromQuaternion(Transformation.GetOrientation()) * Matrix4.CreateTranslation(Transformation.GetPosition()));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            body.UserObject = this;

            PhysicalBody = body;

            return body;
        }

        public void Draw()
        {
            Draw(false);
        }

        public void Draw(bool ignoreDisableDepthWriteFlag = false)
        {
            //if(IsOddframe && DrawOddOnly)
            //    return;
            if(Transformation.HasBeenModified())
            {
                UpdateMatrix();

                //ModelMatricesBuffer.MapData(Matrix);
                //RotationMatricesBuffer.MapData(RotationMatrix);
                Transformation.ClearModifiedFlag();
            }
            if(Camera.Current == null)
                return;

            SetUniforms();
            GetCurrentMaterial().GetShaderProgram().SetUniform("ModelMatrix",  Matrix);
            GetCurrentMaterial().GetShaderProgram().SetUniform("RotationMatrix",  RotationMatrix);

            if(!ignoreDisableDepthWriteFlag)
            {
                if(DisableDepthWrite)
                    OpenTK.Graphics.OpenGL4.GL.DepthMask(false);
                GetCurrent3dInfo().Draw();
                if(DisableDepthWrite)
                    OpenTK.Graphics.OpenGL4.GL.DepthMask(true);
            }
            else
            {
                GetCurrent3dInfo().Draw();
            }

            GLThread.CheckErrors();
        }

        public void SetUniforms()
        {
            bool shaderSwitchResult = GetCurrentMaterial().Use();
            ShaderProgram shader = ShaderProgram.Current;

            //ModelMatricesBuffer.Use(0);
            //RotationMatricesBuffer.Use(1);

            // if(Sun.Current != null) Sun.Current.BindToShader(shader); per mesh
            GLThread.GraphicsSettings.SetUniforms(shader);
            if(!PostProcessingUniformsOnly)
            {
                shader.SetUniform("SpecularComponent", SpecularComponent);
                shader.SetUniform("DiffuseComponent", DiffuseComponent);
                shader.SetUniform("SpecularSize", SpecularSize);
                shader.SetUniform("ReflectionStrength", ReflectionStrength);
                shader.SetUniform("RefractionStrength", RefractionStrength);
                shader.SetUniform("IgnoreLighting", IgnoreLighting);
                shader.SetUniform("ColoredID", MeshColoredID); //magic
                shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
                shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
                if(AlphaMask != null)
                {
                    shader.SetUniform("UseAlphaMask", 1);
                    AlphaMask.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture2);
                }
                else
                {
                    shader.SetUniform("UseAlphaMask", 0);
                }
            }
            else
            {
                shader.SetUniform("ViewMatrix", Camera.MainDisplayCamera.ViewMatrix);
                shader.SetUniform("ProjectionMatrix", Camera.MainDisplayCamera.ProjectionMatrix);
            }

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
            /*if(LastMaterialHash == 0)
                LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            if(LastMaterialHash != Material.GetShaderProgram().GetHashCode())
            {*/
                //LastMaterialHash = Material.GetShaderProgram().GetHashCode();
                // per world
                shader.SetUniform("Instances", 1);
                shader.SetUniform("LogEnchacer", 0.01f);

                shader.SetUniform("CameraPosition", Camera.Current.Transformation.GetPosition());
                shader.SetUniform("CameraDirection", Camera.Current.Transformation.GetOrientation().ToDirection());
                shader.SetUniform("CameraTangentUp", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up));
                shader.SetUniform("CameraTangentLeft", Camera.Current.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left));
                shader.SetUniform("FarPlane", Camera.Current.Far);
                shader.SetUniform("resolution", GLThread.Resolution);
           // }
        }

        public CollisionShape GetCollisionShape()
        {
            return PhysicalShape;
        }

        public float GetMass()
        {
            return Mass;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public Mesh3d SetCollisionShape(CollisionShape shape)
        {
            PhysicalShape = shape;
            PhysicalShape.UserObject = this;
            Transformation.MarkAsModified();
            return this;
        }

        public Mesh3d SetMass(float mass)
        {
            Mass = mass;
            Transformation.MarkAsModified();
            return this;
        }

        public void UpdateMatrix(bool noPhysics = false)
        {
            RotationMatrix = Matrix4.CreateFromQuaternion(Transformation.GetOrientation());
            Matrix =  Matrix4.CreateScale(Transformation.GetScale()) * RotationMatrix * Matrix4.CreateTranslation(Transformation.GetPosition());
            if(!noPhysics && PhysicalBody != null)
            {
                PhysicalBody.WorldTransform = RotationMatrix
                    * Matrix;
            }
        }
    }
}