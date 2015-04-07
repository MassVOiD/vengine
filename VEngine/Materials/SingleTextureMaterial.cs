using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class SingleTextureMaterial : IMaterial
    {
        public SingleTextureMaterial(Texture tex, Texture normalMap = null)
        {
            if(Program == null)
                Program = ShaderProgram.Compile(Media.ReadAllText("Generic.vertex.glsl"),
                    Media.ReadAllText("SingleTextureMaterial.fragment.glsl"));
            Tex = tex;
            NormalMap = normalMap;
        }

        protected static ShaderProgram Program;
        private Texture Tex, NormalMap, BumpMap;
        public float NormalMapScale = 1.0f;

        public static SingleTextureMaterial FromMedia(string key)
        {
            return new SingleTextureMaterial(new Texture(Media.Get(key)));
        }

        public static SingleTextureMaterial FromMedia(string key, string normalmap_key)
        {
            return new SingleTextureMaterial(new Texture(Media.Get(key)), new Texture(Media.Get(normalmap_key)));
        }

        public void SetBumpMapFromMedia(string bumpmapKey)
        {
            BumpMap = new Texture(Media.Get(bumpmapKey));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public bool Use()
        {
            bool res = Program.Use();
            Tex.Use(TextureUnit.Texture0);
            if(NormalMap != null)
            {
                Program.SetUniform("UseNormalMap", 1);
                Program.SetUniform("NormalMapScale", NormalMapScale);
                NormalMap.Use(TextureUnit.Texture1);
            }
            else
                Program.SetUniform("UseNormalMap", 0);

            if(BumpMap != null)
            {
                Program.SetUniform("UseBumpMap", 1);
                BumpMap.Use(TextureUnit.Texture16);
            }
            else
                Program.SetUniform("UseBumpMap", 0);
            return res;
        }
    }
}