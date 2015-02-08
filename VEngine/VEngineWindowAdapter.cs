using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VDGTech
{
    public class VEngineWindowAdapter : GameWindow, IVEngineDisplayAdapter
    {

        static float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
        static uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

        Framebuffer PostProcessFramebuffer, NormalWritingFramebuffer;
        ShaderProgram NormalWritingShaderProgram;
        IMaterial PostProcessingDefaultMaterial;
        Mesh3d PostProcessingMesh;

        public VEngineWindowAdapter(string title, int width, int height)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(32, 32, 0, 16), title, GameWindowFlags.Default,
                DisplayDevice.Default, 4, 4,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            GLThread.StartTime = DateTime.Now;
            GLThread.Resolution = new Vector2(width, height);

            PostProcessFramebuffer = new Framebuffer(width, height);
            NormalWritingFramebuffer = new Framebuffer(width, height);
            NormalWritingShaderProgram = new ShaderProgram(Media.ReadAllText("WriteNormals.vertex.glsl"), Media.ReadAllText("WriteNormals.fragment.glsl"));
            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
            PostProcessingDefaultMaterial = new ManualShaderMaterial(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("PostProcess.fragment.glsl"));
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, PostProcessingDefaultMaterial);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

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
            CursorVisible = false;
        }

        public void SetCustomPostProcessingMaterial(IMaterial material)
        {
            PostProcessingMesh.Material = material;
        }
        public void SetDefaultPostProcessingMaterial()
        {
            PostProcessingMesh.Material = PostProcessingDefaultMaterial;
        }

        void VEngineWindowAdapter_Load(object sender, EventArgs e)
        {
            GLThread.InvokeOnLoad();
        }

        void VEngineWindowAdapter_MouseWheel(object sender, OpenTK.Input.MouseWheelEventArgs e)
        {
            GLThread.InvokeOnMouseWheel(e);
        }

        void VEngineWindowAdapter_MouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            GLThread.InvokeOnMouseUp(e);
        }

        void VEngineWindowAdapter_MouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            GLThread.InvokeOnMouseDown(e);
        }

        void VEngineWindowAdapter_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            GLThread.InvokeOnKeyUp(e);
        }

        void VEngineWindowAdapter_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            GLThread.InvokeOnKeyDown(e);
        }

        void VEngineWindowAdapter_KeyPress(object sender, KeyPressEventArgs e)
        {
            GLThread.InvokeOnKeyPress(e);
        }

        void PhysicsThread()
        {
            long time = Stopwatch.GetTimestamp();
            while(true)
            {
                var now = Stopwatch.GetTimestamp();
                World.Root.UpdatePhysics((now - time) / (float)Stopwatch.Frequency);
                time = now;
            }
        }

        void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            if (Camera.Current != null)
            {
                var p = this.PointToScreen(new System.Drawing.Point(Width / 2, Height / 2));
                var p2 = this.PointToScreen(new System.Drawing.Point(e.X, e.Y));
                GLThread.InvokeOnMouseMove(new OpenTK.Input.MouseMoveEventArgs(e.X, e.Y, p2.X - p.X, p2.Y - p.Y));
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
            GLThread.InvokeOnBeforeDraw();
            GLThread.InvokeQueue();

            LightPool.MapAll();

            PostProcessFramebuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (Skybox.Current != null) Skybox.Current.Draw();

            LightPool.UseTextures(0);
            World.Root.Draw();
            NormalWritingFramebuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            PostProcessFramebuffer.RevertToDefault();

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            PostProcessFramebuffer.UseTexture(8);

            LightPool.UseTextures(0);
            PostProcessingMesh.Draw();

            GLThread.InvokeOnAfterDraw();
            GLThread.CheckErrors();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //if(Camera.Current != null)Camera.Current.LookAt(new Vector3((DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f, 0, (DateTime.Now - GLThread.StartTime).Milliseconds / 1000.0f * 10.0f));
            GLThread.InvokeOnUpdate();
            MeshLinker.Resolve();
            Debugger.Send("FrameTime", e.Time);
            Debugger.Send("FPS", 1.0/e.Time);
            var keyboard = OpenTK.Input.Keyboard.GetState();
            //if (Camera.Current != null) Camera.Current.ProcessKeyboardState(keyboard);
            if (keyboard[OpenTK.Input.Key.Escape])
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                // this disposing causes more errors than normal killing
                //World.Root.DisposePhysics();
                //Exit();
            }
        }
    }
}