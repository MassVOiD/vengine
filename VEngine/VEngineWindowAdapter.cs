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

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, message);
            }, (IntPtr)0);
            MouseMove += Mouse_Move;
            CursorVisible = false;
        }

        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            var p = this.PointToScreen(new System.Drawing.Point(Width / 2, Height / 2));
            var p2 = this.PointToScreen(new System.Drawing.Point(e.X, e.Y));
            if (Camera.Current != null && p2.X != p.X && p2.Y != p.Y)
            {
                Camera.Current.ProcessMouseMovement(e.XDelta, e.YDelta);
                if (Math.Abs(p.X - p2.X) > 10 || Math.Abs(p.Y - p2.Y) > 10) System.Windows.Forms.Cursor.Position = p;
            }
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

            GLThread.InvokeQueue();
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }
            GLThread.InvokeOnBeforeDraw();
            SceneNode.Root.Draw(Matrix4.Identity);
            GLThread.InvokeOnAfterDraw();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f));
            GLThread.InvokeOnUpdate();
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (Camera.Current != null) Camera.Current.ProcessKeyboardState(keyboard);
            if (keyboard[OpenTK.Input.Key.Escape])
                Exit();
        }
    }
}