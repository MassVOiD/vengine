namespace VDGTech
{
    public interface IMaterial
    {
        ShaderProgram GetShaderProgram();

        void Use();
    }
}