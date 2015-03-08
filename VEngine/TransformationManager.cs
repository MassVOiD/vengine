using OpenTK;

namespace VDGTech
{
    public class TransformationManager
    {
        public TransformationManager(Vector3 pos, Quaternion orient, Vector3 scale)
        {
            Position = pos;
            Orientation = orient;
            ScaleValue = scale;
            BeenModified = true;
        }
        public TransformationManager(Vector3 pos, Vector3 orient, Vector3 scale)
        {
            Position = pos;
            Orientation = Quaternion.FromEulerAngles(orient);
            ScaleValue = scale;
            BeenModified = true;
        }
        public TransformationManager(Vector3 pos, Quaternion orient, float scale)
        {
            Position = pos;
            Orientation = orient;
            ScaleValue = new Vector3(scale, scale, scale);
            BeenModified = true;
        }
        public TransformationManager(Vector3 pos, Vector3 orient, float scale)
        {
            Position = pos;
            Orientation = Quaternion.FromEulerAngles(orient);
            ScaleValue = new Vector3(scale, scale, scale);
            BeenModified = true;
        }
        public TransformationManager(Vector3 pos, Quaternion orient)
        {
            Position = pos;
            Orientation = orient;
            ScaleValue = new Vector3(1, 1, 1);
            BeenModified = true;
        }
        public TransformationManager(Vector3 pos)
        {
            Position = pos;
            Orientation = Quaternion.Identity;
            ScaleValue = new Vector3(1, 1, 1);
            BeenModified = true;
        }

        public bool BeenModified;
        private Quaternion Orientation;
        private Vector3 Position;
        private Vector3 ScaleValue;

        public Quaternion GetOrientation()
        {
            return Orientation;
        }

        public Vector3 GetPosition()
        {
            return Position;
        }

        public Vector3 GetScale()
        {
            return ScaleValue;
        }

        public TransformationManager Rotate(Quaternion orient)
        {
            Orientation = Quaternion.Multiply(Orientation, orient);
            BeenModified = true;
            return this;
        }

        public TransformationManager Scale(float scale)
        {
            ScaleValue *= scale;
            BeenModified = true;
            return this;
        }
        public TransformationManager Scale(Vector3 scale)
        {
            ScaleValue *= scale;
            BeenModified = true;
            return this;
        }

        public TransformationManager SetOrientation(Quaternion orient)
        {
            Orientation = orient;
            BeenModified = true;
            return this;
        }

        public TransformationManager SetPosition(Vector3 pos)
        {
            Position = pos;
            BeenModified = true;
            return this;
        }

        public TransformationManager SetScale(Vector3 scale)
        {
            ScaleValue = scale;
            BeenModified = true;
            return this;
        }
        public TransformationManager SetScale(float scale)
        {
            ScaleValue = new Vector3(scale, scale, scale);
            BeenModified = true;
            return this;
        }

        public TransformationManager Translate(Vector3 pos)
        {
            Position += pos;
            BeenModified = true;
            return this;
        }

        public Matrix4 GetWorldTransform()
        {
            return Matrix4.CreateScale(ScaleValue) * Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateTranslation(Position);
        }
        public Matrix4 GetWorldTransformWithoutOrientation()
        {
            return Matrix4.CreateScale(ScaleValue) * Matrix4.CreateTranslation(Position);
        }
        public Matrix4 GetRotationMatrix()
        {
            return Matrix4.CreateFromQuaternion(Orientation);
        }
    }
}