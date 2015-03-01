using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class SkyboxMaterial : IMaterial
    {
        private static ShaderProgram Program = ShaderProgram.Compile(Media.ReadAllText("Skybox.vertex.glsl"), Media.ReadAllText("Skybox.fragment.glsl"));
        
        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public bool Use()
        {
            return Program.Use();
        }
    }
}