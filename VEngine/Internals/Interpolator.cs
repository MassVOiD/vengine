using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VEngine
{
    public class Interpolator
    {
        private class SingleInterpolation<T>
        {
            public Action Callback = null;
            public float Duration;
            public Easing Easing;
            public bool HasEnded = false;
            public ValuePointer<T> Reference;
            public long StartTime;
            public ValuePointer<T> ValueEnd;
            public ValuePointer<T> ValueStart;

            public void Interpolate()
            {
                Reference.R = GetValue();
                Reference.MarkAsModified();
            }

            private T GetValue()
            {
                long now = Stopwatch.GetTimestamp();
                float seconds = (now - StartTime) / (float)Stopwatch.Frequency;
                float t = seconds;
                float d = Duration;
                dynamic b = ValueStart.R;
                dynamic c = ValueEnd.Sub(ValueStart.R);
                if(seconds > Duration)
                {
                    HasEnded = true;
                    return ValueEnd.R;
                }

                switch(Easing)
                {
                    case Easing.Linear:
                    return c * t / d + b;

                    case Easing.EaseIn:
                    t /= d;
                    return c * t * t * t + b;

                    case Easing.EaseOut:
                    t /= d;
                    t--;
                    return c * (t * t * t + 1) + b;

                    case Easing.EaseInOut:
                    t /= d / 2;
                    if(t < 1)
                        return c / 2 * t * t * t + b;
                    t -= 2;
                    return c / 2 * (t * t * t + 2) + b;

                    case Easing.QuadEaseIn:
                    t /= d;
                    return c * t * t * t * t + b;

                    case Easing.QuadEaseOut:
                    t /= d;
                    t--;
                    return -c * (t * t * t * t - 1) + b;

                    case Easing.QuadEaseInOut:
                    t /= d / 2;
                    if(t < 1)
                        return c / 2 * t * t * t * t + b;
                    t -= 2;
                    return -c / 2 * (t * t * t * t - 2) + b;
                }
                return default(T);
            }
        }

        private static List<object> Interpolators = new List<object>();

        public enum Easing
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            QuadEaseIn,
            QuadEaseOut,
            QuadEaseInOut
        }

        public static void Interpolate<T>(ValuePointer<T> value, T start, T end, float duration, Easing easing = Easing.Linear)
        {
            Interpolators.Add(new SingleInterpolation<T>()
            {
                Reference = value,
                ValueStart = start,
                ValueEnd = end,
                Duration = duration,
                StartTime = Stopwatch.GetTimestamp(),
                Easing = easing
            });
        }

        public static void StepAll()
        {
            var emp = new object[0];
            var toRemove = new List<object>();
            foreach(var i in Interpolators)
            {
                i.GetType().GetMethod("Interpolate").Invoke(i, emp);
                bool ended = (bool)i.GetType().GetField("HasEnded").GetValue(i);
                if(ended)
                    toRemove.Add(i);
            }
            foreach(var r in toRemove)
                Interpolators.Remove(r);
        }
    }
}