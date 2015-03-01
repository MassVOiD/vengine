using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VDGTech
{
    public class TransformationManager
    {
        private Vector3 Position;
        private Quaternion Orientation;
        private float ScaleValue;
        public bool BeenModified;

        public TransformationManager(Vector3 pos, Quaternion orient, float scale)
        {
            Position = pos;
            Orientation = orient;
            ScaleValue = scale;
            BeenModified = true;
        }

        public Vector3 GetPosition()
        {
            return Position;
        }

        public TransformationManager SetPosition(Vector3 pos)
        {
            Position = pos;
            BeenModified = true;
            return this;
        }
        public TransformationManager Translate(Vector3 pos)
        {
            Position += pos;
            BeenModified = true;
            return this;
        }
        public Quaternion GetOrientation()
        {
            return Orientation;
        }

        public TransformationManager SetOrientation(Quaternion orient)
        {
            Orientation = orient;
            BeenModified = true;
            return this;
        }
        public TransformationManager Rotate(Quaternion orient)
        {
            Orientation = Quaternion.Mult(Orientation, orient);
            BeenModified = true;
            return this;
        }
        public float GetScale()
        {
            return ScaleValue;
        }
        public TransformationManager SetScale(float scale)
        {
            ScaleValue = scale;
            BeenModified = true;
            return this;
        }
        public TransformationManager Scale(float scale)
        {
            ScaleValue *= scale;
            BeenModified = true;
            return this;
        }
    }
}
