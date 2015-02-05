using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDGTech;
using OpenTK;

namespace ShadowsTester
{
    class FreeCamera
    {
        public Camera Cam;
        public FreeCamera()
        {
            Cam = new Camera(new Vector3(0, 20, 0), new Vector3(0, 2, 0), 1600.0f / 900.0f, 3.14f / 2.0f, 2.0f, 20000.0f);
            GLThread.OnUpdate += UpdateSterring;
            GLThread.OnMouseMove += OnMouseMove;
        }

        void OnMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            Cam.Pitch += (float)e.XDelta / 100.0f;
            if(Cam.Pitch > MathHelper.TwoPi)
                Cam.Pitch = 0.0f;

            Cam.Roll += (float)e.YDelta / 100.0f;
            if(Cam.Roll > MathHelper.Pi / 2)
                Cam.Roll = MathHelper.Pi / 2;
            if(Cam.Roll < -MathHelper.Pi / 2)
                Cam.Roll = -MathHelper.Pi / 2;

            Cam.UpdateFromRollPitch();
        }

        void UpdateSterring(object o, EventArgs e)
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            KeyboardHandler.Process();

            float speed = 1;
            if(keyboard.IsKeyDown(OpenTK.Input.Key.ShiftLeft))
            {
                speed *= 4;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.AltLeft))
            {
                speed *= 60;
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.ControlLeft))
            {
                speed *= 0.3f;
            }

            if(keyboard.IsKeyDown(OpenTK.Input.Key.W))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = Vector4.UnitZ;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                Cam.Position -= direction.Xyz * speed;
                Cam.UpdateFromRollPitch();
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitZ;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                Cam.Position -= direction.Xyz * speed;
                Cam.UpdateFromRollPitch();
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                Cam.Position -= direction.Xyz * speed;
                Cam.UpdateFromRollPitch();
            }
            if(keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Cam.Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Cam.Roll);
                Vector4 direction = -Vector4.UnitX;
                direction = Vector4.Transform(direction, rotationY);
                direction = Vector4.Transform(direction, rotationX);
                Cam.Position -= direction.Xyz * speed;
                Cam.UpdateFromRollPitch();
            }

        }
    }
}
