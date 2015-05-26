using OpenTK;
namespace VEngine
{
    public class PostProcessLoadingMaterial : IMaterial
    {
        public PostProcessLoadingMaterial()
        {
            Program = ShaderProgram.Compile("PostProcess.vertex.glsl", "Loading.fragment.glsl");
        }

        private ShaderProgram Program;

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public bool Use()
        {
            bool res = Program.Use();
            Program.SetUniform("resolution", new Vector2(GLThread.Resolution.Width, GLThread.Resolution.Height));
            return res;
        }
    }
}