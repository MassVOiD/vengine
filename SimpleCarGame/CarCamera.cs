using System;
using OpenTK;
using VEngine;

namespace SimpleCarGame
{
    internal class CarCamera
    {
        public Camera Cam;

        private float Horizontal, Vertical;

        public CarCamera(float aspectRatio, float fov)
        {
            Cam = new Camera(new Vector3(20, 20, 20), new Vector3(0, 2, 0), aspectRatio, fov, 1.0f, 10000.0f);
            GLThread.OnMouseMove += OnMouseMove;
            GLThread.OnUpdate += GLThread_OnUpdate;
        }

        private void GLThread_OnUpdate(object sender, EventArgs e)
        {
            if(CarScene.CurrentCar == null)
                return;
            /*var tmp = CarScene.CurrentCar.GetOrientation().ToDirection();
            var d = Vector2.Dot(tmp.Xz, Vector2.UnitX);
            float a = (float)(Math.Acos(d));
            if(d < 0)
                a = -(1.0f - a);
            Horizontal = (Horizontal * 10 + a) / 11;*/
            var initial = CarScene.CurrentCar.GetPosition();
            var displace = (Vector3.UnitZ * 20).Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -Vertical));
            displace = displace.Rotate(Quaternion.FromAxisAngle(Vector3.UnitY, -Horizontal));
            var res = initial + displace;
            if(res.Y < -10)
                res.Y = -10;
            Cam.SetPosition((Cam.GetPosition() * 21 + res) / 22);
            Cam.LookAt(initial);

            //Cam.Transformation.SetPosition(rigidBody.WorldTransform.ExtractTranslation());

            //Cam.Update();
            Cam.Transformation.ClearModifiedFlag();
        }

        private void OnMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if(CarScene.CurrentCar == null)
                return;
            Horizontal += (float)e.XDelta / 100.0f;
            if(Horizontal > MathHelper.TwoPi)
                Horizontal = -MathHelper.TwoPi;
            if(Horizontal < -MathHelper.TwoPi)
                Horizontal = MathHelper.TwoPi;

            Vertical += (float)e.YDelta / 100.0f;
            if(Vertical > MathHelper.Pi / 2 - 0.01f)
                Vertical = MathHelper.Pi / 2 - 0.01f;
            if(Vertical < -0.2f)
                Vertical = -0.2f;
        }
    }
}