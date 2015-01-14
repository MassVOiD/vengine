using OpenTK;
using System.Collections.Generic;
using BulletSharp;

namespace VDGTech
{
    public class World
    {
        public static World Root;
        public Quaternion Orientation = Quaternion.Identity;
        public Vector3 Position = new Vector3(0, 0, 0);
        public float Scale = 1.0f;
        private List<IRenderable> Children;
        private Matrix4 Matrix;
        public bool Disposed;

        public DiscreteDynamicsWorld PhysicalWorld;
        CollisionDispatcher Dispatcher;
        DbvtBroadphase Broadphase;
        CollisionConfiguration CollisionConf;
        CollisionShape GroundShape;
        CollisionObject Ground;
        Dictionary<IRenderable, CollisionObject> CollisionObjects;

        public World()
        {
            Children = new List<IRenderable>();
            CollisionConf = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(CollisionConf);
            Broadphase = new DbvtBroadphase();
            PhysicalWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
            PhysicalWorld.Gravity = new Vector3(0, -10, 0);
            GroundShape = new StaticPlaneShape(Vector3.UnitY, 1.0f);
            Ground = CreateRigidBody(0, Matrix4.CreateTranslation(0, 0, 0), GroundShape, null);
            CollisionObjects = new Dictionary<IRenderable, CollisionObject>();
        }

        public RigidBody CreateRigidBody(float mass, Matrix4 startTransform, CollisionShape shape, Mesh3d reference)
        {
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            body.UserObject = reference;

            PhysicalWorld.AddRigidBody(body);

            return body;
        }

        public virtual void UpdatePhysics(float elapsedTime)
        {
            if (Disposed) return;
            PhysicalWorld.StepSimulation(elapsedTime);
            int len = PhysicalWorld.CollisionObjectArray.Count;
            for (int i = 0; i < len; i++)
            {
                var body = PhysicalWorld.CollisionObjectArray[i];
                Mesh3d mesh = body.UserObject as Mesh3d;
                if (mesh != null)
                {
                    mesh.UpdateMatrixFromPhysics(body.WorldTransform);
                }
            }
        }

        public void Add(IRenderable renderable)
        {
            Children.Add(renderable);
            if (renderable is Mesh3d)
            {
                Mesh3d mesh = renderable as Mesh3d;
                if (mesh.GetCollisionShape() != null)
                {
                    CollisionObjects.Add(renderable, CreateRigidBody(mesh.GetMass(), mesh.Matrix, mesh.GetCollisionShape(), mesh));
                }
            }
        }

        public void DisposePhysics()
        {
            Disposed = true;
            int i;
            //remove/dispose constraints]
            try
            {
                for (i = PhysicalWorld.NumConstraints - 1; i >= 0; i--)
                {
                    TypedConstraint constraint = PhysicalWorld.GetConstraint(i);
                    PhysicalWorld.RemoveConstraint(constraint);
                    constraint.Dispose();
                }
            }
            catch { }

            try
            {
                //remove the rigidbodies from the dynamics world and delete them
                for (i = PhysicalWorld.NumCollisionObjects - 1; i >= 0; i--)
                {
                    CollisionObject obj = PhysicalWorld.CollisionObjectArray[i];
                    RigidBody body = obj as RigidBody;
                    if (body != null && body.MotionState != null)
                    {
                        body.MotionState.Dispose();
                    }
                    PhysicalWorld.RemoveCollisionObject(obj);
                    obj.Dispose();
                }
            }
            catch { }

            //delete collision shapes
            foreach (var shape in CollisionObjects.Values)
                shape.Dispose();
            CollisionObjects.Clear();

            PhysicalWorld.Dispose();
            Broadphase.Dispose();
            if (Dispatcher != null)
            {
                Dispatcher.Dispose();
            }
            CollisionConf.Dispose();
        }

        public void Draw()
        {
            if (Disposed) return;
            foreach (var c in Children)
            {
                c.Draw();
            }
        }

        public void Remove(IRenderable renderable)
        {
            Children.Remove(renderable);
            if (renderable is Mesh3d)
            {
                Mesh3d mesh = renderable as Mesh3d;
                if (mesh.GetCollisionShape() != null)
                {
                    PhysicalWorld.RemoveCollisionObject(CollisionObjects[renderable]);
                }
            }
        }

        public void UpdateMatrix()
        {
            Matrix = Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale);
        }
    }
}