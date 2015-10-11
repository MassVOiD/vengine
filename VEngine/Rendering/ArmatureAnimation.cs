using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VEngine.Rendering
{
    public class ArmatureAnimation
    {
        public class KeyFrame
        {
            public float Duration;
            public Dictionary<string, Quaternion> Orientations;
        }

        public class TimeStepData
        {
            public KeyFrame Frame;
            public KeyFrame PreviousFrame;
            public float Step;
        }

        public List<KeyFrame> Frames = new List<KeyFrame>();

        public void Apply(Mesh3d mesh, float time, float speed = 1.0f)
        {
            var framedata = GetCurrentKeyFrame(time, speed);
            if(framedata == null)
                return;
            foreach(var o in framedata.Frame.Orientations)
            {
                var orient = Quaternion.Slerp(framedata.PreviousFrame.Orientations[o.Key], o.Value, framedata.Step);
                mesh.Bones.Find((a) => a.Name == o.Key).Orientation = orient;
            }
        }

        public TimeStepData GetCurrentKeyFrame(float time, float speed)
        {
            float accumulator = 0;
            time *= speed;
            float totalTime = Frames.Sum((a) => a.Duration);
            if(time > totalTime)
                time = time % totalTime;
            for(int i = 0; i < Frames.Count + 1; i++)
            {
                var frame = Frames[i == Frames.Count ? 0 : i];
                if(accumulator > time && accumulator < time + frame.Duration)
                {
                    float step = (time + frame.Duration - accumulator) / frame.Duration;
                    return new TimeStepData()
                    {
                        Frame = frame,
                        PreviousFrame = Frames[i == 0 ? Frames.Count - 1 : i - 1],
                        Step = step
                    };
                }
                else
                {
                    accumulator += Frames[i >= Frames.Count ? Frames.Count - 1 : i].Duration;
                }
            }
            return null;
        }
    }
}