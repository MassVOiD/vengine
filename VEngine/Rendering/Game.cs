using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Game
    {
        static public AbsDisplayAdapter DisplayAdapter;
        static public GraphicsSettings GraphicsSettings = new GraphicsSettings();
        static public Size Resolution;
        static public DateTime StartTime;
        static public World World;
        static public bool Initialized = false;
        private static Queue<Action> ActionQueue = new Queue<Action>();

        static public event EventHandler<OpenTK.Input.KeyboardKeyEventArgs> OnKeyDown, OnKeyUp;

        static public event EventHandler<OpenTK.KeyPressEventArgs> OnKeyPress;

        static public event EventHandler<OpenTK.Input.MouseButtonEventArgs> OnMouseDown, OnMouseUp;

        static public event EventHandler<OpenTK.Input.MouseMoveEventArgs> OnMouseMove;

        static public event EventHandler<OpenTK.Input.MouseWheelEventArgs> OnMouseWheel;

        static public event EventHandler<FrameEventArgs> OnUpdate;
        static public event EventHandler<FrameEventArgs> OnBeforeDraw, OnAfterDraw;
        static public event EventHandler OnLoad;

        static public float CurrentFPS = 0;

        static public void Initialize(Size resolution, string mediapath, GameWindowFlags flags = GameWindowFlags.Default)
        {
            StartTime = DateTime.Now;
            World = new World();
            Media.SearchPath = mediapath;
            Media.LoadFileMap();
            Resolution = resolution;
            SetCurrentThreadCores(1);
            
            var thread = Task.Factory.StartNew(() =>
            {
                SetCurrentThreadCores(2);
                DisplayAdapter = new VEngineWindowAdapter("VEngine App", resolution.Width, resolution.Height, flags);

                GraphicsSettings.UseDeferred = true;
                GraphicsSettings.UseRSM = false;
                GraphicsSettings.UseVDAO = true;
                GraphicsSettings.UseFog = false;
                GraphicsSettings.UseBloom = false;
                GraphicsSettings.UseLightPoints = true;

                DisplayAdapter.CursorVisible = false;

                Invoke(() => Initialized = true);
                DisplayAdapter.Run(60);
            });
            while(!Initialized)
                ;
        }

        static public void JoinUntilInitialized()
        {
            while(!Initialized)
            {
                System.Threading.Thread.Sleep(100);
            }
            return;
        }

        static public void Initialize(string mediapath)
        {
            StartTime = DateTime.Now;
            World = new World();
            Media.SearchPath = mediapath;
            Media.LoadFileMap();
            SetCurrentThreadCores(1);
        }

        static public void CheckErrors(string message = "Global")
        {
           // var error = GL.GetError();
           // if(error != ErrorCode.NoError)
           // {
           //     Console.WriteLine(message + ":: " + error.ToString());
           // }
        }

        static public System.Timers.Timer CreateTimer(Action func, int interval)
        {
            var t = new System.Timers.Timer(interval);
            t.Elapsed += (o, e) => func.Invoke();
            return t;
        }

        static public void Invoke(Action action)
        {
            ActionQueue.Enqueue(action);
        }

        static public void InvokeOnAfterDraw(FrameEventArgs eventargs)
        {
            if(OnAfterDraw != null)
                OnAfterDraw.Invoke(null, eventargs);
        }

        static public void InvokeOnBeforeDraw(FrameEventArgs eventargs)
        {
            if(OnBeforeDraw != null)
                OnBeforeDraw.Invoke(null, eventargs);
        }

        static public void InvokeOnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if(OnKeyDown != null)
                OnKeyDown.Invoke(null, e);
        }

        static public void InvokeOnKeyPress(OpenTK.KeyPressEventArgs e)
        {
            if(OnKeyPress != null)
                OnKeyPress.Invoke(null, e);
        }

        static public void InvokeOnKeyUp(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if(OnKeyUp != null)
                OnKeyUp.Invoke(null, e);
        }

        static public void InvokeOnLoad()
        {
            if(OnLoad != null)
                OnLoad.Invoke(null, new EventArgs());
        }

        static public void InvokeOnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            if(OnMouseDown != null)
                OnMouseDown.Invoke(null, e);
        }

        static public void InvokeOnMouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            if(OnMouseMove != null)
                OnMouseMove.Invoke(null, e);
        }

        static public void InvokeOnMouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            if(OnMouseUp != null)
                OnMouseUp.Invoke(null, e);
        }

        static public void InvokeOnMouseWheel(OpenTK.Input.MouseWheelEventArgs e)
        {
            if(OnMouseWheel != null)
                OnMouseWheel.Invoke(null, e);
        }

        static public void InvokeOnUpdate(FrameEventArgs eventargs)
        {
            if(OnUpdate != null)
                OnUpdate.Invoke(null, eventargs);
        }

        static public void InvokeQueue()
        {
            //int count = 5; // 5 actions per frame
            while(ActionQueue.Count > 0)
            {
                var obj = ActionQueue.Dequeue();
                if(obj != null)
                    obj.Invoke();
            }
        }

        static public void InvokeSynchronized(Action action)
        {
            bool jobdone = false;
            Action action2 = new Action(() =>
            {
                action.Invoke();
                jobdone = true;
            });
            ActionQueue.Enqueue(action);
            while(!jobdone)
                ;
        }

        static public System.Threading.Tasks.Task RunAsync(Action action)
        {
            return System.Threading.Tasks.Task.Run(action);
        }

        static public void SetCurrentThreadCores(int core)
        {
            foreach(ProcessThread pt in Process.GetCurrentProcess().Threads)
            {
                int utid = GetCurrentThreadId();
                if(utid == pt.Id)
                {
                    pt.ProcessorAffinity = (IntPtr)(1 << core);
                }
            }
        }

        [DllImport("kernel32")]
        private static extern int GetCurrentThreadId();
    }
}