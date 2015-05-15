using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
namespace VEngine.UI
{
    public class Rectangle : AbsUIElement
    {
        static public ManualShaderMaterial Program = ManualShaderMaterial.FromMedia("UI.vertex.glsl", "UI.Rectangle.fragment.glsl");
        public Color Color;

        public Rectangle(float x, float y, float width, float height, Color color)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Color = color;
        }

        public Rectangle(Vector2 pos, Vector2 size, Color color)
        {
            Position = pos;
            Size = size;
            Color = color;
        }

        public void Update(float x, float y, float width, float height, Color color)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);
            Color = color;
        }

        public void Update(Vector2 pos, Vector2 size, Color color)
        {
            Position = pos;
            Size = size;
            Color = color;
        }

        public override void Draw()
        {
            Program.Use();
            Program.GetShaderProgram().SetUniform("Position", Position);
            Program.GetShaderProgram().SetUniform("Size", Size);
            Program.GetShaderProgram().SetUniform("Color", Color);
            Info3d.Draw();
        }

    }
}