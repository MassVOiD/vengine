using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;

namespace VDGTech
{
    public class VEngineWindowAdapter : GameWindow, IVEngineDisplayAdapter
    {
        public VEngineWindowAdapter(string title, int width, int height)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(32, 24, 8, 4), title, GameWindowFlags.Default,
                DisplayDevice.Default, 4, 3,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            GLThread.StartTime = DateTime.Now;
            SceneNode.Root = new SceneNode();

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, message);
            }, (IntPtr)0);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            VSync = VSyncMode.On;

            GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);
            var s = GL.GetString(StringName.Version);
            Console.WriteLine(s);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.DepthTest);
            GLThread.InvokeQueue();
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }
            SceneNode.Root.Draw(Matrix4.Identity);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f));
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.Escape])
                Exit();
        }
    }
}