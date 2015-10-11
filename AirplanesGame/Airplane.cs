using System;
using System.Drawing;
using BulletSharp;
using OpenTK;
using VEngine;

namespace AirplanesGame
{
    internal class Airplane
    {
        public Mesh3d Body;

        private Object3dInfo Body3dInfo;

        public Airplane(Scene scene)
        {
            Body3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("stuka.obj"));
            Body3dInfo.Normalize();

            Body = new Mesh3d(Body3dInfo, new GenericMaterial(Color.Cyan));
            Body.SetMass(13.1f);
            Body.Scale(0.1f);
            Body.SetCollisionShape(new BoxShape(0.1f, 0.04f, 0.01f));

            Body.SetOrientation(Quaternion.FromAxisAngle(new Vector3(1, 1, 1), 0.4f));
            Body.SetPosition(new Vector3(1, 1, 1));
            scene.Add(Body);

            GLThread.OnUpdate += OnUpdate;
            GLThread.OnBeforeDraw += GLThread_OnBeforeDraw;
            GLThread.OnKeyDown += GLThread_OnKeyDown;
        }

        public void Delete()
        {
            GLThread.OnUpdate -= OnUpdate;
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

        private void GLThread_OnBeforeDraw(object sender, EventArgs e)
        {
            float velocity = Body.PhysicalBody.LinearVelocity.Length;
            float upforce = velocity < 0.1f ? 0 : 2.0f * (float)Math.Atan(velocity / 70.0f) / (float)Math.PI * 3.0f;
            var bodyDirection = Body.GetOrientation().ToDirection();
            var upDirection = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
            Camera.Current.SetPosition(Body.GetPosition() - Body.GetOrientation().ToDirection() * 0.1f * (upforce + 1.0f) + upDirection * 0.01f);
            Camera.Current.Transformation.Position.R.Y = Body.GetPosition().Y;

            Camera.Current.LookAt(Body.GetPosition() + Body.PhysicalBody.LinearVelocity * 0.001f);
            //Camera.Current.Transformation.Orientation.R = Body.GetOrientation();
        }

        private void GLThread_OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            UpdateSterring();
            //$-\ln \left(x+0.07\right)-0.06$
            var bodyDirection = Body.GetOrientation().ToDirection();
            var relativePos = Body.GetPosition() - bodyDirection * 3.0f;
            float velocity = Body.PhysicalBody.LinearVelocity.Length;
            float upforce = 2.0f * (float)Math.Atan(velocity / 70.0f) / (float)Math.PI * 230.0f;

            var upDirection = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
            Body.PhysicalBody.ApplyCentralForce(upDirection * upforce);
            //Body.PhysicalBody.ApplyCentralForce(bodyDirection * velocity);
            Body.PhysicalBody.AngularVelocity *= 0.95f;
            Body.PhysicalBody.LinearVelocity = (Body.PhysicalBody.LinearVelocity * 50 + Body.GetOrientation().ToDirection() * Body.PhysicalBody.LinearVelocity.Length) / 51.0f;
            Body.PhysicalBody.ApplyCentralForce(-bodyDirection * Body.PhysicalBody.LinearVelocity.Length);
        }

        private void UpdateSterring()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            Body.PhysicalBody.Gravity = new Vector3(0, -0.5f, 0);
            Body.PhysicalBody.ApplyGravity();
            if(keyboard.IsKeyDown(OpenTK.Input.Key.W))
            {
                var bodyDirection = Body.GetOrientation().ToDirection();
                Body.PhysicalBody.LinearVelocity += bodyDirection * 0.18f;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                Body.PhysicalBody.LinearVelocity *= 0.91f;
                Body.PhysicalBody.AngularVelocity *= 0.93f;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                Body.PhysicalBody.AngularVelocity += Vector3.Transform(new Vector3(0, 0, -0.1f), Body.PhysicalBody.WorldTransform.ExtractRotation());
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                Body.PhysicalBody.AngularVelocity += Vector3.Transform(new Vector3(0, 0, 0.1f), Body.PhysicalBody.WorldTransform.ExtractRotation());
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.J))
            {
                Body.PhysicalBody.AngularVelocity += Vector3.Transform(new Vector3(-0.1f, 0, 0), Body.PhysicalBody.WorldTransform.ExtractRotation());
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.U))
            {
                Body.PhysicalBody.AngularVelocity += Vector3.Transform(new Vector3(0.1f, 0, 0), Body.PhysicalBody.WorldTransform.ExtractRotation());
            }
        }
    }
}