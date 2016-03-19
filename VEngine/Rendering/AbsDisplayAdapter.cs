using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public abstract class AbsDisplayAdapter : GameWindow
    {
        public Renderer MainRenderer;
        private volatile bool PhysicsNeedsUpdate = false;

        public AbsDisplayAdapter(string title, int width, int height, GameWindowFlags flags)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(new ColorFormat(8, 8, 8, 8), 8, 0, 1), title, flags,
                DisplayDevice.Default, 4, 5,
                GraphicsContextFlags.Debug)
        {
            Game.DisplayAdapter = this;
            Game.Resolution = new Size(Width, Height);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.Enable(EnableCap.Dither);
            GL.Enable(EnableCap.Multisample);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
           // GL.ClipControl(ClipOrigin.LowerLeft, ClipDepthMode.ZeroToOne);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.ClearColor(0, 0, 0, 0);
            GL.ClearDepth(1);

            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);

            GL.Disable(EnableCap.Blend);
            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                string msg = Marshal.PtrToStringAnsi(message);
                Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}", source, type, id, severity, length, msg, userParam);
            }, (IntPtr)0);

            MouseMove += Mouse_Move;
            KeyPress += VEngineWindowAdapter_KeyPress;
            KeyDown += VEngineWindowAdapter_KeyDown;
            KeyUp += VEngineWindowAdapter_KeyUp;
            MouseDown += VEngineWindowAdapter_MouseDown;
            MouseUp += VEngineWindowAdapter_MouseUp;
            MouseWheel += VEngineWindowAdapter_MouseWheel;
            Load += VEngineWindowAdapter_Load;
            Resize += VEngineWindowAdapter_Resize;
            Task.Factory.StartNew(() => PhysicsThread());
        }

        private void VEngineWindowAdapter_Resize(object sender, EventArgs e)
        {
            if(Game.DisplayAdapter.MainRenderer != null && Game.Initialized)
            {
                Game.Invoke(() =>
                {
                   // Game.InvokeOnResize(e);
                });
            }
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
            TargetRenderFrequency = 3000;
            TargetUpdateFrequency = 3000;
            this.Context.ErrorChecking = true;
            
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
                if(!PhysicsNeedsUpdate)
                    continue;
                watch.Stop();
                double ms = 1000.0 * (double)watch.ElapsedTicks / Stopwatch.Frequency;
                watch.Reset();
                watch.Start();
                //Game.World.Physics.UpdateAllModifiedTransformations();
                Game.World.Physics.SimulationStep((float)(ms * 0.001f));
                PhysicsNeedsUpdate = false;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if(Game.Resolution.Width != this.Width || Game.Resolution.Height != this.Height)
                Game.InvokeOnResize(e);
            //Interpolator.StepAll();
            //TransformationJoint.Resolve();

            Game.CurrentFrameTime = (float)( e.Time * 1000.0 );

            Game.InvokeQueue();

            Game.World.Scene.MapLights();

            //LightPool.UseTextures(2);
            // this is here so you can issue draw calls from there if you want
            Game.InvokeOnBeforeDraw(e);
            MainRenderer.RenderToFramebuffer(Framebuffer.Default);
            //DrawAll();
            Game.InvokeOnAfterDraw(e);

            //Game.CheckErrors();
            PhysicsNeedsUpdate = true;

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
                    Game.InvokeOnMouseMove(e);
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