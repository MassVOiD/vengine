using System;
using OpenTK;

namespace VEngine
{
    public static class TransformationManagerExtensions
    {
        public static Quaternion GetOrientation(this ITransformable o)
        {
            return o.GetTransformationManager().Orientation;
        }

        public static Vector3 GetPosition(this ITransformable o)
        {
            return o.GetTransformationManager().Position;
        }

        public static Vector3 GetScale(this ITransformable o)
        {
            return o.GetTransformationManager().ScaleValue;
        }

        public static TransformationManager Rotate(this ITransformable o, Quaternion orient)
        {
            o.GetTransformationManager().Rotate(orient);
            return o.GetTransformationManager();
        }

        /*public static TransformationManager Rotate(this ITransformable o, float pitch, float yaw, float roll)
        {
            o.GetTransformationManager().Orientation = Quaternion.Multiply(o.GetTransformationManager().Orientation, Quaternion.FromEulerAngles(pitch, yaw, roll));
            o.GetTransformationManager().MarkAsModified();
            return o.GetTransformationManager();
        }*/

        public static TransformationManager Scale(this ITransformable o, float scale)
        {
            o.GetTransformationManager().Scale(scale);
            return o.GetTransformationManager();
        }

        public static TransformationManager Scale(this ITransformable o, Vector3 scale)
        {
            o.GetTransformationManager().Scale(scale);
            return o.GetTransformationManager();
        }

        public static TransformationManager Scale(this ITransformable o, float x, float y, float z)
        {
            o.GetTransformationManager().Scale(x, y, z);
            return o.GetTransformationManager();
        }

        public static TransformationManager SetOrientation(this ITransformable o, Quaternion orient)
        {
            o.GetTransformationManager().SetOrientation(orient);
            return o.GetTransformationManager();
        }

        /*public static TransformationManager SetOrientation(this ITransformable o, float pitch, float yaw, float roll)
        {
            o.GetTransformationManager().Orientation = Quaternion.FromEulerAngles(pitch, yaw, roll);
            o.GetTransformationManager().MarkAsModified();
            return o.GetTransformationManager();
        }*/

        public static TransformationManager SetPosition(this ITransformable o, Vector3 pos)
        {
            o.GetTransformationManager().SetPosition(pos);
            return o.GetTransformationManager();
        }

        public static TransformationManager SetPosition(this ITransformable o, float x, float y, float z)
        {
            o.GetTransformationManager().SetPosition(x, y, z);
            return o.GetTransformationManager();
        }

        public static TransformationManager SetScale(this ITransformable o, Vector3 scale)
        {
            o.GetTransformationManager().Scale(scale);
            return o.GetTransformationManager();
        }

        public static TransformationManager SetScale(this ITransformable o, float x, float y, float z)
        {
            o.GetTransformationManager().Scale(x, y, z);
            return o.GetTransformationManager();
        }

        public static TransformationManager SetScale(this ITransformable o, float scale)
        {
            o.GetTransformationManager().Scale(scale);
            return o.GetTransformationManager();
        }

        public static TransformationManager Translate(this ITransformable o, Vector3 pos)
        {
            o.GetTransformationManager().Translate(pos);
            return o.GetTransformationManager();
        }

        public static TransformationManager Translate(this ITransformable o, float x, float y, float z)
        {
            o.GetTransformationManager().Translate(x, y, z);
            return o.GetTransformationManager();
        }
    }

    public class TransformationManager
    {
        private bool BeenModified;

        private DateTime LastUpdated;

        public TransformationManager(Vector3 pos, Quaternion orient, Vector3 scale)
        {
            Position = new ValuePointer<Vector3>(pos);
            Orientation = orient;
            ScaleValue = scale;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public TransformationManager(Vector3 pos, Vector3 axis, float angle, Vector3 scale)
        {
            Position = pos;
            Orientation = Quaternion.FromAxisAngle(axis, angle);
            ScaleValue = scale;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public TransformationManager(Vector3 pos, Quaternion orient, float scale)
        {
            Position = pos;
            Orientation = orient;
            ScaleValue = new Vector3(scale, scale, scale);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public TransformationManager(Vector3 pos, Vector3 axis, float angle, float scale)
        {
            Position = pos;
            Orientation = Quaternion.FromAxisAngle(axis, angle);
            ScaleValue = new Vector3(scale, scale, scale);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public TransformationManager(Vector3 pos, Quaternion orient)
        {
            Position = pos;
            Orientation = orient;
            ScaleValue = new Vector3(1, 1, 1);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public TransformationManager(Vector3 pos)
        {
            Position = pos;
            Orientation = Quaternion.Identity;
            ScaleValue = new Vector3(1, 1, 1);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public delegate void DelegaetOnUpdate();

        public event DelegaetOnUpdate OnUpdate;

        public ValuePointer<Quaternion> Orientation
        {
            get; protected set;
        }

        public ValuePointer<Vector3> Position
        {
            get; protected set;
        }

        public ValuePointer<Vector3> ScaleValue
        {
            get; protected set;
        }

        public void ClearModifiedFlag()
        {
            BeenModified = false;
            Position.ClearModifiedFlag();

            Orientation.ClearModifiedFlag();

            ScaleValue.ClearModifiedFlag();
        }

        public TransformationManager Copy()
        {
            return new TransformationManager(GetPosition(), GetOrientation(), GetScale());
        }

        public Quaternion GetOrientation()
        {
            return Orientation;
        }

        public Vector3 GetPosition()
        {
            return Position;
        }

        public Matrix4 GetRotationMatrix()
        {
            return Matrix4.CreateFromQuaternion(Orientation);
        }

        public Vector3 GetScale()
        {
            return ScaleValue;
        }

        public Matrix4 GetWorldTransform()
        {
            return Matrix4.CreateScale(ScaleValue) * Matrix4.CreateFromQuaternion(Orientation) * Matrix4.CreateTranslation(Position);
        }

        public Matrix4 GetWorldTransformWithoutOrientation()
        {
            return Matrix4.CreateScale(ScaleValue) * Matrix4.CreateTranslation(Position);
        }

        public bool HasBeenModified()
        {
            return BeenModified || Position.HasBeenModified() || Orientation.HasBeenModified() || ScaleValue.HasBeenModified();
        }

        public void MarkAsModified()
        {
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
        }

        public TransformationManager Rotate(Quaternion orient)
        {
            Orientation = Quaternion.Multiply(Orientation, orient);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager Scale(float scale)
        {
            ScaleValue.R *= scale;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager Scale(Vector3 scale)
        {
            ScaleValue *= scale;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        //---------------/
        //---------------/
        public TransformationManager Scale(float x, float y, float z)
        {
            ScaleValue *= new Vector3(x, y, z);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager SetOrientation(Quaternion orient)
        {
            Orientation = orient;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager SetPosition(Vector3 pos)
        {
            Position = pos;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        //---------------/
        public TransformationManager SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager SetScale(Vector3 scale)
        {
            ScaleValue = scale;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        //---------------/
        public TransformationManager SetScale(float x, float y, float z)
        {
            ScaleValue = new Vector3(x, y, z);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager SetScale(float scale)
        {
            ScaleValue = new Vector3(scale, scale, scale);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        public TransformationManager Translate(Vector3 pos)
        {
            Position += pos;
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        //---------------/
        //---------------/
        public TransformationManager Translate(float x, float y, float z)
        {
            Position += new Vector3(x, y, z);
            BeenModified = true;
            LastUpdated = DateTime.Now;
            FireOnUpdate();
            return this;
        }

        private void FireOnUpdate()
        {
            if(OnUpdate != null)
                OnUpdate.Invoke();
        }

        //---------------/
        //---------------/
        //---------------/
        /*public TransformationManager Rotate(float pitch, float yaw, float roll)
        {
            Orientation = Quaternion.Multiply(Orientation, Quaternion.FromEulerAngles(pitch, yaw, roll));
            BeenModified = true;
            return this;
        }*/
        //---------------/
        /*public TransformationManager SetOrientation(float pitch, float yaw, float roll)
        {
            Orientation = Quaternion.FromEulerAngles(pitch, yaw, roll);
            BeenModified = true;
            return this;
        }*/
        //---------------/
        //---------------/
        //---------------/
    }
}