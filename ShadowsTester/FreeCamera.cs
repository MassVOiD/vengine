using System;
using System.Text;
using OpenTK;
using VEngine;

namespace ShadowsTester
{
    public class TitleOutput
    {
        static public string Message = "Initialized";
    }
    internal class FreeCamera
    {
        public Camera Cam;

        public bool Freeze = false;

        public FreeCamera(float aspectRatio, float fov)
        {
            float fovdegree = 90;
            Cam = new Camera(new Vector3(0, 5, 0), new Vector3(0, 0, 1), Vector3.UnitY, aspectRatio, MathHelper.DegreesToRadians(fovdegree), 0.1f, 10000.0f);
            Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
            Camera.MainDisplayCamera = Cam;

            Game.OnBeforeDraw += UpdateSterring;
            Game.OnMouseMove += OnMouseMove;

            Game.OnResize += (o, e) =>
            {
                float aspect = (float)Game.Resolution.Width / (float)Game.Resolution.Height;
                aspectRatio = aspect;
                Matrix4 a = Matrix4.Zero;
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), aspect, 0.1f, 10000.0f, out a);
                Cam.SetProjectionMatrix(a);
                Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
            };
            Game.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.M)
                {
                    fovdegree += 5f;
                    if(fovdegree >= 179)
                        fovdegree = 179;
                    Matrix4 a = Matrix4.Zero;
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), aspectRatio, 0.1f, 10000.0f, out a);
                    Cam.SetProjectionMatrix(a);
                    Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
                }
                if(e.Key == OpenTK.Input.Key.N)
                {
                    fovdegree -= 5f;
                    if(fovdegree <= 10)
                        fovdegree = 10;
                    Matrix4 a = Matrix4.Zero;
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fovdegree), aspectRatio, 0.1f, 10000.0f, out a);
                    Cam.SetProjectionMatrix(a);
                    Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
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
        
        private void UpdateSterring(object o, OpenTK.FrameEventArgs e)
        {
            var time = e.Time;
            float fps = (float)Math.Round(1.0 / e.Time, 2);
            float ft = (float)Math.Round(e.Time * 1000.0, 2);
            var mem = (double)GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            float MBmemory = (float)Math.Round(mem, 2);
            StringBuilder newTitle = new StringBuilder();
            newTitle.Append(string.Format("VEngine App | FPS: {0} | FrameTime: {1} ms | Memory: {2} megabytes", fps.ToString(System.Globalization.CultureInfo.InvariantCulture), ft.ToString(System.Globalization.CultureInfo.InvariantCulture), MBmemory.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            while(newTitle.Length < 71)
                newTitle.Append(" ");
            newTitle.Append("$>");
            newTitle.Append(TitleOutput.Message);
            Game.DisplayAdapter.Title = newTitle.ToString();
            var currentPosition = Cam.GetPosition();
            if(Game.DisplayAdapter.IsCursorVisible)
                return;
            var keyboard = OpenTK.Input.Keyboard.GetState();
            KeyboardHandler.Process();

            float speed = (float)time;
            if(keyboard.IsKeyDown(OpenTK.Input.Key.ShiftLeft))
            {
                speed *= 7f;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.AltLeft))
            {
                speed *= 20f;
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
                currentPosition -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitZ;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                currentPosition -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                currentPosition -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                currentPosition -= direction.Xyz * speed;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.Space))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitY;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                currentPosition -= direction.Xyz * speed;
            }

            // rigidBody.LinearVelocity = new Vector3( rigidBody.LinearVelocity.X * 0.94f,
            // rigidBody.LinearVelocity.Y * 0.94f, rigidBody.LinearVelocity.Z * 0.94f);
            if(Cam.Transformation.GetPosition() != currentPosition)
            {
                Cam.Transformation.SetPosition(currentPosition);
                Cam.Transformation.MarkAsModified();
            }

            //Cam.UpdateFromRollPitch();
            Cam.Update();
        }
    }
}