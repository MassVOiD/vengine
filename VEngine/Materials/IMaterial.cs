namespace VDGTech
{
    public interface IMaterial
    {
        ShaderProgram GetShaderProgram();

        bool Use();
    }
}