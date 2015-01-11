using System;
using System.Collections.Generic;

namespace VDGTech
{
    public class GLThread
    {
        static public DateTime StartTime;
        private static Queue<Action> ActionQueue = new Queue<Action>();

        static public void Invoke(Action action)
        {
            ActionQueue.Enqueue(action);
        }

        static public void InvokeQueue()
        {
            int count = 5; // 5 actions per frame
            while (ActionQueue.Count > 0 && count-- > 0) ActionQueue.Dequeue().Invoke();
        }
    }
}