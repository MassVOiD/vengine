using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;

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
                float b = 0;
                float c = 1;
                if(seconds > Duration)
                {
                    HasEnded = true;
                    return ValueEnd.R;
                }
                float percent = 0;
                switch(Easing)
                {
                    case Easing.Linear:
                    percent = c * t / d + b;
                    break;

                    case Easing.EaseIn:
                    t /= d;
                    percent = c * t * t * t + b;
                    break;

                    case Easing.EaseOut:
                    t /= d;
                    t--;
                    percent = c * (t * t * t + 1) + b;
                    break;

                    case Easing.EaseInOut:
                    t /= d / 2;
                    if(t < 1)
                    {
                        percent = c / 2 * t * t * t + b;
                        break;
                    }
                    t -= 2;
                    percent = c / 2 * (t * t * t + 2) + b;
                    break;

                    case Easing.QuadEaseIn:
                    t /= d;
                    percent = c * t * t * t * t + b;
                    break;

                    case Easing.QuadEaseOut:
                    t /= d;
                    t--;
                    percent = -c * (t * t * t * t - 1) + b;
                    break;

                    case Easing.QuadEaseInOut:
                    t /= d / 2;
                    if(t < 1)
                    {
                        percent = c / 2 * t * t * t * t + b;
                        break;
                    }
                    t -= 2;
                    percent = -c / 2 * (t * t * t * t - 2) + b;
                    break;
                }
                if(ValueStart.R is Quaternion)
                {
                    dynamic q1 = ValueStart.R;
                    dynamic q2 = ValueEnd.R;
                    return Quaternion.Slerp(q1, q2, percent);
                }
                else if(ValueStart.R is Vector2)
                {
                    dynamic q1 = ValueStart.R;
                    dynamic q2 = ValueEnd.R;
                    return Vector2.Lerp(q1, q2, percent);
                }
                else if(ValueStart.R is Vector3)
                {
                    dynamic q1 = ValueStart.R;
                    dynamic q2 = ValueEnd.R;
                    return Vector3.Lerp(q1, q2, percent);
                }
                else if(ValueStart.R is Vector4)
                {
                    dynamic q1 = ValueStart.R;
                    dynamic q2 = ValueEnd.R;
                    return Vector4.Lerp(q1, q2, percent);
                }
                else
                {
                    dynamic v1 = ValueStart.R;
                    dynamic v2 = ValueEnd.R;
                    return (v1 * percent + v2 * (1.0 - percent));
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