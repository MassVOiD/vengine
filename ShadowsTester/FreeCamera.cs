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
            Cam = new Camera(new Vector3(0, 5, 0), new Vector3(0, 0, 1), Vector3.UnitY, aspectRatio, MathHelper.DegreesToRadians(fovdegree), 0.01f, 10000.0f);
            Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
            Camera.MainDisplayCamera = Cam;
            Camera.Current = Cam;

            Game.OnBeforeDraw += UpdateSterring;
            Game.OnMouseMove += OnMouseMove;

            Game.OnResize += (o, e) =>
            {
                float aspect = (float)Game.Resolution.Width / (float)Game.Resolution.Height;
                aspectRatio = aspect;
                Cam.UpdatePerspective(aspect, MathHelper.DegreesToRadians(fovdegree), 0.1f, 10000.0f);
                Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
            };
            Game.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.M)
                {
                    fovdegree += 5f;
                    if(fovdegree >= 179)
                        fovdegree = 179;
                    Cam.UpdatePerspective(aspectRatio, MathHelper.DegreesToRadians(fovdegree), 0.1f, 10000.0f);
                    Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
                }
                if(e.Key == OpenTK.Input.Key.N)
                {
                    fovdegree -= 5f;
                    if(fovdegree <= 10)
                        fovdegree = 10;
                    Cam.UpdatePerspective(aspectRatio, MathHelper.DegreesToRadians(fovdegree), 0.1f, 10000.0f);
                    Cam.FocalLength = (float)(43.266f / (2.0f * Math.Tan(Math.PI * fovdegree / 360.0f))) / 1.5f;
                }
            };
        }

        private void OnMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            /*
            if(Freeze || (e.XDelta == 0 && e.YDelta == 0))
                return;

            float newPitch = Cam.Pitch + (float)e.XDelta / 100.0f;
            if(newPitch > MathHelper.TwoPi)
                newPitch -= MathHelper.TwoPi;

            float newRoll = Cam.Roll + (float)e.YDelta / 100.0f;
            if(newRoll > MathHelper.Pi / 2)
                newRoll = MathHelper.Pi / 2;
            else if(newRoll < -MathHelper.Pi / 2)
                newRoll = -MathHelper.Pi / 2;

            if(newRoll != Cam.Roll)
                Cam.Roll = newRoll;
            if(newPitch != Cam.Pitch)
                Cam.Pitch = newPitch;

            Console.WriteLine("{0}, {1}", Cam.Pitch, Cam.Roll);*/

            //Cam.Transformation.SetPosition(rigidBody.WorldTransform.ExtractTranslation());
            //Cam.Transformation.ClearModifiedFlag();
            //Game.Invoke(() => Cam.UpdateFromRollPitch());

        }

        float mousespeedX = 0.0f, mousespeedY = 0.0f;
        Vector3 camvelocity = Vector3.Zero;

        private void UpdateSterring(object o, OpenTK.FrameEventArgs e)
        {
            Camera.MainDisplayCamera = Cam;
            var time = e.Time;
            float fps = (float)Math.Round(1.0 / e.Time, 2);
            float ft = (float)Math.Round(e.Time * 1000.0, 2);
            var mem = (double)GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            float MBmemory = (float)Math.Round(mem, 2);
            StringBuilder newTitle = new StringBuilder();
            newTitle.Append(string.Format("VEngine App | FPS: {0} | FrameTime: {1} ms | Memory: {2} megabytes | Drawn {3} ", fps.ToString(System.Globalization.CultureInfo.InvariantCulture), ft.ToString(System.Globalization.CultureInfo.InvariantCulture), MBmemory.ToString(System.Globalization.CultureInfo.InvariantCulture), Game.World.Scene.LastDrawnObjectsCount));
            while(newTitle.Length < 71)
                newTitle.Append(" ");
            newTitle.Append("$>");
            newTitle.Append(TitleOutput.Message);
            Game.DisplayAdapter.Title = newTitle.ToString();
            var currentPosition = Cam.GetPosition();
            if(Game.DisplayAdapter.IsCursorVisible)
                return;

            var mouse = OpenTK.Input.Mouse.GetCursorState();


            var p = Game.DisplayAdapter.PointToScreen(new System.Drawing.Point(Game.Resolution.Width / 2, Game.Resolution.Height / 2));
            var p3 = (new System.Drawing.Point(Game.Resolution.Width / 2, Game.Resolution.Height / 2));
            var p2 = (new System.Drawing.Point(mouse.X, mouse.Y));
            //Console.WriteLine(p2);
            int deltaX = p2.X - p.X;
            int deltaY = p2.Y - p.Y;
            System.Windows.Forms.Cursor.Position = p;

            mousespeedX += (float)deltaX / 200.0f;
            mousespeedX *= 0.7f;
            mousespeedY += (float)deltaY / 200.0f;
            mousespeedY *= 0.7f;

            bool needsUpdate = false;
            if(!Freeze)
            {

                float newPitch = Cam.Pitch + mousespeedX;
                if(newPitch > MathHelper.TwoPi)
                    newPitch -= MathHelper.TwoPi;

                float newRoll = Cam.Roll + mousespeedY;
                if(newRoll > MathHelper.Pi / 2)
                    newRoll = MathHelper.Pi / 2;
                else if(newRoll < -MathHelper.Pi / 2)
                    newRoll = -MathHelper.Pi / 2;

                if(newRoll != Cam.Roll)
                {
                    Cam.Roll = newRoll;
                    needsUpdate = true;
                }
                if(newPitch != Cam.Pitch)
                {
                    Cam.Pitch = newPitch;
                    needsUpdate = true;
                }
            }


            var keyboard = OpenTK.Input.Keyboard.GetState(0);
            //KeyboardHandler.Process();

            float speed = 0.01f;
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
                speed *= 300f;
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
                camvelocity -= direction.Xyz * speed;
                needsUpdate = true;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitZ;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                camvelocity -= direction.Xyz * speed;
                needsUpdate = true;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                camvelocity -= direction.Xyz * speed;
                needsUpdate = true;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                //direction.Y = 0.0f;
                camvelocity -= direction.Xyz * speed;
                needsUpdate = true;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.Space))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitY;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                camvelocity -= direction.Xyz * speed;
                needsUpdate = true;
            }

            currentPosition += camvelocity * 0.1f;
            camvelocity *= 0.93f;

            // rigidBody.LinearVelocity = new Vector3( rigidBody.LinearVelocity.X * 0.94f,
            // rigidBody.LinearVelocity.Y * 0.94f, rigidBody.LinearVelocity.Z * 0.94f);
            if(needsUpdate)
            {
                Cam.Transformation.SetPosition(currentPosition);
                Cam.Transformation.ClearModifiedFlag();
                // Cam.Update();
                Cam.UpdateFromRollPitch();
            } else
                Cam.Update();

            //Cam.UpdateFromRollPitch();
            //Cam.Update();
        }
    }
}