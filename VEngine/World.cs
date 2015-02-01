using OpenTK;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Character;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionRuleManagement;
using BEPUutilities.Threading;
using System;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.Constraints;

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

        public Space PhysicalWorld;
        Dictionary<IRenderable, Entity> CollisionObjects;

        public World()
        {
            Children = new List<IRenderable>();
            // this will sslow down graphics thread and its not what we want here
            var parallelLooper = new ParallelLooper();
            if(Environment.ProcessorCount > 1)
            {
                for(int i = 0; i < Environment.ProcessorCount; i++)
                {
                    parallelLooper.AddThread();
                }
            }
            PhysicalWorld = new Space(parallelLooper);
            PhysicalWorld.Solver.AllowMultithreading = true;
            PhysicalWorld.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
            SolverSettings.DefaultMinimumIterationCount = 0;
            PhysicalWorld.TimeStepSettings.MaximumTimeStepsPerFrame = 4;
            PhysicalWorld.Solver.IterationLimit = 4;
            GeneralConvexPairTester.UseSimplexCaching = true;
            //GroundShape = new StaticPlaneShape(Vector3.UnitY, 1.0f);
            //Ground = CreateRigidBody(0, Matrix4.CreateTranslation(0, 0, 0), GroundShape, null);
            CollisionObjects = new Dictionary<IRenderable, Entity>();
            if(Root == null)
                Root = this;
        }

        public void CreateRigidBody(float mass, Matrix4 startTransform, Entity shape, Mesh3d reference)
        {
            shape.Mass = mass;
            shape.Tag = reference;
            shape.WorldTransform = startTransform;

            PhysicalWorld.Add(shape);
        }

        public virtual void UpdatePhysics(float time)
        {
            PhysicalWorld.Update(time);

            int len = PhysicalWorld.Entities.Count;
            Mesh3d mesh;
            Entity body;
            for(int i = 0; i < len; i++)
            {
                body = PhysicalWorld.Entities[i];
                mesh = body.Tag as Mesh3d;
                if(mesh != null)
                {
                    //mesh.UpdateMatrixFromPhysics(body.OrientationMatrix * body.Position);
                    mesh.SetOrientation(body.BufferedStates.InterpolatedStates.Orientation);
                    mesh.SetPosition(body.BufferedStates.InterpolatedStates.Position);

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
                        PhysicalWorld.Add(mesh.GetCollisionShape());
                    }
                }
                if(mesh.GetStaticCollisionMesh() != null || renderable is IPhysical)
                {
                    lock(PhysicalWorld)
                    {
                        PhysicalWorld.Add(mesh.GetStaticCollisionMesh());
                    }
                }
            }
            else if(renderable is IPhysical)
            {
                IPhysical physicalObject = renderable as IPhysical;
                lock(PhysicalWorld)
                {
                    PhysicalWorld.Add(physicalObject.GetCollisionShape());
                }

            }
        }

        public void Draw()
        {
            for(int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw();
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
                    PhysicalWorld.Remove(CollisionObjects[renderable]);
                }
            }
        }

        public void UpdateMatrix()
        {
            Matrix = Matrix4.CreateTranslation(Position) * Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateScale(Scale);
        }
    }
}