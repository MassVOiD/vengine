using OpenTK.Graphics;
using System.Drawing;

namespace VDGTech
{
    public class ManualShaderMaterial : IMaterial
    {
        protected ShaderProgram Program;

        public ManualShaderMaterial(string vertexShader, string fragmentShader)
        {
            Program = new ShaderProgram(vertexShader, fragmentShader);
        }
        public ManualShaderMaterial()
        {
        }

        public static ManualShaderMaterial FromName(string name)
        {
            return new ManualShaderMaterial(Media.ReadAllText(name + ".vertex.glsl"), Media.ReadAllText(name + ".fragment.glsl"));
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