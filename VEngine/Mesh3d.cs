using System;
using OpenTK;
using BulletSharp;

namespace VDGTech
{
    public class Mesh3d : IRenderable, ITransformable
    {
        public Mesh3d(Object3dInfo objectInfo, IMaterial material)
        {
            Randomizer = new Random();
            Instances = 1;
            ObjectInfo = objectInfo;
            Material = material;
            Transformation = new TransformationManager(Vector3.Zero, Quaternion.Identity, 1.0f);
            UpdateMatrix();
        }

        public int Instances;
        public IMaterial Material;
        public Matrix4 Matrix, RotationMatrix;
        private float Mass = 1.0f;
        private Object3dInfo ObjectInfo;
        private CollisionShape PhysicalShape;
        public RigidBody PhysicalBody;
        private Random Randomizer;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        public TransformationManager Transformation;

        private static int LastMaterialHash = 0;


        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public void Draw()
        {
            if(Transformation.BeenModified)
            {
                UpdateMatrix();
                Transformation.BeenModified = false;
            }
            if(Camera.Current == null)
                return;
            ShaderProgram shader = Material.GetShaderProgram();
            bool shaderSwitchResult = Material.Use();

         //   if(Sun.Current != null)
        //        Sun.Current.BindToShader(shader);
            // per mesh

            shader.SetUniform("ModelMatrix", Matrix);
            shader.SetUniform("SpecularComponent", SpecularComponent);
            shader.SetUniform("DiffuseComponent", DiffuseComponent);
            shader.SetUniform("RotationMatrix", RotationMatrix);
            shader.SetUniform("SpecularSize", SpecularSize);
            shader.SetUniform("RandomSeed", (float)Randomizer.NextDouble());
            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            if(LastMaterialHash == 0)
                LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            if(LastMaterialHash != Material.GetShaderProgram().GetHashCode())
            {
                LastMaterialHash = Material.GetShaderProgram().GetHashCode();
                // per world
                shader.SetUniform("Instances", 1);
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

            ObjectInfo.Draw();

            GLThread.CheckErrors();
        }

        public RigidBody CreateRigidBody()
        {
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

        public CollisionShape GetCollisionShape()
        {
            return PhysicalShape;
        }

        public float GetMass()
        {
            return Mass;
        }

        public Mesh3d SetCollisionShape(CollisionShape shape)
        {
            PhysicalShape = shape;
            PhysicalShape.UserObject = this;
            Transformation.BeenModified = true;
            return this;
        }

        public Mesh3d SetMass(float mass)
        {
            Mass = mass;
            Transformation.BeenModified = true;
            return this;
        }

        private void UpdateMatrix()
        {
            RotationMatrix = Matrix4.CreateFromQuaternion(Transformation.GetOrientation());
            Matrix = RotationMatrix * Matrix4.CreateScale(Transformation.GetScale()) * Matrix4.CreateTranslation(Transformation.GetPosition());
        }
    }
}