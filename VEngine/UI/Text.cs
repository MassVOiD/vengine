using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech.UI
{
    public class Text : AbsUIElement
    {
        static public ManualShaderMaterial Program = ManualShaderMaterial.FromMedia("UI.vertex.glsl", "UI.Text.fragment.glsl");
        public Texture Tex;

        public Text(float x, float y, string text, string font, float size, Color textColor)
        {
            Tex = Texture.FromText(text, font, size, textColor, Color.Transparent);
            Position = new Vector2(x, y);
            var measured = Texture.MeasureText(text, font, size);
            Size = new Vector2(measured.Width / GLThread.Resolution.X, measured.Height / GLThread.Resolution.Y);
        }
        public Text(Vector2 pos, string text, string font, float size, Color textColor)
        {
            Tex = Texture.FromText(text, font, size, textColor, Color.Transparent);
            Position = pos;
            var measured = Texture.MeasureText(text, font, size);
            Size = new Vector2(measured.Width / GLThread.Resolution.X, measured.Height / GLThread.Resolution.Y);
        }

        public void Update(float x, float y, string text, string font, float size, Color textColor)
        {
            Tex = Texture.FromText(text, font, size, textColor, Color.Transparent);
            var measured = Texture.MeasureText(text, font, size);
            Position = new Vector2(x, y);
            Size = new Vector2(measured.Width / GLThread.Resolution.X, measured.Height / GLThread.Resolution.Y);
        }
        public void Update(Vector2 pos, string text, string font, float size, Color textColor)
        {
            Tex = Texture.FromText(text, font, size, textColor, Color.Transparent);
            var measured = Texture.MeasureText(text, font, size);
            Position = pos;
            Size = new Vector2(measured.Width / GLThread.Resolution.X, measured.Height / GLThread.Resolution.Y);
        }

        public override void Draw()
        {
            Program.Use();
            Program.GetShaderProgram().SetUniform("Position", Position);
            Program.GetShaderProgram().SetUniform("Size", Size);
            Tex.Use(TextureUnit.Texture0);
            Info3d.Draw();
        }

    }
}