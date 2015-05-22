using System;
using System.Collections.Generic;
using BulletSharp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VEngine.UI;

namespace VEngine
{
    public class World
    {
        public World()
        {
            Children = new List<IRenderable>();
            LinesPool = new Line2dPool();
            CollisionConf = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(CollisionConf);
            Broadphase = new DbvtBroadphase();
            PhysicalWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf);
            PhysicalWorld.Gravity = new Vector3(0, -10, 0);
            PhysicalWorld.SolverInfo.SolverMode = SolverModes.InterleaveContactAndFrictionConstraints;
            PhysicalWorld.SolverInfo.Restitution = 0;
            CollisionObjects = new Dictionary<IRenderable, CollisionObject>();
            UI = new UIRenderer();
            if(Root == null)
                Root = this;
        }

        public static World Root;
        public volatile bool Disposed;
        public Line2dPool LinesPool;
        public UIRenderer UI;
        public Quaternion Orientation = Quaternion.Identity;
        public DiscreteDynamicsWorld PhysicalWorld;
        public Vector3 Position = new Vector3(0, 0, 0);
        public float Scale = 1.0f;
        public bool ShouldUpdatePhysics = false;
        private DbvtBroadphase Broadphase;
        private List<IRenderable> Children;
        private CollisionConfiguration CollisionConf;
        private Dictionary<IRenderable, CollisionObject> CollisionObjects;
        private CollisionDispatcher Dispatcher;
        private Matrix4 Matrix;
        public Mesh3d SkyDome;

        public void Add(IRenderable renderable)
        {
            if(Children.Contains(renderable))
                return;
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
        }

        public void SetSkyDome(Mesh3d dome)
        {

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

        public void Draw(bool ignoreMeshWithDisabledDepthTest = false, bool ignoreDisableDepthWriteFlag = false)
        {
            //if(Camera.Current != null) SortByCameraDistance();
            GL.CullFace(CullFaceMode.Back);
            for(int i = 0; i < Children.Count; i++)
            {
                if(Children[i] != null)
                {
                    if(Children[i] is Mesh3d && ((Mesh3d)Children[i]).DisableDepthWrite == true)
                        continue;
                    Children[i].Draw();

                }
            }
            if(!ignoreMeshWithDisabledDepthTest)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.BlendEquation(BlendEquationMode.FuncAdd);
               // GL.CullFace(CullFaceMode.Front);
                for(int i = 0; i < Children.Count; i++)
                {
                    if(Children[i] != null)
                    {
                        if((Children[i] is Mesh3d) && ((Mesh3d)Children[i]).DisableDepthWrite == true)
                            ((Mesh3d)Children[i]).Draw(ignoreDisableDepthWriteFlag);
                        if((Children[i] is InstancedMesh3d) && ((InstancedMesh3d)Children[i]).DisableDepthWrite == true)
                            ((InstancedMesh3d)Children[i]).Draw(ignoreDisableDepthWriteFlag);
                    }
                }
                GL.Disable(EnableCap.Blend);
               // GL.CullFace(CullFaceMode.Back);
            }
        }

        public void Remove(IRenderable renderable)
        {
            if(!Children.Contains(renderable))
                return;
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


        public void Explode(Vector3 position, float magnitude)
        {
            int len = PhysicalWorld.CollisionObjectArray.Count;
            for(int i = 0; i < len; i++)
            {
                var e = PhysicalWorld.CollisionObjectArray[i];
                var mesh = e.UserObject as Mesh3d;
                if(mesh != null)
                {
                    Vector3 offset = mesh.Transformation.GetPosition() - position;
                    float distanceSquared = offset.LengthSquared;
                    if(distanceSquared > float.Epsilon)
                    {
                        var distance = (float)Math.Sqrt(distanceSquared);
                        mesh.PhysicalBody.ApplyImpulse((offset * ((magnitude * 100.0f) / (distanceSquared))), Vector3.Zero);
                    }
                    else
                    {
                        mesh.PhysicalBody.ApplyImpulse(new Vector3(0, magnitude, 0), Vector3.Zero);
                    }
                }
            }

        }

        public void UpdateMatrix()
        {
            Matrix = Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale);
        }

        public float SimulationSpeed = 1.0f;

        public delegate void MeshCollideDelegate(Mesh3d meshA, Mesh3d meshB, Vector3 collisionPoint, Vector3 normalA);
        public event MeshCollideDelegate MeshCollide;

        public virtual void UpdatePhysics(float time)
        {
            PhysicalWorld.StepSimulation(time * SimulationSpeed);
            if(!World.Root.ShouldUpdatePhysics)
                return;
            World.Root.ShouldUpdatePhysics = false;
            int len = PhysicalWorld.CollisionObjectArray.Count;
            Mesh3d mesh;
            CollisionObject body;
            for(int i = 0; i < len; i++)
            {
                body = PhysicalWorld.CollisionObjectArray[i];
                mesh = body.UserObject as Mesh3d;
                if(mesh != null)
                {

                    mesh.Transformation.SetPosition(mesh.PhysicalBody.CenterOfMassPosition);
                    mesh.Transformation.SetOrientation(mesh.PhysicalBody.Orientation);
                    mesh.UpdateMatrix(true);
                    mesh.Transformation.ClearModifiedFlag();
                }
            }
            if(MeshCollide != null)
            {
                int numManifolds = Dispatcher.NumManifolds;
                for(int i = 0; i < numManifolds; i++)
                {
                    var contactManifold = Dispatcher.GetManifoldByIndexInternal(i);
                    var obA = contactManifold.Body0;
                    var obB = contactManifold.Body1;
                    var meshA = obA.UserObject as Mesh3d;
                    var meshB = obB.UserObject as Mesh3d;
                    if(meshA == null || meshB == null)
                        continue;

                    int numContacts = contactManifold.NumContacts;
                    for(int j = 0; j < numContacts; j++)
                    {
                        var pt = contactManifold.GetContactPoint(j);
                        if(pt.Distance < 0.0f)
                        {
                            MeshCollide.Invoke(meshA, meshB, pt.PositionWorldOnA, pt.NormalWorldOnB);
                        }
                    }
                }
            }
            MeshLinker.Resolve();
        }

        public void SortByObject3d()
        {
            Children.Sort((a, b) =>
            {
                if(!(a is Mesh3d))
                    return 0;
                if(!(b is Mesh3d))
                    return 0;
                var am = a as Mesh3d;
                var bm = b as Mesh3d;
                return am.MainObjectInfo.GetHash() - bm.MainObjectInfo.GetHash();
            });
        }
        public void SortByCameraDistance()
        {
            Children.Sort((a, b) =>
            {
                if(!(a is Mesh3d))
                    return 0;
                if(!(b is Mesh3d))
                    return 0;
                var am = a as Mesh3d;
                var bm = b as Mesh3d;
                return (int)((am.Transformation.GetPosition() - Camera.Current.Transformation.GetPosition()).Length - (bm.Transformation.GetPosition() - Camera.Current.Transformation.GetPosition()).Length * 100.0f);
            });
        }
        public void SortByDepthMasking()
        {
            Children.Sort((a, b) =>
            {
                if(!(a is Mesh3d))
                    return 0;
                if(!(b is Mesh3d))
                    return 0;
                var am = a as Mesh3d;
                var bm = b as Mesh3d;
                return am.DisableDepthWrite && !bm.DisableDepthWrite ? 0 : 1;
            });
        }
    }
}