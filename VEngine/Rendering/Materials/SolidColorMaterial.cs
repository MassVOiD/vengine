using System.Drawing;
using OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class SolidColorMaterial : IMaterial
    {
        public SolidColorMaterial(Color color)
        {
            if(Program == null)
                Program = ShaderProgram.Compile("Generic.vertex.glsl", "SolidColorMaterial.fragment.glsl");
            Color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }
        public SolidColorMaterial(Vector4 color)
        {
            if(Program == null)
                Program = ShaderProgram.Compile("Generic.vertex.glsl", "SolidColorMaterial.fragment.glsl");
            Color = color;
        }

        protected static ShaderProgram Program;
        private Texture NormalMap;
        public Vector4 Color;
        public void SetNormalMapFromMedia(string bumpmapKey)
        {
            NormalMap = new Texture(Media.Get(bumpmapKey));
        }

        public ShaderProgram GetShaderProgram()
        {
            return Program;
        }

        public float NormalMapScale = 1.0f;

        public bool Use()
        {
            bool res = Program.Use();
            if(!ShaderProgram.Lock)
                Program.SetUniform("input_Color", Color);
            if(NormalMap != null)
            {
                Program.SetUniform("UseNormalMap", 1);
                Program.SetUniform("NormalMapScale", NormalMapScale);
                NormalMap.Use(TextureUnit.Texture1);
            }
            else
                Program.SetUniform("UseNormalMap", 0);
            Program.SetUniform("UseBumpMap", 0);
            return res;
        }
    }
}