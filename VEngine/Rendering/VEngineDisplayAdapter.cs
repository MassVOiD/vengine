using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
namespace VEngine
{
    public abstract class AbsDisplayAdapter : GameWindow
    {
        public PostProcessing PostProcessor;
        int Width, Height;

        public AbsDisplayAdapter(string title, int width, int height)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(8, 0, 0, 0), title, GameWindowFlags.Default,
                DisplayDevice.Default, 4, 3,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            Width = width;
            Height = height;
        }

        protected void Initialize()
        {
            GLThread.DisplayAdapter = this;
            GLThread.StartTime = DateTime.Now;
            GLThread.Resolution = new Vector2(Width, Height);

            PostProcessor = new PostProcessing(Width, Height);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

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