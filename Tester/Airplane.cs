using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BulletSharp;

namespace Tester
{
    class Airplane : IRenderable, IPhysical
    {
        Mesh3d Body;

        Object3dInfo Body3dInfo;
        public Airplane()
        {
            Body3dInfo = Object3dInfo.LoadFromCompressed(Media.Get("airplane.rend"));

            Body = new Mesh3d(Body3dInfo, new SolidColorMaterial(Color.Cyan));
            Body.SetMass(10.0f);
            Body.SetCollisionShape(new BoxShape(10,2,10));
            Body.SetOrientation(Quaternion.FromAxisAngle(new Vector3(1, 1, 1), 0.4f));
            Body.SetPosition(new Vector3(400, 900, 0));
            
            GLThread.OnUpdate += OnUpdate;
            GLThread.OnBeforeDraw += GLThread_OnBeforeDraw;
            GLThread.OnKeyDown += GLThread_OnKeyDown;
        }

        void GLThread_OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            
        }

        void UpdateSterring()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard.IsKeyDown(OpenTK.Input.Key.W))
            {
                var bodyDirection = Body.GetOrientation().ToDirection();
                // DAMP A HELL OF IT 
                //Body.PhysicalBody.LinearVelocity *= 0.993f;
                Body.PhysicalBody.ApplyCentralForce(bodyDirection * 400.0f);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                Body.PhysicalBody.LinearVelocity *= 0.993f;
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                var left = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                float damping = 1.0f / (Body.PhysicalBody.LinearVelocity.Length + 0.13f) + 4.35f;
                Body.PhysicalBody.ApplyForce(down * 250.0f * damping, Body.GetPosition() + right);
                Body.PhysicalBody.ApplyForce(up * 250.0f * damping, Body.GetPosition() + left);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                var left = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                float damping = 1.0f / (Body.PhysicalBody.LinearVelocity.Length + 0.13f) + 4.35f;
                Body.PhysicalBody.ApplyForce(up * 250.0f * damping, Body.GetPosition() + right);
                Body.PhysicalBody.ApplyForce(down * 250.0f * damping, Body.GetPosition() + left);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.J))
            {
                var forward = Body.GetOrientation().ToDirection();
                var backward = -Body.GetOrientation().ToDirection();
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                float damping = 1.0f / (Body.PhysicalBody.LinearVelocity.Length + 0.13f) + 0.85f;
                Body.PhysicalBody.ApplyForce(up * 290.0f * damping, Body.GetPosition() + forward);
                Body.PhysicalBody.ApplyForce(down * 290.0f * damping, Body.GetPosition() + backward);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.U))
            {
                var forward = Body.GetOrientation().ToDirection();
                var backward = -Body.GetOrientation().ToDirection();
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                float damping = 1.0f / (Body.PhysicalBody.LinearVelocity.Length + 0.13f) + 0.85f;
                Body.PhysicalBody.ApplyForce(up * 290.0f * damping, Body.GetPosition() + backward);
                Body.PhysicalBody.ApplyForce(down * 290.0f * damping, Body.GetPosition() + forward);
            }
        }

        void GLThread_OnBeforeDraw(object sender, EventArgs e)
        {
            float velocity = Body.PhysicalBody.LinearVelocity.Length;
            float upforce = velocity < 0.1f ? 0 : 2.0f * (float)Math.Atan(velocity / 70.0f) / (float)Math.PI * 3.0f;
            var bodyDirection = Body.GetOrientation().ToDirection();
            var upDirection = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
            Camera.Current.Position = Body.GetPosition() - Body.GetOrientation().ToDirection() * 25.0f * (upforce + 1.0f) + upDirection * 5.0f;
            //Camera.Current.Orientation = Body.GetOrientation();
            //Camera.Current.Update();
            Camera.Current.LookAt(Body.GetPosition() + Body.GetOrientation().ToDirection() * 125.0f);
        }

        public void Delete()
        {
            GLThread.OnUpdate -= OnUpdate;
        }

        void OnUpdate(object sender, EventArgs e)
        {
            UpdateSterring();
            //$-\ln \left(x+0.07\right)-0.06$
            var bodyDirection = Body.GetOrientation().ToDirection();
            var relativePos = Body.GetPosition() - bodyDirection * 3.0f;
            float velocity = Body.PhysicalBody.LinearVelocity.Length;
            float upforce = 2.0f * (float)Math.Atan(velocity/70.0f) / (float)Math.PI * 230.0f;

            Debugger.Send("upforce", upforce);
            Debugger.Send("velocity", velocity);

            var upDirection = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
            Body.PhysicalBody.ApplyCentralForce(upDirection * upforce);
            //Body.PhysicalBody.ApplyCentralForce(bodyDirection * velocity);
            Body.PhysicalBody.AngularVelocity *= 0.95f;
            Body.PhysicalBody.LinearVelocity *= 0.999f;
            Body.PhysicalBody.ApplyCentralForce(bodyDirection * Body.PhysicalBody.LinearVelocity.Length);
            
        }

        public void Draw()
        {
            Body.Draw();
        }

        public CollisionShape GetCollisionShape()
        {
            return Body.GetCollisionShape();
        }
        public RigidBody GetRigidBody()
        {
            Body.CreateRigidBody();
            return Body.PhysicalBody;
        }
    }
}
