using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OpenTK;

namespace VEngine
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

        public static IEnumerable<XElement> SelectMany(this XElement element, string localname)
        {
            return element.Elements().Where((a) => a.Name.LocalName.Trim() == localname);
        }

        public static XElement SelectSingle(this XElement element, string localname)
        {
            return element.Elements().First((a) => a.Name.LocalName.Trim() == localname);
        }

        public static Vector3 ToDirection(this Quaternion quaternion)
        {
            return Vector3.Transform(Vector3.UnitZ, quaternion);
        }

        public static Vector3 ToDirection(this Quaternion quaternion, Vector3 referenceAxis)
        {
            return Vector3.Transform(referenceAxis, quaternion);
        }

        public static Matrix4 ToMatrix(this Vector3 vector)
        {
            return Matrix4.CreateTranslation(vector);
        }

        public static Quaternion ToQuaternion(this Vector3 direction, Vector3 up)
        {
            return Matrix4.LookAt(Vector3.Zero, direction, up).ExtractRotation(true);
        }
    }
}