using System;
using System.Collections.Generic;

namespace VDGTech
{
    public class GLThread
    {
        static public DateTime StartTime;
        private static Queue<Action> ActionQueue = new Queue<Action>();
        static public event EventHandler OnUpdate, OnBeforeDraw, OnAfterDraw;

        static public void Invoke(Action action)
        {
            ActionQueue.Enqueue(action);
        }
        static public void InvokeOnUpdate()
        {
            if (OnUpdate != null) OnUpdate.Invoke(null, new EventArgs());
        }
        static public void InvokeOnBeforeDraw()
        {
            if(OnBeforeDraw != null) OnBeforeDraw.Invoke(null, new EventArgs());
        }
        static public void InvokeOnAfterDraw()
        {
            if (OnAfterDraw != null) OnAfterDraw.Invoke(null, new EventArgs());
        }

        static public void InvokeQueue()
        {
            int count = 5; // 5 actions per frame
            while (ActionQueue.Count > 0 && count-- > 0) ActionQueue.Dequeue().Invoke();
        }
    }
}