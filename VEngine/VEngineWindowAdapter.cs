using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using VDGTech.Particles;

namespace VDGTech
{
    public class VEngineWindowAdapter : GameWindow, IVEngineDisplayAdapter
    {
        public VEngineWindowAdapter(string title, int width, int height)
            : base(width, height,
                new OpenTK.Graphics.GraphicsMode(32, 32, 0, 0), title, GameWindowFlags.Default,
                DisplayDevice.Default, 4, 4,
                GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            GLThread.StartTime = DateTime.Now;
            GLThread.Resolution = new Vector2(width, height);

            PostProcessFramebuffer1 = new Framebuffer(width, height);
            PostProcessFramebuffer2 = new Framebuffer(width, height);
            WorldPosFramebuffer = new Framebuffer(width, height);
            WorldPosWriteMaterial = ManualShaderMaterial.FromMedia("Generic.vertex.glsl", "WriteWorldPosition.fragment.glsl");
            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
            PostProcessingDefaultMaterialStage1 = new ManualShaderMaterial(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("PostProcess.pass1.fragment.glsl"));
            PostProcessingDefaultMaterialStage2 = new ManualShaderMaterial(Media.ReadAllText("PostProcess.vertex.glsl"), Media.ReadAllText("PostProcess.pass2.fragment.glsl"));
            PostProcessingMesh = new Mesh3d(postPlane3dInfo, PostProcessingDefaultMaterialStage1);

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

        public Framebuffer PostProcessFramebuffer1, PostProcessFramebuffer2, WorldPosFramebuffer;

        private static uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

        private static float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };

        private IMaterial PostProcessingDefaultMaterialStage1, PostProcessingDefaultMaterialStage2, WorldPosWriteMaterial;
        private Mesh3d PostProcessingMesh;

        public void SetCustomPostProcessingMaterial(IMaterial material)
        {
            PostProcessingMesh.Material = material;
        }

        public void SetDefaultPostProcessingMaterial()
        {
            PostProcessingMesh.Material = PostProcessingDefaultMaterialStage1;
        }

        public void StartPhysicsThread()
        {
            Task.Factory.StartNew(PhysicsThread);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            VSync = VSyncMode.On;

            GL.ClearColor(0.37f, 0.37f, 0.37f, 1.0f);
            var s = GL.GetString(StringName.Version);
            Console.WriteLine(s);
        }

        public void DrawAll()
        {
            World.Root.Draw();
            ParticleSystem.DrawAll();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GLThread.InvokeQueue();

            LightPool.MapAll();

            WorldPosFramebuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            WorldPosWriteMaterial.Use();
            ShaderProgram.Lock = true;
            DrawAll();
            ShaderProgram.Lock = false;

            PostProcessFramebuffer1.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if(Skybox.Current != null)
                Skybox.Current.Draw();

            LightPool.UseTextures(2);
            World.Root.ShouldUpdatePhysics = true;
            // this is here so you can issue draw calls from there if you want
            GLThread.InvokeOnBeforeDraw();
            DrawAll();
            GLThread.InvokeOnAfterDraw();
            /*NormalWritingFramebuffer.Use();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);*/

            // Now we have image in pp fbo1
            // lets set up drawing to fbo2

            PostProcessFramebuffer2.Use();

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            PostProcessFramebuffer1.UseTexture(0);
            WorldPosFramebuffer.UseTexture(31);
            var ppProgram = PostProcessingMesh.Material.GetShaderProgram();
            ppProgram.Use();
            var linesStarts = World.Root.LinesPool.GetStartsVectors();
            var linesEnds = World.Root.LinesPool.GetEndsVectors();
            var linesColors = World.Root.LinesPool.GetColors();
            ppProgram.SetUniform("Lines2dCount", linesStarts.Length);
            ppProgram.SetUniformArray("Lines2dStarts", linesStarts);
            ppProgram.SetUniformArray("Lines2dEnds", linesEnds);
            ppProgram.SetUniformArray("Lines2dColors", linesColors);
            //LightPool.UseTextures(2);

            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;


            PostProcessFramebuffer2.RevertToDefault();
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            PostProcessFramebuffer2.UseTexture(0);
            PostProcessingDefaultMaterialStage2.Use();
            if(Camera.Current != null)
            {
                ppProgram.SetUniform("CameraCurrentDepth", Camera.Current.CurrentDepthFocus);
                ppProgram.SetUniform("LensBlurAmount", Camera.Current.LensBlurAmount);
            }
            ShaderProgram.Lock = true;
            PostProcessingMesh.Draw();
            ShaderProgram.Lock = false;

            World.Root.UI.DrawAll();

            GLThread.CheckErrors();

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