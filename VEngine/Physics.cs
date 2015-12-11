using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using OpenTK;

namespace VEngine
{
    public class Physics
    {

        private DbvtBroadphase broadphase;

        private CollisionConfiguration collisionConf;

        private List<CollisionShape> collisionShapes = new List<CollisionShape>();

        private CollisionDispatcher dispatcher;

        public delegate void MeshCollideDelegate(Mesh3d meshA, Mesh3d meshB, Vector3 collisionPoint, Vector3 normalA);

        public event MeshCollideDelegate MeshCollide;


        private DiscreteDynamicsWorld World;

        private List<PhysicalBody> ActiveBodies;

        public Vector3 Gravity
        {
            get
            {
                return World.Gravity;
            }
            set
            {
                World.Gravity = value;
            }
        }

        public Physics()
        {
            ActiveBodies = new List<PhysicalBody>();
            collisionConf = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConf);

            broadphase = new DbvtBroadphase();
            World = new DiscreteDynamicsWorld(dispatcher, broadphase, null, collisionConf);
            World.SolverInfo.SolverMode = SolverModes.InterleaveContactAndFrictionConstraints;
            World.SolverInfo.Restitution = 0;
            World.Gravity = new Vector3(0, -9.81f, 0);
        }

        public PhysicalBody CreateBody(float mass, Mesh3dInstance mesh, CollisionShape shape)
        {
            return CreateBody(mass, mesh.Transformation, shape);
        }

        public PhysicalBody CreateBody(float mass, TransformationManager startTransform, CollisionShape shape)
        {
            var rb = CreateRigidBody(mass, startTransform.GetWorldTransform(), shape);
            var pb = new PhysicalBody(rb, shape, startTransform);
            return pb;
        }

        public void AddBody(PhysicalBody body)
        {
            World.AddRigidBody(body.Body);
            ActiveBodies.Add(body);
        }

        public void RemoveBody(PhysicalBody body)
        {
            World.RemoveRigidBody(body.Body);
            ActiveBodies.Remove(body);
        }

        public void UpdateAllModifiedTransformations()
        {
            for(int i = 0; i < ActiveBodies.Count; i++)
            {
                var b = ActiveBodies[i];
                if(b.Transformation.HasBeenModified())
                {
                    b.ReadChanges();
                    b.Body.Activate();
                }
            }
        }

        public void SimulationStep(float elapsedTime)
        {
            World.StepSimulation(elapsedTime);
            for(int i = 0; i < ActiveBodies.Count; i++)
            {
                var b = ActiveBodies[i];
                if(b.Body.ActivationState != ActivationState.IslandSleeping)
                {
                    b.ApplyChanges();
                }
            }
        }

        public static CollisionShape CreateConvexCollisionShape(Object3dInfo info)
        {
            return info.GetConvexHull();
        }
        public static CollisionShape CreateBoundingBoxShape(Object3dInfo info)
        {
            info.UpdateBoundingBox();
            return new BoxShape(info.AABB.Maximum - info.AABB.Minimum);
        }

        private RigidBody CreateRigidBody(float mass, Matrix4 startTransform, CollisionShape shape)
        {
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if(isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);


            body.SetSleepingThresholds(0, 0);
            body.ContactProcessingThreshold = 0;
            body.CcdMotionThreshold = 0;

            //World.AddRigidBody(body);

            return body;
        }

        public void ExitPhysics()
        {
            //remove/dispose constraints
            int i;
            for(i = World.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = World.GetConstraint(i);
                World.RemoveConstraint(constraint);
                constraint.Dispose();
            }

            //remove the rigidbodies from the dynamics world and delete them
            for(i = World.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = World.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if(body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                World.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            //delete collision shapes
            foreach(CollisionShape shape in collisionShapes)
                shape.Dispose();
            collisionShapes.Clear();

            World.Dispose();
            broadphase.Dispose();
            if(dispatcher != null)
            {
                dispatcher.Dispose();
            }
            collisionConf.Dispose();
        }

    }
}