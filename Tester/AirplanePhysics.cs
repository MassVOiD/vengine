using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BEPUphysics.Entities;

namespace Tester
{
    partial class Airplane : IRenderable, IPhysical
    {
        
        void OnUpdate(object sender, EventArgs e)
        {
            UpdateSterring();
            //$-\ln \left(x+0.07\right)-0.06$
            var bodyDirection = Body.GetOrientation().ToDirection();
            var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
            var velocityVector = Body.GetCollisionShape().LinearVelocity;
            float velocity = velocityVector.Length();
            velocityVector.Normalize();
            if(velocity > 1.0f)
            {
                //float velocityMult = Vector3.Dot(velocityVector, bodyDirection);
                //var impulse = (up * velocityMult * 4.2f).ToBepu();
                //Body.GetCollisionShape().LinearVelocity += impulse;
            }

           
            Body.GetCollisionShape().AngularVelocity *= 0.95f;
            //Body.GetCollisionShape().LinearVelocity *= 0.998f;
            
        }
        
        public Entity GetCollisionShape()
        {
            return Body.GetCollisionShape();
        }
        public Entity GetRigidBody()
        {
            return Body.GetCollisionShape();
        }
    }
}
