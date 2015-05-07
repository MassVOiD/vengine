using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using VEngine.Particles;

namespace VEngine
{
    public class VEngineWindowAdapter : AbsDisplayAdapter
    {
        public VEngineWindowAdapter(string title, int width, int height)
            : base(title, width, height)
        {
            Initialize();
            var settings = new GraphicsSettings();
            new SettingsWindow(settings).Show();

            /*GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, message);
            }, (IntPtr)0);*/
            MouseMove += Mouse_Move;
            KeyPress += VEngineWindowAdapter_KeyPress;
            KeyDown += VEngineWindowAdapter_KeyDown;
            KeyUp += VEngineWindowAdapter_KeyUp;
            MouseDown += VEngineWindowAdapter_MouseDown;
            MouseUp += VEngineWindowAdapter_MouseUp;
            MouseWheel += VEngineWindowAdapter_MouseWheel;
            Load += VEngineWindowAdapter_Load;
            CursorVisible = false;
        }



        public void StartPhysicsThread()
        {
            Task.Factory.StartNew(PhysicsThread);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            VSync = VSyncMode.Off;

            GL.ClearColor(0.37f, 0.37f, 0.37f, 1.0f);
            var s = GL.GetString(StringName.Version);
            Console.WriteLine(s);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Interpolator.StepAll();

            GLThread.InvokeQueue();

            LightPool.MapAll();

            //LightPool.UseTextures(2);
            // this is here so you can issue draw calls from there if you want
            GLThread.InvokeOnBeforeDraw();
            PostProcessor.ExecutePostProcessing();
            //DrawAll();
            GLThread.InvokeOnAfterDraw();
            
            World.Root.UI.DrawAll();

            GLThread.CheckErrors();

            World.Root.ShouldUpdatePhysics = true;

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f));
            GLThread.InvokeOnUpdate();
            MeshLinker.Resolve();
            //Debugger.Send("FrameTime", e.Time);
            //Debugger.Send("FPS", 1.0 / e.Time);
            var keyboard = OpenTK.Input.Keyboard.GetState();
            //if (Camera.Current != null) Camera.Current.ProcessKeyboardState(keyboard);
            if(keyboard[OpenTK.Input.Key.Escape])
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                // this disposing causes more errors than normal killing
                //World.Root.DisposePhysics();
                //Exit();
            }
        }

        private void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if(Camera.Current != null)
            {
                if(!CursorVisible)
                {
                    var p = this.PointToScreen(new System.Drawing.Point(Width / 2, Height / 2));
                    var p2 = this.PointToScreen(new System.Drawing.Point(e.X, e.Y));
                    GLThread.InvokeOnMouseMove(new OpenTK.Input.MouseMoveEventArgs(e.X, e.Y, p2.X - p.X, p2.Y - p.Y));
                    System.Windows.Forms.Cursor.Position = p;
                }
                else
                {
                    GLThread.InvokeOnMouseMove(e);
                }
            }
        }

        private void PhysicsThread()
        {
            GLThread.SetCurrentThreadCores(3);
            long time = Stopwatch.GetTimestamp();
            while(true)
            {
                try
                {
                    var now = Stopwatch.GetTimestamp();
                    World.Root.UpdatePhysics((now - time) / (float)Stopwatch.Frequency);
                    time = now;
                }
                catch
                {
                }
            }
        }

        private void VEngineWindowAdapter_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            GLThread.InvokeOnKeyDown(e);
        }

        private void VEngineWindowAdapter_KeyPress(object sender, KeyPressEventArgs e)
        {
            GLThread.InvokeOnKeyPress(e);
        }

        private void VEngineWindowAdapter_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            GLThread.InvokeOnKeyUp(e);
        }

        private void VEngineWindowAdapter_Load(object sender, EventArgs e)
        {
            GLThread.InvokeOnLoad();
        }

        private void VEngineWindowAdapter_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            GLThread.InvokeOnMouseDown(e);
        }

        private void VEngineWindowAdapter_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            GLThread.InvokeOnMouseUp(e);
        }

        private void VEngineWindowAdapter_MouseWheel(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            GLThread.InvokeOnMouseWheel(e);
        }
    }
}