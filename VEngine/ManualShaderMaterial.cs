using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class ManualShaderMaterial : IMaterial
    {
        protected static ShaderProgram Program;

        public ManualShaderMaterial(string vertexShader, string fragmentShader)
        {
            Program = new ShaderProgram(vertexShader, fragmentShader);
        }
        public ManualShaderMaterial()
        {
        }
        
        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public virtual void Use()
        {
            Program.Use();
        }
    }
}