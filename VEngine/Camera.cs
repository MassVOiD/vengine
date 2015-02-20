using BEPUphysics;
using OpenTK;

namespace VDGTech
{
    public class Camera
    {
        static public Camera Current;
        public Matrix4 ViewMatrix, RotationMatrix, ProjectionMatrix;
        public Vector3 Position;
        public Quaternion Orientation;
        public float Pitch, Roll, Far;

        public Camera(Vector3 position, Vector3 lookAt, float aspectRatio, float fov, float near, float far)
        {
            Position = position;
            //ViewMatrix = Matrix4.LookAt(position, lookAt, new Vector3(0, 1, 0));
            Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far, out ProjectionMatrix);
            Far = far;
            if(Current == null)
                Current = this;
            Pitch = 0.0f;
            Roll = 0.0f;
            Update();
        }
        public Camera(Vector3 position, Vector3 lookAt, Vector2 size, float near, float far)
        {
            Position = position;
            Far = far;
            //ViewMatrix = Matrix4.LookAt(position, lookAt, new Vector3(0, 1, 0));
            Matrix4.CreateOrthographic(size.X, size.Y, near, far, out ProjectionMatrix);
            if(Current == null)
                Current = this;
            Pitch = 0.0f;
            Roll = 0.0f;
            Update();
        }

        public void LookAt(Vector3 location)
        {
            RotationMatrix = Matrix4.LookAt(-Vector3.Zero, location - Position, new Vector3(0, 1, 0));
            ViewMatrix = Matrix4.LookAt(Position, location, new Vector3(0, 1, 0));
        }

        
        public Mesh3d RayCastMesh3d()
        {
            RayCastResult rcResult;
            var dir = GetDirection();
            World.Root.PhysicalWorld.RayCast(new BEPUutilities.Ray(Position + (dir * 2), dir), 10000.0f, out rcResult);
            return rcResult.HitObject != null ? rcResult.HitObject.Tag as Mesh3d : null;
        }
        public Vector3 RayCastPosition()
        {
            RayCastResult rcResult;
            var dir = GetDirection();
            World.Root.PhysicalWorld.RayCast(new BEPUutilities.Ray(Position + (dir * 2), dir), 10000.0f, out rcResult);
            return rcResult.HitData.Location;
        }

        public void ProcessMouseMovement(int deltax, int deltay)
        {
            /*Pitch += (float)deltax / 100.0f;
            if (Pitch > MathHelper.TwoPi) Pitch = 0.0f;

            Roll += (float)deltay / 100.0f;
            if (Roll > MathHelper.Pi / 2) Roll = MathHelper.Pi / 2;
            if (Roll < -MathHelper.Pi / 2) Roll = -MathHelper.Pi / 2;

            Update();*/
        }

        public void Update()
        {
            Orientation.Invert();
            RotationMatrix = Matrix4.CreateFromQuaternion(Orientation);
            ViewMatrix = Matrix4.CreateTranslation(-Position) * RotationMatrix;
        }
        public void UpdateFromRollPitch()
        {
            var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, Pitch);
            var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, Roll);
            Orientation = Quaternion.Multiply(rotationX.Inverted(), rotationY.Inverted());
            RotationMatrix = Matrix4.CreateFromQuaternion(rotationX) * Matrix4.CreateFromQuaternion(rotationY);
            ViewMatrix = Matrix4.CreateTranslation(-Position) * RotationMatrix;
        }
        public Vector3 GetDirection()
        {
            var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Pitch);
            var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Roll);
            Vector4 direction = Vector4.UnitZ;
            direction = Vector4.Transform(direction, rotationY);
            direction = Vector4.Transform(direction, rotationX);
            System.Console.WriteLine(direction.ToString());
            return -direction.Xyz;
        }

        public void ProcessKeyboardState(OpenTK.Input.KeyboardState keys)
        {
            /**/
        }
    }
}