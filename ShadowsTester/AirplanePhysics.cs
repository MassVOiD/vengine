using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BEPUphysics.Entities;

namespace ShadowsTester
{
    partial class Airplane : IRenderable, IPhysical
    {
        
        void OnUpdate(object sender, EventArgs e)
        {
            var bodyDirection = Body.GetOrientation().ToDirection();
            var velocityVector = Body.GetCollisionShape().LinearVelocity;
            float velocity = velocityVector.Length();
            if(Mode == CameraMode.BehindTowardsVelocity)
            {
                float upforce = velocity < 0.1f ? 0 : 2.0f * (float)Math.Atan(velocity / 70.0f) / (float)Math.PI * 3.0f;
                var upDirection = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                Camera.Current.Position = Body.GetPosition() - Body.GetOrientation().ToDirection() * 15.0f * (upforce + 1.0f) + upDirection * 5.0f;
                Camera.Current.Position.Y = Body.GetPosition().Y;

                Camera.Current.LookAt(Body.GetPosition() - (Body.GetCollisionShape().LinearVelocity.ToOpenTK() * 0.02f));
                //Camera.Current.Orientation = Body.GetOrientation();
            }
            else if(Mode == CameraMode.StrictBehind)
            {
                Camera.Current.Position = Body.GetPosition() - Body.GetOrientation().ToDirection() * 15.0f * ((velocity + 10.0f) / 500.0f);

                //Camera.Current.LookAt(Body.GetPosition() - Body.PhysicalBody.LinearVelocity * 0.02f);
                Camera.Current.Orientation = Body.GetOrientation();
                Camera.Current.Update();
            }
            UpdateSterring();
            //$-\ln \left(x+0.07\right)-0.06$
            var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
            velocityVector.Normalize();
            if(velocity > 1.0f)
            {
                //float velocityMult = Vector3.Dot(velocityVector, bodyDirection);
                //var impulse = (up * velocityMult * 4.2f).ToBepu();
                //Body.GetCollisionShape().LinearVelocity += impulse;
            }

           
            //Body.GetCollisionShape().AngularVelocity *= 0.95f;
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
