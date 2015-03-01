using OpenTK.Graphics;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class ManualShaderMaterial : IMaterial
    {
        protected ShaderProgram Program;
        protected Texture Tex = null;
        protected Color Color;

        public ManualShaderMaterial(string vertex, string fragment, string geometry = null, string tesscontrol = null, string tesseval = null)
        {
            Program = ShaderProgram.Compile(vertex, fragment, geometry, tesscontrol, tesseval);
        }

        public void SetTexture(Texture tex)
        {
            Tex = tex;
        }

        public void SetColor(Color color)
        {
            Color = color;
        }

        public static ManualShaderMaterial FromName(string name)
        {
            return new ManualShaderMaterial(Media.ReadAllText(name + ".vertex.glsl"), Media.ReadAllText(name + ".fragment.glsl"));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public virtual bool Use()
        {
            bool res = Program.Use();
            if(Tex != null)
            {
                Program.SetUniform("UseNormalMap", 0);
                Tex.Use(TextureUnit.Texture0);
            }
            if(Color != null)
            {
                Program.SetUniform("input_Color", Color);
            }
            return res;
        }
    }
}