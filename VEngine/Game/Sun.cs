using System.Collections.Generic;
using OpenTK;

namespace VEngine
{
    public class Sun
    {
        public static Sun Current;
        public Vector3 LightColor;
        public Quaternion Orientation;

        private Dictionary<float, ProjectionLight> Cascades;

        public Sun(Quaternion orientation, Vector3 color, params float[] levels)
        {
            Current = this;
            LightColor = color;
            Orientation = orientation;
            Cascades = new Dictionary<float, ProjectionLight>();

            for(int i = 0; i < levels.Length; i++)
            {
                float start = levels[i];
                //float end = levels[i + 1];
                var casc = new ProjectionLight(Camera.MainDisplayCamera.GetPosition(), orientation, 2048, 2048, (MathHelper.PiOver2 - 0.1f) / levels[i], 100f, 3000.0f);
                casc.LightColor = color;

                casc.FBO.DepthPixelType = OpenTK.Graphics.OpenGL4.PixelType.Float;
                casc.FBO.DepthPixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent;
                casc.FBO.DepthInternalFormat = OpenTK.Graphics.OpenGL4.PixelInternalFormat.DepthComponent32f;

                Cascades.Add(start, casc);
            }

            Game.OnUpdate += GLThread_OnUpdate;
        }

        private void GLThread_OnUpdate(object sender, System.EventArgs e)
        {
            // 252 251 162
            foreach(var c in Cascades)
            {
                var dir = Orientation.ToDirection();
                var up = Orientation.GetTangent(MathExtensions.TangentDirection.Up);
                var right = Orientation.GetTangent(MathExtensions.TangentDirection.Right);
                var newlocationAbstract = Camera.MainDisplayCamera.GetPosition() - dir * 1300;
                var newlocationCamera = Camera.MainDisplayCamera.GetPosition();
                float heightPrcentage = -dir.Y;
                if(heightPrcentage >= 0)
                {
                    var color = Vector3.Lerp(new Vector3(0.988f, 0.924f, 0.63f), new Vector3(1, 1, 1), heightPrcentage);
                    c.Value.LightColor = new Vector3(color);
                }
                else
                {
                    // moon
                    c.Value.LightColor = new Vector3(0.86f, 0.86f, 1);
                    dir.Xz = dir.Zx;
                    newlocationAbstract = Camera.MainDisplayCamera.GetPosition() + dir * 1300;
                    newlocationCamera = Camera.MainDisplayCamera.GetPosition();
                }
                //newlocationCamera.Y = 20;
                //newlocationCamera.Z *= -1;
                //newlocationCamera -= c.Key * (Orientation.GetTangent(MathExtensions.TangentDirection.Up));
                //newlocationCamera += c.Key * right;
                // newlocationCamera += c.Key * up;
                c.Value.camera.SetPosition(newlocationAbstract);
                c.Value.camera.LookAt(newlocationCamera);
            }
        }

        private float max(float a, float b)
        {
            return (float)System.Math.Max(a, b);
        }

        private float min(float a, float b)
        {
            return (float)System.Math.Min(a, b);
        }
    }
}