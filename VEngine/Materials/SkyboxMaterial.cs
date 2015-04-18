namespace VDGTech
{
    public class SkyboxMaterial : IMaterial
    {
        private static ShaderProgram Program = ShaderProgram.Compile("Skybox.vertex.glsl", "Skybox.fragment.glsl");

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