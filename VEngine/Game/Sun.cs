using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;

namespace VEngine
{
    public class Sun
    {
        Dictionary<float, ProjectionLight> Cascades;
        public Quaternion Orientation;
        public Vector4 LightColor;
        public static Sun Current;

        public Sun(Quaternion orientation, Vector4 color, params float[] levels)
        {
            Current = this;
            LightColor = color;
            Orientation = orientation;
            Cascades = new Dictionary<float, ProjectionLight>();

            for(int i = 0; i < levels.Length; i++)
            {
                float start = levels[i];
                //float end = levels[i + 1];
                var casc = new ProjectionLight(Camera.MainDisplayCamera.GetPosition(), orientation, 2048, 2048, (MathHelper.PiOver2 - 0.1f) / levels[i], 0.01f, 10000.0f);
                casc.LightColor = color;

                casc.FBO.DepthPixelType = OpenTK.Graphics.OpenGL4.PixelType.Float;
                casc.FBO.DepthPixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent;
                casc.FBO.DepthInternalFormat = OpenTK.Graphics.OpenGL4.PixelInternalFormat.DepthComponent32f;

                Cascades.Add(start, casc);

                casc.LightMixRange.Start = start;
                casc.LightMixRange.End = start;
                casc.LightMixMode = LightMixMode.SunCascade;
                LightPool.Add(casc);
            }


            GLThread.OnUpdate += GLThread_OnUpdate;
        }

        float max(float a, float b)
        {
            return (float)System.Math.Max(a, b);
        }
        float min(float a, float b)
        {
            return (float)System.Math.Min(a, b);
        }

        void GLThread_OnUpdate(object sender, System.EventArgs e)
        {
            foreach(var c in Cascades)
            {
                var dir = Orientation.ToDirection();
                var up = Orientation.GetTangent(MathExtensions.TangentDirection.Up);
                var right = Orientation.GetTangent(MathExtensions.TangentDirection.Right);
                var newlocationAbstract = Camera.MainDisplayCamera.GetPosition() - dir * 1300;
                var newlocationCamera = Camera.MainDisplayCamera.GetPosition();
                //newlocationCamera.Y = 20;
                //newlocationCamera.Z *= -1;
                //newlocationCamera -= c.Key * (Orientation.GetTangent(MathExtensions.TangentDirection.Up));
                //newlocationCamera += c.Key * right;
               // newlocationCamera += c.Key * up;
                c.Value.camera.SetPosition(newlocationAbstract);
                c.Value.camera.LookAt(newlocationCamera);
            }
        }


    }
}