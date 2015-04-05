using System.Drawing;
using OpenTK.Graphics;
using OpenTK;

namespace VDGTech
{
    public class SolidColorMaterial : IMaterial
    {
        public SolidColorMaterial(Color color)
        {
            if(Program == null)
                Program = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"), Media.ReadAllText("SolidColorMaterial.fragment.glsl"));
            Color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        protected static ShaderProgram Program;
        public Vector4 Color;

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public bool Use()
        {
            bool res = Program.Use();
            if(!ShaderProgram.Lock)
                Program.SetUniform("input_Color", Color);
            return res;
        }
    }
}