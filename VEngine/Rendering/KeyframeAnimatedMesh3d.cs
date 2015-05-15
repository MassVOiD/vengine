using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VEngine
{
    public class KeyframeAnimatedMesh3d : IRenderable, ITransformable
    {
        List<Mesh3d> Frames;
        public int CurrentFrame;
        TransformationManager Transformation;
        public KeyframeAnimatedMesh3d(List<Object3dInfo> infos, IMaterial material)
        {
            Transformation = new TransformationManager(Vector3.Zero);
            Frames = new List<Mesh3d>();
            CurrentFrame = 0;
            foreach(var i in infos)
            {
                Frames.Add(new Mesh3d(i, material));
            }
        }

        public void NextFrame()
        {
            CurrentFrame++;
            if(CurrentFrame >= Frames.Count)
            {
                CurrentFrame = 0;
            }
        }
        public void PreviousFrame()
        {
            CurrentFrame--;
            if(CurrentFrame < 0)
            {
                CurrentFrame = Frames.Count - 1;
            }
        }

        public void Draw()
        {
            var mesh = Frames[CurrentFrame];
            mesh.Transformation = Transformation;
            mesh.Draw();
        }
        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }
    }
}
