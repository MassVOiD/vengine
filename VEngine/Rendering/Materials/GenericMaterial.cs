using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class GenericMaterial : AbsMaterial
    {
        public Texture Tex;
        public Vector4 Color;
        public enum DrawMode
        {
            TextureOnly,
            ColorOnly,
            TextureMultipleColor,
            OneMinusColorOverTexture
        }
        public DrawMode Mode;

        void CompileShaders()
        {
            if(Program == null)
                Program = ShaderProgram.Compile("Generic.vertex.glsl",
                    "Generic.fragment.glsl");
            if(TesselatedProgram == null)
                TesselatedProgram = ShaderProgram.Compile("Tesselation.vertex.glsl",
                    "Generic.fragment.glsl", null, "Generic.tesscontrol.glsl", "Generic.tesseval.glsl");
        }

        public GenericMaterial(Texture tex, Texture normalMap = null)
        {
            CompileShaders();
            Tex = tex;
            NormalMap = normalMap;
            Color = Vector4.One;
            Mode = GenericMaterial.DrawMode.TextureOnly;
        }
        public GenericMaterial(Vector4 color)
        {
            CompileShaders();
            Color = color;
            Mode = GenericMaterial.DrawMode.ColorOnly;
        }
        public GenericMaterial(Color color)
        {
            CompileShaders();
            Color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            Mode = GenericMaterial.DrawMode.ColorOnly;
        }

        public static GenericMaterial FromMedia(string key)
        {
            return new GenericMaterial(new Texture(Media.Get(key)));
        }

        public static GenericMaterial FromMedia(string key, string normalmap_key)
        {
            return new GenericMaterial(new Texture(Media.Get(key)), new Texture(Media.Get(normalmap_key)));
        }


        public override bool Use()
        {
            bool res = base.Use();

            if(Tex != null)
                Tex.Use(TextureUnit.Texture0);

            GetShaderProgram().SetUniform("input_Color", Color);
            GetShaderProgram().SetUniform("DrawMode", (int)Mode);

            return res;
        }
    }
}