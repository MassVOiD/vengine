using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class SolidColorMaterial : IMaterial
    {
        private Color4 Colour;
        private ShaderProgram Program;

        public SolidColorMaterial(Color color)
        {
            Colour = color;
            Program = new ShaderProgram(Media.ReadAllText("SolidColorMaterial.vertex.glsl"), Media.ReadAllText("SolidColorMaterial.fragment.glsl"));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public void Use()
        {
            Program.Use();
            Program.SetUniform("input_Color", Colour);
        }
    }
}