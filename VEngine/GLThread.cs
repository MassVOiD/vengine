using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

using System.Diagnostics;
using System.Threading;
using System.Timers;
namespace VEngine
{
    public class GLThread
    {
        static public Vector2 Resolution;
        static public DateTime StartTime;
        private static Queue<Action> ActionQueue = new Queue<Action>();

        static public event EventHandler<OpenTK.Input.KeyboardKeyEventArgs> OnKeyDown, OnKeyUp;

        static public event EventHandler<OpenTK.KeyPressEventArgs> OnKeyPress;

        static public event EventHandler<OpenTK.Input.MouseButtonEventArgs> OnMouseDown, OnMouseUp;

        static public event EventHandler<OpenTK.Input.MouseMoveEventArgs> OnMouseMove;

        static public event EventHandler<OpenTK.Input.MouseWheelEventArgs> OnMouseWheel;

        static public event EventHandler OnUpdate, OnBeforeDraw, OnAfterDraw, OnLoad;

        static public VEngineWindowAdapter WindowAdapter;
        static public GraphicsSettings GraphicsSettings = new GraphicsSettings();

        static public void CheckErrors()
        {
           /* var error = GL.GetError();
            if(error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
            }*/
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
        static public System.Threading.Tasks.Task RunAsync(Action action)
        {
            return System.Threading.Tasks.Task.Run(action);
        }

        static public void InvokeOnAfterDraw()
        {
            if(OnAfterDraw != null)
                OnAfterDraw.Invoke(null, new EventArgs());
        }

        static public void InvokeOnBeforeDraw()
        {
            if(OnBeforeDraw != null)
                OnBeforeDraw.Invoke(null, new EventArgs());
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

        static public void InvokeOnUpdate()
        {
            if(OnUpdate != null)
                OnUpdate.Invoke(null, new EventArgs());
        }

        static public void InvokeQueue()
        {
            //int count = 5; // 5 actions per frame
            while(ActionQueue.Count > 0)
            {
                var obj = ActionQueue.Dequeue();
                if(obj != null) obj.Invoke();
            }
        }

        [DllImport("kernel32")]
        static extern int GetCurrentThreadId();

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
    }
}