namespace VEngine
{
    public interface IMaterial
    {
        ShaderProgram GetShaderProgram();

        bool Use();
    }
}