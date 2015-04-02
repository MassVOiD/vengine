using System;
using BulletSharp;
using OpenTK;

namespace VDGTech
{
    public class Mesh3d : IRenderable, ITransformable
    {
        public Mesh3d(Object3dInfo objectInfo, IMaterial material)
        {
            DisableDepthWrite = false;
            Instances = 1;
            ObjectInfo = objectInfo;
            Material = material;
            Transformation = new TransformationManager(Vector3.Zero, Quaternion.Identity, 1.0f);
            UpdateMatrix();
            MeshColoredID = new Vector3((float)Randomizer.NextDouble(), (float)Randomizer.NextDouble(), (float)Randomizer.NextDouble());
        }

        public int Instances;
        public IMaterial Material;
        public Matrix4 Matrix, RotationMatrix;
        public RigidBody PhysicalBody;
        public float SpecularSize = 1.0f, SpecularComponent = 1.0f, DiffuseComponent = 1.0f;
        public TransformationManager Transformation;
        public bool DisableDepthWrite;

        private static int LastMaterialHash = 0;
        private float Mass = 1.0f;
        public Object3dInfo ObjectInfo;
        private CollisionShape PhysicalShape;
        private static Random Randomizer = new Random();
        private Vector3 MeshColoredID;

        public bool CastShadows = true;
        public bool ReceiveShadows = true;
        public bool IgnoreLighting = false;


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

        public void Draw()
        {
            if(Transformation.BeenModified)
            {
                UpdateMatrix();
                Transformation.BeenModified = false;
            }
            if(Camera.Current == null)
                return;

            SetUniforms();
            Material.GetShaderProgram().SetUniformArray("ModelMatrixes", new Matrix4[] { Matrix });
            Material.GetShaderProgram().SetUniformArray("RotationMatrixes", new Matrix4[] { RotationMatrix });
           
            if(DisableDepthWrite)
                OpenTK.Graphics.OpenGL4.GL.DepthMask(false);
            ObjectInfo.Draw();
            if(DisableDepthWrite)
                OpenTK.Graphics.OpenGL4.GL.DepthMask(true);

            GLThread.CheckErrors();
        }

        public void SetUniforms()
        {
            ShaderProgram shader = ShaderProgram.Current;
            bool shaderSwitchResult = Material.Use();

            // if(Sun.Current != null) Sun.Current.BindToShader(shader); per mesh

            shader.SetUniform("SpecularComponent", SpecularComponent);
            shader.SetUniform("DiffuseComponent", DiffuseComponent);
            shader.SetUniform("SpecularSize", SpecularSize);
            shader.SetUniform("IgnoreLighting", IgnoreLighting);
            shader.SetUniform("RandomSeed", (float)Randomizer.NextDouble());
            shader.SetUniform("ColoredID", MeshColoredID); //magic
            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            /*if(LastMaterialHash == 0)
                LastMaterialHash = Material.GetShaderProgram().GetHashCode();
            if(LastMaterialHash != Material.GetShaderProgram().GetHashCode())
            {*/
                //LastMaterialHash = Material.GetShaderProgram().GetHashCode();
                // per world
                shader.SetUniform("Instances", 1);
                shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
                shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
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
            Matrix =  Matrix4.CreateScale(Transformation.GetScale()) * RotationMatrix * Matrix4.CreateTranslation(Transformation.GetPosition());
        }
    }
}