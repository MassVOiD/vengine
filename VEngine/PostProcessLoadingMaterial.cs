namespace VDGTech
{
    public class PostProcessLoadingMaterial : IMaterial
    {
        private ShaderProgram Program;

        public PostProcessLoadingMaterial()
        {
            Program = new ShaderProgram(Media.ReadAllText("Loading.vertex.glsl"), Media.ReadAllText("Loading.fragment.glsl"));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public void Use()
        {
            Program.Use();
            Program.SetUniform("resolution", GLThread.Resolution);
        }
    }
}