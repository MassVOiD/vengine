using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace VDGTech
{
    public class SingleTextureMaterial : IMaterial
    {
        protected static ShaderProgram Program;
        private Texture Tex, NormalMap;

        public SingleTextureMaterial(Texture tex, Texture normalMap = null)
        {
            Program = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"),
                Media.ReadAllText("SingleTextureMaterial.fragment.glsl"));
            Tex = tex;
            NormalMap = normalMap;
        }

        public static SingleTextureMaterial FromMedia(string key)
        {
            return new SingleTextureMaterial(new Texture(Media.Get(key)));
        }

        public static SingleTextureMaterial FromMedia(string key, string normalmap_key)
        {
            return new SingleTextureMaterial(new Texture(Media.Get(key)), new Texture(Media.Get(normalmap_key)));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public void Use()
        {
            Program.Use();
            Tex.Use(TextureUnit.Texture0);
            if(NormalMap != null)
            {
                Program.SetUniform("UseNormalMap", 1);
                NormalMap.Use(TextureUnit.Texture1);
            } else Program.SetUniform("UseNormalMap", 0);
        }
    }
}