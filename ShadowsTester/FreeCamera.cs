using System;
using BulletSharp;
using OpenTK;
using VEngine;

namespace ShadowsTester
{
    internal class FreeCamera
    {
        public Camera Cam;

        public bool Freeze = false;

        private SphereShape collisionShape;

        private bool GravityEnabled = false;

        private RigidBody rigidBody;

        public FreeCamera(float aspectRatio, float fov)
        {
            float fovdegree = 90;
            Cam = new Camera(new Vector3(0, 0, 0), new Vector3(0, 2, 0), aspectRatio, MathHelper.DegreesToRadians(fovdegree), 0.1f, 10000.0f);
            Camera.MainDisplayCamera = Cam;
            collisionShape = new SphereShape(0.8f);
            //collisionShape.LinearDamping = 0.5f;
            rigidBody = World.Root.CreateRigidBody(0.01f, Matrix4.CreateTranslation(Cam.Transformation.GetPosition()), collisionShape, null);
            rigidBody.SetSleepingThresholds(0, 0);
            rigidBody.ContactProcessingThreshold = 0;
            rigidBody.CcdMotionThreshold = 0;
            World.Root.PhysicalWorld.AddRigidBody(rigidBody);
            rigidBody.Gravity = Vector3.Zero;
            rigidBody.ApplyGravity();
            rigidBody.SetDamping(0.9f, 0.01f);
            GLThread.OnUpdate += UpdateSterring;
            GLThread.OnMouseMove += OnMouseMove;

            GLThread.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.M)
                {
                    fovdegree += 5f;    
                    if(fovdegree >= 180)
                        fovdegree = 180;
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), aspectRatio, 0.1f, 10000.0f, out Cam.ProjectionMatrix);
                    Camera.Current.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
                }
                if(e.Key == OpenTK.Input.Key.N)
                {
                    fovdegree -= 5f;
                    if(fovdegree <= 10)
                        fovdegree = 10;
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), aspectRatio, 0.1f, 10000.0f, out Cam.ProjectionMatrix);
                    Camera.Current.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
                }
                if(e.Key == OpenTK.Input.Key.P)
                {
                    GravityEnabled = !GravityEnabled;
                    if(GravityEnabled)
                    {
                        rigidBody.Gravity = new Vector3(0, -9.81f, 0);
                        rigidBody.ApplyGravity();
                        rigidBody.SetDamping(0.8f, 0.01f);
                    }
                    else
                    {
                        rigidBody.Gravity = Vector3.Zero;
                        rigidBody.ApplyGravity();
                        rigidBody.SetDamping(0.5f, 0.01f);
                    }
                }
            };
        }

        private void OnMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if(Freeze)
                return;
            Cam.Pitch += (float)e.XDelta / 100.0f;
            if(Cam.Pitch > MathHelper.TwoPi)
                Cam.Pitch -= MathHelper.TwoPi;

            Cam.Roll += (float)e.YDelta / 100.0f;
            if(Cam.Roll > MathHelper.Pi / 2)
                Cam.Roll = MathHelper.Pi / 2;
            if(Cam.Roll < -MathHelper.Pi / 2)
                Cam.Roll = -MathHelper.Pi / 2;

            //Cam.Transformation.SetPosition(rigidBody.WorldTransform.ExtractTranslation());

            Cam.UpdateFromRollPitch();
            Cam.Transformation.ClearModifiedFlag();
        }

        private void UpdateSterring(object o, EventArgs e)
        {
            if(GLThread.DisplayAdapter.IsCursorVisible)
                return;
            var keyboard = OpenTK.Input.Keyboard.GetState();
            KeyboardHandler.Process();

            float speed = 0.05f;
            if(keyboard.IsKeyDown(OpenTK.Input.Key.ShiftLeft))
            {
                speed *= 4;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.AltLeft))
            {
                speed *= 20;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.ControlLeft))
            {
                speed *= 0.03f;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.LBracket))
            {
                Camera.MainDisplayCamera.Brightness -= 0.003f;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.RBracket))
            {
                Camera.MainDisplayCamera.Brightness += 0.003f;
            }

            if(keyboard.IsKeyDown(OpenTK.Input.Key.W))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = Vector4.UnitZ;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                rigidBody.LinearVelocity -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitZ;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                rigidBody.LinearVelocity -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                rigidBody.LinearVelocity -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                rigidBody.LinearVelocity -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.Space))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitY;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                rigidBody.LinearVelocity -= direction.Xyz * speed;
            }

            // rigidBody.LinearVelocity = new Vector3( rigidBody.LinearVelocity.X * 0.94f,
            // rigidBody.LinearVelocity.Y * 0.94f, rigidBody.LinearVelocity.Z * 0.94f);
            Cam.Transformation.SetPosition(rigidBody.WorldTransform.ExtractTranslation());

            Cam.UpdateFromRollPitch();
            Cam.Transformation.MarkAsModified();
        }
    }
}