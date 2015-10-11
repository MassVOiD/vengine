using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using VEngine.UI;

namespace VEngine.GameConsole
{
    public class GameConsole
    {
        private int Limit = 7;
        private List<Message> Messages;

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
            for(int i = 0; i < Messages.Count; i++)
            {
                Messages[i].ShowAt(cursor);
                cursor.Y += Messages[i].LastMaxMeasure;
            }
        }
    }

    public class Message
    {
        public float LastMaxMeasure = 0.01f;
        private List<Text> Elements;
        private List<Token> Tokens;

        public Message(List<Token> tokens)
        {
            Tokens = tokens;
        }

        public void Hide()
        {
            if(Elements != null)
                foreach(var e in Elements)
                    World.Root.UI.Elements.Remove(e);
            Elements = new List<Text>();
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
    }

    public class Token
    {
        public string Content;

        // public Texture Tex;
        public SizeF Size;

        public Color TextColor;
        public Font Typeface;

        public Token(string content, Font typeface, Color color)
        {
            Content = content;
            Typeface = typeface;
            TextColor = color;
            // Tex = Texture.FromText(Content, Typeface.Name, Typeface.Size, TextColor, Color.Transparent);
            Size = Texture.MeasureText(Content, Typeface.Name, Typeface.Size);
        }
    }
}