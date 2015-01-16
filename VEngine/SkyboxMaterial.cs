using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class SkyboxMaterial : IMaterial
    {
        private static ShaderProgram Program = new ShaderProgram(Media.ReadAllText("Skybox.vertex.glsl"), Media.ReadAllText("Skybox.fragment.glsl"));
        
        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public void Use()
        {
            Program.Use();
        }
    }
}