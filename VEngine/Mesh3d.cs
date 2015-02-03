using System;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Character;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using OpenTK;

namespace VDGTech
{
    public class Mesh3d : IRenderable
    {
        public Mesh3d(Object3dInfo objectInfo, IMaterial material)
        {
            Randomizer = new Random();
            Instances = 1;
            ObjectInfo = objectInfo;
            Material = material;
            UpdateMatrix();
        }

        public int Instances;
        public IMaterial Material;
        public Matrix4 Matrix;
        private float Mass = 1.0f, Scale = 1.0f;
        private Object3dInfo ObjectInfo;
        private Quaternion Orientation = Quaternion.Identity;
        private Entity PhysicalShape;
        private StaticMesh StaticMesh;
        private Vector3 Position = new Vector3(0, 0, 0);
        private Random Randomizer;

        public bool HasBeenModified
        {
            get;
            private set;
        }

        public void Draw()
        {
            if(HasBeenModified)
            {
                UpdateMatrix();
                HasBeenModified = false;
            }
            if(Camera.Current == null)
                return;
            ShaderProgram shader = Material.GetShaderProgram();
            Material.Use();

            if(Sun.Current != null)
                Sun.Current.BindToShader(shader);
            shader.SetUniform("ModelMatrix", Matrix);
            shader.SetUniform("ViewMatrix", Camera.Current.ViewMatrix);
            shader.SetUniform("ProjectionMatrix", Camera.Current.ProjectionMatrix);
            shader.SetUniformArray("LightsPs", LightPool.GetPMatrices());
            shader.SetUniformArray("LightsVs", LightPool.GetVMatrices());
            shader.SetUniformArray("LightsPos", LightPool.GetPositions());
            shader.SetUniformArray("LightsFarPlane", LightPool.GetFarPlanes());
            shader.SetUniform("LightsCount", LightPool.GetPositions().Length);

            shader.SetUniform("CameraPosition", Camera.Current.Position);
            shader.SetUniform("Time", (float)(DateTime.Now - GLThread.StartTime).TotalMilliseconds / 1000);
            shader.SetUniform("RandomSeed", (float)Randomizer.NextDouble());

            if(Instances > 1)
            {
                ObjectInfo.DrawInstanced(Instances);
            }
            else
            {
                ObjectInfo.Draw();
            }
            GLThread.CheckErrors();
        }

        public Entity GetCollisionShape()
        {
            return PhysicalShape;
        }
        public StaticMesh GetStaticCollisionMesh()
        {
            return StaticMesh;
        }

        public float GetMass()
        {
            return Mass;
        }

        public Quaternion GetOrientation()
        {
            return Orientation;
        }

        public Vector3 GetPosition()
        {
            return Position;
        }

        public Mesh3d Rotate(Quaternion rotation)
        {
            Orientation = Quaternion.Multiply(Orientation, rotation);
            HasBeenModified = true;
            return this;
        }

        public Mesh3d SetCollisionShape(Entity shape)
        {
            PhysicalShape = shape;
            PhysicalShape.Tag = this;
            PhysicalShape.CollisionInformation.Tag = this;
            HasBeenModified = true;
            return this;
        }
        public Mesh3d SetStaticCollisionMesh(StaticMesh shape)
        {
            StaticMesh = shape;
            StaticMesh.Tag = this;
            HasBeenModified = true;
            return this;
        }

        public Mesh3d SetMass(float mass)
        {
            Mass = mass;
            HasBeenModified = true;
            return this;
        }

        public Mesh3d SetOrientation(Quaternion orientation)
        {
            Orientation = orientation;
            HasBeenModified = true;
            return this;
        }

        public Mesh3d SetPosition(Vector3 position)
        {
            Position = position;
            HasBeenModified = true;
            return this;
        }

        public Mesh3d SetScale(float scale)
        {
            Scale = scale;
            UpdateMatrix();
            return this;
        }

        public Mesh3d Translate(Vector3 translation)
        {
            Position += translation;
            HasBeenModified = true;
            return this;
        }

        public void UpdateMatrixFromPhysics(Matrix4 matrix)
        {
            Matrix = Matrix4.CreateScale(Scale) * matrix;
            Position = Matrix.ExtractTranslation();
            Orientation = PhysicalShape.Orientation;
        }

        private void UpdateMatrix()
        {
            Matrix = Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);
        }
    }
}