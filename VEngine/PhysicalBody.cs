using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace VEngine
{
    public class PhysicalBody : ITransformable
    {
        public RigidBody Body;
        public CollisionShape Shape;
        public TransformationManager Transformation;
        private bool Enabled;

        public PhysicalBody(RigidBody rigidBody, CollisionShape shape, TransformationManager manager)
        {
            Body = rigidBody;
            Shape = shape;
            Transformation = manager;
            Enabled = false;
        }

        public void ApplyChanges()
        {
            Transformation.SetPosition(Body.CenterOfMassPosition);
            Transformation.SetOrientation(Body.Orientation);
        }

        public void Disable()
        {
            Enabled = false;
            Game.World.Physics.RemoveBody(this);
        }

        public void Enable()
        {
            Enabled = true;
            Game.World.Physics.AddBody(this);
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public bool IsEnabled()
        {
            return Enabled;
        }

        public void ReadChanges()
        {
            Body.WorldTransform = Transformation.GetWorldTransform();
            Shape.LocalScaling = Transformation.GetScale();
        }
    }
}