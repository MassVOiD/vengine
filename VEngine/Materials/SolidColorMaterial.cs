using System.Drawing;
using OpenTK.Graphics;

namespace VDGTech
{
    public class SolidColorMaterial : IMaterial
    {
        public SolidColorMaterial(Color color)
        {
            if(Program == null)
                Program = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"), Media.ReadAllText("SolidColorMaterial.fragment.glsl"));
            Colour = color;
        }

        protected static ShaderProgram Program;
        private Color4 Colour;

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public bool Use()
        {
            bool res = Program.Use();
            if(!ShaderProgram.Lock)
                Program.SetUniform("input_Color", Colour);
            return res;
        }
    }
}