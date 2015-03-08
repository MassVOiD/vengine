using OpenTK;

namespace VDGTech
{
    public static class MathExtensions
    {
        public enum TangentDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        public static Vector3 GetTangent(this Quaternion quaternion, TangentDirection direction)
        {
            switch(direction)
            {
                case TangentDirection.Up:
                return Vector3.Transform(Vector3.UnitY, quaternion);

                case TangentDirection.Down:
                return Vector3.Transform(-Vector3.UnitY, quaternion);

                case TangentDirection.Left:
                return Vector3.Transform(Vector3.UnitX, quaternion);

                case TangentDirection.Right:
                return Vector3.Transform(-Vector3.UnitX, quaternion);
            }
            return Vector3.Zero;
        }

        public static Vector3 Rotate(this Vector3 vector, Quaternion quaternion)
        {
            return Vector3.Transform(vector, quaternion);
        }
        public static Matrix4 ToMatrix(this Vector3 vector)
        {
            return Matrix4.CreateTranslation(vector);
        }

        public static Vector3 ToDirection(this Quaternion quaternion)
        {
            return Vector3.Transform(-Vector3.UnitZ, quaternion);
        }
    }
}