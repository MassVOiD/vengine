using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class SolidColorMaterial : IMaterial
    {
        private Color4 Colour;
        private static ShaderProgram Program = new ShaderProgram(Media.ReadAllText("SolidColorMaterial.vertex.glsl"), Media.ReadAllText("SolidColorMaterial.fragment.glsl"));

        public SolidColorMaterial(Color color)
        {
            Colour = color;
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