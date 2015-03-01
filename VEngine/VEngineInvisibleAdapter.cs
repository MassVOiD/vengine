using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VDGTech
{
    public class VEngineInvisibleAdapter : GameWindow, IVEngineDisplayAdapter
    {
        public VEngineInvisibleAdapter()
            : base(1, 1,
                new OpenTK.Graphics.GraphicsMode(32, 24, 8, 4), "INVISIBLE", GameWindowFlags.Default,
                DisplayDevice.Default, 4, 3,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, message);
            }, (IntPtr)0);
        }
    }
}