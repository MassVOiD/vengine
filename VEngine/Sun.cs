using System.Drawing;
using OpenTK;

namespace VEngine
{
    public class Sun
    {
        public Sun(Vector3 positionFromZero, Color color, float brightness)
        {
            NormalizedPosition = positionFromZero.Normalized();
            LightColor = color;
            Brightness = brightness;
            if(Current == null)
                Current = this;
        }

        public static Sun Current;
        private float Brightness;
        private Color LightColor;
        private Vector3 NormalizedPosition;

        public void BindToShader(ShaderProgram shader)
        {
            shader.SetUniform("input_SunPosition", NormalizedPosition);
            shader.SetUniform("input_SunLightColor", LightColor);
            shader.SetUniform("input_SunBrightness", Brightness);
        }
    }
}