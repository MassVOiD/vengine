using OpenTK;

namespace VDGTech
{
    public class Camera
    {
        static public Camera Current;
        public Matrix4 ViewMatrix, ProjectionMatrix;
        private Vector3 Position;

        public Camera(Vector3 position, Vector3 lookAt, float fov)
        {
            Position = position;
            ViewMatrix = Matrix4.LookAt(position, lookAt, new Vector3(0, 1, 0));
            Matrix4.CreatePerspectiveFieldOfView(fov, 16.0f / 9.0f, 0.1f, 1000.0f, out ProjectionMatrix);
            if (Current == null) Current = this;
        }

        public void LookAt(Vector3 location)
        {
            ViewMatrix = Matrix4.LookAt(Position, location, new Vector3(0, 1, 0));
        }
    }
}