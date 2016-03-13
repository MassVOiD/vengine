using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    public class CascadeShadows
    {
        ShadowMapsArrayTexture MapsArray;

        ProjectionLight[] Mappers;

        public Vector3 Color;

        private Vector3 Direction;

        private volatile bool Ready = false;

        public CascadeShadows(Vector3 color, float[] sizes, float length)
        {
            MapsArray = new ShadowMapsArrayTexture(2048, 2048, false);
            Mappers = new ProjectionLight[sizes.Length];
            float halflength = length * 0.5f;
            Quaternion minusUnitY = Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(-90));
            for(int i = 0; i < sizes.Length; i++)
            {
                Mappers[i] = new ProjectionLight(Vector3.One, minusUnitY, 1, 1, 0.1f, 0.01f, 100.0f);
                float halfsize = sizes[i] * 0.5f;
                Mappers[i].camera.UpdatePerspectiveOrtho(-halfsize, halfsize, -halfsize, halfsize, halflength, -halflength);
            }
            Game.Invoke(() =>
            {
                MapsArray.UpdateFromLightsList(Mappers);
                Ready = true;

            });

            Color = color;
            Direction = -1 * (minusUnitY.ToDirection());

            Game.OnBeforeDraw += OnBeforeDraw;
        }

        public void SetDirection(Quaternion direction)
        {
            for(int i = 0; i < Mappers.Length; i++)
            {
                Mappers[i].camera.SetOrientation(direction);
                Mappers[i].camera.Update();
            }
            Direction = -1 * (direction.ToDirection());
        }

        public Quaternion GetDirection()
        {
            return Mappers[0].camera.GetOrientation();
        }

        private void OnBeforeDraw(object sender, FrameEventArgs e)
        {
            for(int i = 0; i < Mappers.Length; i++)
            {
                var vec = Camera.MainDisplayCamera.GetPosition();
                vec.Y = 0;
                Mappers[i].camera.SetPosition(vec);
                Mappers[i].camera.Update();
                Mappers[i].Map();
            }
        }

        public void SetUniforms()
        {
            Matrix4[] matricesP = new Matrix4[Mappers.Length];
            Matrix4[] matricesV = new Matrix4[Mappers.Length];

            for(int i = 0; i < Mappers.Length; i++)
            {
                matricesP[i] = Mappers[i].GetPMatrix();
                matricesV[i] = Mappers[i].GetVMatrix();
            }
            ShaderProgram.Current.SetUniform("SunColor", Color);
            ShaderProgram.Current.SetUniform("SunCascadeCount", Ready ? Mappers.Length : 0);
            ShaderProgram.Current.SetUniform("SunDirection", Direction);
            ShaderProgram.Current.SetUniformArray("SunMatricesP", matricesP);
            ShaderProgram.Current.SetUniformArray("SunMatricesV", matricesV);

            MapsArray.Bind(22);
        }
    }
}
