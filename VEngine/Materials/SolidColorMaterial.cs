using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class SolidColorMaterial : IMaterial
    {
        protected ShaderProgram Program;
        private Color4 Colour;

        public SolidColorMaterial(Color color)
        {
            Program = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"), Media.ReadAllText("SolidColorMaterial.fragment.glsl"));
            Colour = color;
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public void Use()
        {
            Program.Use();
            if(!ShaderProgram.Lock) Program.SetUniform("input_Color", Colour);
        }
    }
}