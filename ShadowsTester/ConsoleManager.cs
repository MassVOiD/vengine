using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEngine.GameConsole;

namespace ShadowsTester
{
    class ConsoleManager
    {
        private GameConsole Console;
        private VEngine.UI.Rectangle Background;
        public ConsoleManager()
        {
            Console = new GameConsole(12);
            //Background =  new VEngine.UI.Rectangle(new OpenTK.Vector2(0.0f, 0.0f), new OpenTK.Vector2(1.0f, 0.4f), Color.FromArgb(127, 0, 0, 0));
           // VEngine.World.Root.UI.Elements.Add(Background);
        }

        public void PrintLine(string message)
        {
            Console.AddMessage(new Message(new List<Token> { new Token(message, new System.Drawing.Font("Segoe UI", 16), Color.White) }));
            Console.RefreshDisplay(new OpenTK.Vector2(0.01f, 0.01f));
        }


    }
}
