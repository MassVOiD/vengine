using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public abstract class AbsDisplayAdapter : GameWindow
    {
        public DeferredPipeline Pipeline;

        public AbsDisplayAdapter(string title, int width, int height, GameWindowFlags flags)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(8, 0, 0, 0), title, flags,
                DisplayDevice.Default, 4, 4,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            GLThread.DisplayAdapter = this;
            GLThread.Resolution = new Size(Width, Height);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", source, type, id, severity, length, message);
            }, (IntPtr)0);

            MouseMove += Mouse_Move;
            KeyPress += VEngineWindowAdapter_KeyPress;
            KeyDown += VEngineWindowAdapter_KeyDown;
            KeyUp += VEngineWindowAdapter_KeyUp;
            MouseDown += VEngineWindowAdapter_MouseDown;
            MouseUp += VEngineWindowAdapter_MouseUp;
            MouseWheel += VEngineWindowAdapter_MouseWheel;
            Load += VEngineWindowAdapter_Load;
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
        
        protected override void OnLoad(System.EventArgs e)
        {
            VSync = VSyncMode.Off;

            var s = GL.GetString(StringName.Version);
            Console.WriteLine(s);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Interpolator.StepAll();

            GLThread.InvokeQueue();

            World.Root.RootScene.MapLights(Matrix4.Identity);

            //LightPool.UseTextures(2);
            // this is here so you can issue draw calls from there if you want
            GLThread.InvokeOnBeforeDraw();
            Pipeline.PostProcessor.ExecutePostProcessing();
            //DrawAll();
            GLThread.InvokeOnAfterDraw();
            
            GLThread.CheckErrors();

            SwapBuffers();
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f));
            GLThread.InvokeOnUpdate();
            TransformationJoint.Resolve();
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