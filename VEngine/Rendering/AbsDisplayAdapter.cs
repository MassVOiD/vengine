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
                new OpenTK.Graphics.GraphicsMode(new ColorFormat(8, 8, 8, 8), 8, 0, 1), title, flags,
                DisplayDevice.Default, 4, 5,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            Game.DisplayAdapter = this;
            Game.Resolution = new Size(Width, Height);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.Enable(EnableCap.Dither);
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
            //Task.Factory.StartNew(() => PhysicsThread());
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
            TargetRenderFrequency = 60;
            TargetUpdateFrequency = 60;
            //this.Context.

            var s = GL.GetString(StringName.Version);
            Console.WriteLine(s);
        }

        private void PhysicsThread()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while(true)
            {
                watch.Stop();
                double ms = 1000.0 * (double)watch.ElapsedTicks / Stopwatch.Frequency;
                watch.Reset();
                watch.Start();
                Game.World.Physics.UpdateAllModifiedTransformations();
                Game.World.Physics.SimulationStep((float)(ms * 0.001f));
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Interpolator.StepAll();
            TransformationJoint.Resolve();

            Game.CurrentFPS = (float)( 1.0 / e.Time );

            Game.InvokeQueue();

            Game.World.Scene.MapLights();

            //LightPool.UseTextures(2);
            // this is here so you can issue draw calls from there if you want
            Game.InvokeOnBeforeDraw(e);
            Pipeline.PostProcessor.RenderToFramebuffer(Framebuffer.Default);
            //DrawAll();
            Game.InvokeOnAfterDraw(e);

            //Game.CheckErrors();

            GL.Flush();
            GL.Finish();
            
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - Game.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - Game.StartTime).Milliseconds / 1000.0f * 10.0f));
            Game.InvokeOnUpdate(e);
            //Debugger.Send("FrameTime", e.Time);
            //Debugger.Send("FPS", 1.0 / e.Time);
            var keyboard = OpenTK.Input.Keyboard.GetState();
            //if (Camera.Current != null) Camera.Current.ProcessKeyboardState(keyboard);
            if(keyboard[OpenTK.Input.Key.Escape])
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                // this disposing causes more errors than normal killing
                //Game.World.DisposePhysics();
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
                    Game.InvokeOnMouseMove(new OpenTK.Input.MouseMoveEventArgs(e.X, e.Y, p2.X - p.X, p2.Y - p.Y));
                    System.Windows.Forms.Cursor.Position = p;
                }
                else
                {
                    Game.InvokeOnMouseMove(e);
                }
            }
        }

        private void VEngineWindowAdapter_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            Game.InvokeOnKeyDown(e);
        }

        private void VEngineWindowAdapter_KeyPress(object sender, KeyPressEventArgs e)
        {
            Game.InvokeOnKeyPress(e);
        }

        private void VEngineWindowAdapter_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            Game.InvokeOnKeyUp(e);
        }

        private void VEngineWindowAdapter_Load(object sender, EventArgs e)
        {
            Game.InvokeOnLoad();
        }

        private void VEngineWindowAdapter_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            Game.InvokeOnMouseDown(e);
        }

        private void VEngineWindowAdapter_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            Game.InvokeOnMouseUp(e);
        }

        private void VEngineWindowAdapter_MouseWheel(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            Game.InvokeOnMouseWheel(e);
        }
    }
}