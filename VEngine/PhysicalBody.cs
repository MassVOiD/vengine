using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace VEngine
{
    class PhysicalBody : ITransformable
    {
        public CollisionShape Shape;
        public TransformationManager Transformation;
        public RigidBody Body;

        public PhysicalBody()
        {

        }
        
        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public bool IsEnabled()
        {
            return false;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}
