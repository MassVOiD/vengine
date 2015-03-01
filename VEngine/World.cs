using OpenTK;
using System.Collections.Generic;
using System;
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
        public volatile bool Disposed;

        public Line2dPool LinesPool;

        public DiscreteDynamicsWorld PhysicalWorld;
        CollisionDispatcher Dispatcher;
        DbvtBroadphase Broadphase;
        CollisionConfiguration CollisionConf;
        Dictionary<IRenderable, CollisionObject> CollisionObjects;

        public World()
        {
            Children = new List<IRenderable>();
            LinesPool = new Line2dPool();
            CollisionConf = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(CollisionConf);
            Broadphase = new DbvtBroadphase();
            PhysicalWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
            PhysicalWorld.Gravity = new Vector3(0, -10, 0);
            PhysicalWorld.SolverInfo.SolverMode = SolverModes.Simd;
            CollisionObjects = new Dictionary<IRenderable, CollisionObject>();
            if(Root == null)
                Root = this;
        }

        private void SortByShader()
        {
            Children.Sort((a, b) =>
            {
                if(!(a is Mesh3d))return 0;
                if(!(b is Mesh3d))return 0;
                var am = a as Mesh3d;
                var bm = b as Mesh3d;
                return am.GetHashCode() - bm.GetHashCode();
            });
        }

        public RigidBody CreateRigidBody(float mass, Matrix4 startTransform, CollisionShape shape, Mesh3d reference)
        {
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if(isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            body.UserObject = reference;

            PhysicalWorld.AddRigidBody(body);

            return body;
        }


        public virtual void UpdatePhysics(float time)
        {
            PhysicalWorld.StepSimulation(time*4.0f);
            int len = PhysicalWorld.CollisionObjectArray.Count;
            Mesh3d mesh;
            CollisionObject body;
            for(int i = 0; i < len; i++)
            {
                body = PhysicalWorld.CollisionObjectArray[i];
                mesh = body.UserObject as Mesh3d;
                if(mesh != null)
                {
                    mesh.Transformation.SetPosition((Matrix4.CreateScale(Scale) * body.WorldTransform).ExtractTranslation());
                    mesh.Transformation.SetOrientation(mesh.PhysicalBody.Orientation);
                }
            }
        }

        public void Add(IRenderable renderable)
        {
            Children.Add(renderable);
            if(renderable is Mesh3d)
            {
                Mesh3d mesh = renderable as Mesh3d;
                if(mesh.GetCollisionShape() != null || renderable is IPhysical)
                {
                    lock(PhysicalWorld)
                    {
                        CollisionObjects.Add(renderable, mesh.CreateRigidBody());
                        mesh.PhysicalBody.SetSleepingThresholds(0, 0);
                        mesh.PhysicalBody.ContactProcessingThreshold = 0;
                        mesh.PhysicalBody.CcdMotionThreshold = 0;
                        PhysicalWorld.AddRigidBody(mesh.PhysicalBody);
                    }
                }
            }
            else if(renderable is IPhysical)
            {
                IPhysical physicalObject = renderable as IPhysical;
                lock(PhysicalWorld)
                {
                    PhysicalWorld.AddRigidBody(physicalObject.GetRigidBody());
                    physicalObject.GetRigidBody().SetSleepingThresholds(0, 0);
                    physicalObject.GetRigidBody().ContactProcessingThreshold = 0;
                    physicalObject.GetRigidBody().CcdMotionThreshold = 0;
                }

            }
            SortByShader();
        }

        public bool ShouldUpdatePhysics = false;
        public void Draw()
        {
            for(int i = 0; i < Children.Count; i++)
            {
                if(Children[i] != null) Children[i].Draw();
            }
        }

        public void Remove(IRenderable renderable)
        {
            Children.Remove(renderable);
            if(renderable is Mesh3d)
            {
                Mesh3d mesh = renderable as Mesh3d;
                if(mesh.GetCollisionShape() != null)
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