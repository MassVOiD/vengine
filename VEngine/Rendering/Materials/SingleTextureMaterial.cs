using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class SingleTextureMaterial : AbsMaterial
    {
        public Texture Tex;

        public SingleTextureMaterial(Texture tex, Texture normalMap = null)
        {
            if(Program == null)
                Program = ShaderProgram.Compile("Generic.vertex.glsl",
                    "SingleTextureMaterial.fragment.glsl");
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


        public bool Use()
        {
            bool res = base.Use();
            Tex.Use(TextureUnit.Texture0);

            return res;
        }
    }
}