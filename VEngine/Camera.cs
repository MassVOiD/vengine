using BulletSharp;
using OpenTK;

namespace VDGTech
{
    public class Camera : ITransformable
    {
        public Camera(Vector3 position, Vector3 lookAt, float aspectRatio, float fov, float near, float far)
        {
            Transformation = new TransformationManager(position, Quaternion.Identity, 1.0f);
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
            Transformation = new TransformationManager(position, Quaternion.Identity, 1.0f);
            Far = far;
            //ViewMatrix = Matrix4.LookAt(position, lookAt, new Vector3(0, 1, 0));
            Matrix4.CreateOrthographic(size.X, size.Y, near, far, out ProjectionMatrix);
            if(Current == null)
                Current = this;
            Pitch = 0.0f;
            Roll = 0.0f;
            Update();
        }

        static public Camera Current;
        public float CurrentDepthFocus = 0.06f;
        public float LensBlurAmount = 0.0f;
        public float Pitch, Roll, Far;
        public TransformationManager Transformation;
        public Matrix4 ViewMatrix, RotationMatrix, ProjectionMatrix;

        public Vector3 GetDirection()
        {
            var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Pitch);
            var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Roll);
            Vector4 direction = Vector4.UnitZ;
            direction = Vector4.Transform(direction, rotationY);
            direction = Vector4.Transform(direction, rotationX);
            return -direction.Xyz;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public void LookAt(Vector3 location)
        {
            RotationMatrix = Matrix4.LookAt(-Vector3.Zero, location - Transformation.GetPosition(), new Vector3(0, 1, 0));
            ViewMatrix = Matrix4.LookAt(Transformation.GetPosition(), location, new Vector3(0, 1, 0));
        }

        public void ProcessKeyboardState(OpenTK.Input.KeyboardState keys)
        {
            /**/
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

        public Mesh3d RayCastMesh3d()
        {
            var dir = GetDirection();
            ClosestRayResultCallback rrc = new ClosestRayResultCallback(Transformation.GetPosition() + dir, Transformation.GetPosition() + dir * 10000.0f);
            World.Root.PhysicalWorld.RayTest(Transformation.GetPosition() + dir, Transformation.GetPosition() + dir * 10000.0f, rrc);
            if(rrc.HasHit)
            {
                return rrc.CollisionObject.UserObject as Mesh3d;
            }
            else
                return null;
        }

        public Vector3 RayCastPosition()
        {
            var dir = GetDirection();
            ClosestRayResultCallback rrc = new ClosestRayResultCallback(Transformation.GetPosition() + dir, Transformation.GetPosition() + dir * 10000.0f);
            World.Root.PhysicalWorld.RayTest(Transformation.GetPosition() + dir, Transformation.GetPosition() + dir * 10000.0f, rrc);
            if(rrc.HasHit)
            {
                return rrc.HitPointWorld;
            }
            else
                return Vector3.Zero;
        }

        public void Update()
        {
            Transformation.GetOrientation().Invert();
            RotationMatrix = Matrix4.CreateFromQuaternion(Transformation.GetOrientation());
            ViewMatrix = Matrix4.CreateTranslation(-Transformation.GetPosition()) * RotationMatrix;
        }

        public void UpdateFromRollPitch()
        {
            var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, Pitch);
            var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, Roll);
            Transformation.SetOrientation(Quaternion.Multiply(rotationX.Inverted(), rotationY.Inverted()));
            RotationMatrix = Matrix4.CreateFromQuaternion(rotationX) * Matrix4.CreateFromQuaternion(rotationY);
            ViewMatrix = Matrix4.CreateTranslation(-Transformation.GetPosition()) * RotationMatrix;
        }
    }
}