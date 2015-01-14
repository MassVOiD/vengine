using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Threading.Tasks;

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
            World.Root = new World();

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

        void PhysicsThread()
        {
            while (true)
                World.Root.UpdatePhysics((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f);
        }

        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (Camera.Current != null)
            {
                var p = this.PointToScreen(new System.Drawing.Point(Width / 2, Height / 2));
                var p2 = this.PointToScreen(new System.Drawing.Point(e.X, e.Y));
                Camera.Current.ProcessMouseMovement(p2.X - p.X, p2.Y - p.Y);
                System.Windows.Forms.Cursor.Position = p;
            }
        }

        protected override void OnLoad(System.EventArgs e)
        {
            VSync = VSyncMode.On;

            GL.ClearColor(0.37f, 0.37f, 0.37f, 1.0f);
            var s = GL.GetString(StringName.Version);
            Console.WriteLine(s);

        }

        public void StartPhysicsThread()
        {
            Task.Factory.StartNew(PhysicsThread);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //GL.Viewport(0, 0, Width, Height);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GLThread.InvokeQueue();
            GLThread.InvokeOnBeforeDraw();
            World.Root.Draw();
            GLThread.InvokeOnAfterDraw();
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f));
            GLThread.InvokeOnUpdate();
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (Camera.Current != null) Camera.Current.ProcessKeyboardState(keyboard);
            if (keyboard[OpenTK.Input.Key.Escape])
            {
                World.Root.DisposePhysics();
                Exit();
            }
        }
    }
}