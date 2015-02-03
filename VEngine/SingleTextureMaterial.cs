using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace VDGTech
{
    public class SingleTextureMaterial : IMaterial
    {
        protected static ShaderProgram Program;
        private Texture Tex;

        public SingleTextureMaterial(Texture tex)
        {
            Program = new ShaderProgram(Media.ReadAllText("SingleTextureMaterial.vertex.glsl"), Media.ReadAllText("SingleTextureMaterial.fragment.glsl"));
            Tex = tex;
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public void Use()
        {
            Program.Use();
            Tex.Use(TextureUnit.Texture8);
        }
    }
}