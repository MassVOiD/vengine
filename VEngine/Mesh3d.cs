using OpenTK;
using System;
using BulletSharp;

namespace VDGTech
{
    public class Mesh3d : IRenderable
    {
        private float Mass = 1.0f, Scale = 1.0f;
        public IMaterial Material;
        private Quaternion Orientation = Quaternion.Identity;
        private Vector3 Position = new Vector3(0, 0, 0);
        public Matrix4 Matrix;
        private Object3dInfo ObjectInfo;
        private CollisionShape PhysicalShape;
        public RigidBody PhysicalBody;
        public bool HasBeenModified { get; private set; }

        public Mesh3d(Object3dInfo objectInfo, IMaterial material)
        {
            ObjectInfo = objectInfo;
            Material = material;
            UpdateMatrix();
        }

        public RigidBody CreateRigidBody()
        {
            bool isDynamic = (Mass != 0.0f);
            var shape = GetCollisionShape();

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(Mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateTranslation(Position));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(Mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            body.UserObject = this;

            PhysicalBody = body;

            return body;
        }

        public Mesh3d Translate(Vector3 translation)
        {
            Position += translation;
            HasBeenModified = true;
            return this;
        }
        public Mesh3d SetPosition(Vector3 position)
        {
            Position = position;
            HasBeenModified = true;
            return this;
        }
        public Vector3 GetPosition()
        {
            return Position;
        }
        public Quaternion GetOrientation()
        {
            return Orientation;
        }
        public Mesh3d Rotate(Quaternion rotation)
        {
            Orientation = Quaternion.Multiply(Orientation, rotation);
            HasBeenModified = true;
            return this;
        }
        public Mesh3d SetOrientation(Quaternion orientation)
        {
            Orientation = orientation;
            HasBeenModified = true;
            return this;
        }
        public Mesh3d SetCollisionShape(CollisionShape shape)
        {
            PhysicalShape = shape;
            HasBeenModified = true;
            return this;
        }
        public CollisionShape GetCollisionShape()
        {
            return PhysicalShape;
        }
        public float GetMass()
        {
            return Mass;
        }
        public Mesh3d SetMass(float mass)
        {
            Mass = mass;
            HasBeenModified = true;
            return this;
        }
        public Mesh3d SetScale(float scale)
        {
            Scale = scale;
            UpdateMatrix();
            return this;
        }

        public void Draw()
        {
            if (HasBeenModified)
            {
                UpdateMatrix();
                HasBeenModified = false;
            }
            if (Camera.Current == null) return;
            ShaderProgram shader = Material.GetShaderProgram();
            if (shader.Handle == -1) return;
            Material.Use();
            if (Sun.Current != null) Sun.Current.BindToShader(shader);
            shader.SetUniform("ModelMatrix", Matrix);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniform("CameraPosition", Camera.Current.Position);
            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);

            ObjectInfo.Draw();
        }

        public void UpdateMatrix()
        {
            Matrix = Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);
        }
        public void UpdateMatrixFromPhysics(Matrix4 matrix)
        {
            Matrix = Matrix4.CreateScale(Scale) * matrix;
            Position = Matrix.ExtractTranslation();
            Orientation = PhysicalBody.Orientation;
        }
    }
}