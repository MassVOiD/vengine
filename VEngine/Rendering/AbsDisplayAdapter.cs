using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
namespace VEngine
{
    public abstract class AbsDisplayAdapter : GameWindow
    {
        public DeferredPipeline Pipeline;
        public AbsDisplayAdapter(string title, int width, int height)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(8, 0, 0, 0), title, GameWindowFlags.Default,
                DisplayDevice.Default, 4, 3,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            GLThread.DisplayAdapter = this;
            GLThread.Resolution = new Size(Width, Height);
            Pipeline = new DeferredPipeline();
        }

        public bool IsCursorVisible
        {
            get
            {
                return CursorVisible;
            }
            set
            {
                CursorVisible = value;
            }
        }

    }
}