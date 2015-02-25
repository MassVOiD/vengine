namespace VDGTech
{
    public class PostProcessLoadingMaterial : IMaterial
    {
        private ShaderProgram Program;

        public PostProcessLoadingMaterial()
        {
            Program = ShaderProgram.Compile(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("Loading.fragment.glsl"));
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