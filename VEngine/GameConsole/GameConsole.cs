using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using VDGTech.UI;

namespace VDGTech.GameConsole
{
    public class Token
    {
        public string Content;
        public Font Typeface;
        public Color TextColor;
        //  public Texture Tex;
        public SizeF Size;
        public Token(string content, Font typeface, Color color)
        {
            Content = content;
            Typeface = typeface;
            TextColor = color;
            //  Tex = Texture.FromText(Content, Typeface.Name, Typeface.Size, TextColor, Color.Transparent);
            Size = Texture.MeasureText(Content, Typeface.Name, Typeface.Size);
        }
    }
    public class Message
    {
        List<Token> Tokens;
        List<Text> Elements;
        public float LastMaxMeasure = 0.01f;
        public Message(List<Token> tokens)
        {
            Tokens = tokens;
        }
        public void ShowAt(Vector2 UV)
        {
            var cursor = UV;
            if(Elements != null)
                foreach(var e in Elements)
                    World.Root.UI.Elements.Remove(e);

            Elements = new List<Text>();
            foreach(var t in Tokens)
            {
                Text text = new Text(cursor, t.Content, t.Typeface.Name, t.Typeface.Size, t.TextColor);
                Elements.Add(text);
                World.Root.UI.Elements.Add(text);
                cursor.X += text.Size.X;
                LastMaxMeasure = Math.Max(LastMaxMeasure, text.Size.Y);
            }
        }
        public void Hide()
        {
            if(Elements != null)
                foreach(var e in Elements)
                    World.Root.UI.Elements.Remove(e);
            Elements = new List<Text>();
        }
    }
    public class GameConsole
    {
        List<Message> Messages;
        int Limit = 7;

        public GameConsole(int limit)
        {
            Messages = new List<Message>();
            Limit = limit;
        }

        public void AddMessage(Message msg)
        {
            Messages.Add(msg);
            if(Messages.Count > Limit)
            {
                Messages[0].Hide();
                Messages = Messages.Skip(1).ToList();
            }
        }

        public void Clear()
        {
            Messages.Clear();
        }

        public void RefreshDisplay(Vector2 leftup)
        {
            var cursor = leftup;
            for(int i=0;i<Messages.Count;i++)
            {
                Messages[i].ShowAt(cursor);
                cursor.Y += Messages[i].LastMaxMeasure;
            }
        }
    }
}
