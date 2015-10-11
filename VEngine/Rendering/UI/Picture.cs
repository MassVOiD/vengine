using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine.UI
{
    public class Picture : AbsUIElement
    {
        static public GenericMaterial Program = new GenericMaterial(Color.White);
        public float Alpha;
        public Texture Tex;

        public Picture(float x, float y, float width, float height, Texture tex, float alpha)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Tex = tex;
            Alpha = alpha;
        }

        public Picture(Vector2 pos, Vector2 size, Texture tex, float alpha)
        {
            Position = pos;
            Size = size;
            Tex = tex;
            Alpha = alpha;
        }

        public override void Draw()
        {
            Program.Use();
            Program.GetShaderProgram().SetUniform("Position", Position);
            Program.GetShaderProgram().SetUniform("Size", Size);
            Program.GetShaderProgram().SetUniform("Alpha", Alpha);
            Tex.Use(TextureUnit.Texture0);
            Info3d.Draw();
        }

        public void Update(float x, float y, float width, float height, Texture tex, float alpha)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Tex = tex;
            Alpha = alpha;
        }

        public void Update(Vector2 pos, Vector2 size, Texture tex, float alpha)
        {
            Position = pos;
            Size = size;
            Tex = tex;
            Alpha = alpha;
        }
    }
}