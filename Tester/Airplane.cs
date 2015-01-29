using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BEPUphysics.Entities.Prefabs;

namespace Tester
{
    partial class Airplane : IRenderable, IPhysical
    {
        public Mesh3d Body;

        Object3dInfo Body3dInfo;
        CameraMode Mode;
        public Airplane()
        {
            Body3dInfo = Object3dInfo.LoadFromCompressed(Media.Get("airplane.rend"));
            Mode = CameraMode.BehindTowardsVelocity;
            Body = new Mesh3d(Body3dInfo, new SolidColorMaterial(Color.Cyan));
            Body.SetMass(1.0f);
            Body.SetPosition(new Vector3(400, 900, 0));
            Body.SetCollisionShape(new Box(Body.GetPosition(), 10,2,10, 100.0f));
            Body.SetOrientation(Quaternion.FromAxisAngle(new Vector3(1, 1, 1), 0.4f));
            
            GLThread.OnUpdate += OnUpdate;
            GLThread.OnBeforeDraw += GLThread_OnBeforeDraw;
            GLThread.OnMouseMove += GLThread_OnMouseMove;
        }

        void GLThread_OnMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (Mode == CameraMode.Free)
            {
                Camera.Current.Pitch += (float)e.XDelta / 100.0f;
                if (Camera.Current.Pitch > MathHelper.TwoPi) Camera.Current.Pitch = 0.0f;

                Camera.Current.Roll += (float)e.YDelta / 100.0f;
                if (Camera.Current.Roll > MathHelper.Pi / 2) Camera.Current.Roll = MathHelper.Pi / 2;
                if (Camera.Current.Roll < -MathHelper.Pi / 2) Camera.Current.Roll = -MathHelper.Pi / 2;

                Camera.Current.UpdateFromRollPitch();
            }
        }

        enum CameraMode
        {
            Free,
            StrictBehind,
            BehindNoRoll,
            BehindTowardsVelocity
        }
        
        void GLThread_OnBeforeDraw(object sender, EventArgs e)
        {
            if (Mode == CameraMode.BehindTowardsVelocity)
            {
                float velocity = Body.GetCollisionShape().LinearVelocity.Length();
                float upforce = velocity < 0.1f ? 0 : 2.0f * (float)Math.Atan(velocity / 70.0f) / (float)Math.PI * 3.0f;
                var bodyDirection = Body.GetOrientation().ToDirection();
                var upDirection = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                Camera.Current.Position = Body.GetPosition() - Body.GetOrientation().ToDirection() * 15.0f * (upforce + 1.0f) + upDirection * 5.0f;
                Camera.Current.Position.Y = Body.GetPosition().Y;

                Camera.Current.LookAt(Body.GetPosition() - (Body.GetCollisionShape().LinearVelocity.ToOpenTK() * 0.02f));
                //Camera.Current.Orientation = Body.GetOrientation();
            }
            else if (Mode == CameraMode.StrictBehind)
            {
                float velocity = Body.GetCollisionShape().LinearVelocity.Length();
                var bodyDirection = Body.GetOrientation().ToDirection();
                Camera.Current.Position = Body.GetPosition() - Body.GetOrientation().ToDirection() * 15.0f * ((velocity + 10.0f)/500.0f);

                //Camera.Current.LookAt(Body.GetPosition() - Body.PhysicalBody.LinearVelocity * 0.02f);
                Camera.Current.Orientation = Body.GetOrientation();
                Camera.Current.Update();
            }
        }

        public void Delete()
        {
            GLThread.OnUpdate -= OnUpdate;
        }
        public void Draw()
        {
            Body.Draw();
        }

    }
}
