using OpenTK;

namespace VEngine
{
    public class Camera : ITransformable
    {
        static public Camera Current;

        static public Camera MainDisplayCamera;

        public float Brightness = 1.0f;

        public float CurrentDepthFocus = 0.06f;

        public float FocalLength = 75.0f;

        public float LensBlurAmount = 0.0f;

        public float Pitch, Roll, Far;

        public TransformationManager Transformation;

        private FrustumCone cone;

        private Matrix4 ViewMatrix, RotationMatrix, ProjectionMatrix, VPMatrix;

        private class FrustumCone
        {
            public Vector3 LeftBottom;
            public Vector3 LeftTop;
            public Vector3 Origin;
            public Vector3 RightBottom;
            public Vector3 RightTop;

            public static FrustumCone Create(Vector3 origin, Matrix4 view, Matrix4 proj)
            {
                var f = new FrustumCone();
                f.Origin = origin;
                var inVP = Matrix4.Mult(view, proj).Inverted();
                f.LeftBottom = GetDir(origin, new Vector2(-1, -1), inVP);
                f.RightBottom = GetDir(origin, new Vector2(1, -1), inVP);
                f.LeftTop = GetDir(origin, new Vector2(-1, 1), inVP);
                f.RightTop = GetDir(origin, new Vector2(1, 1), inVP);
                return f;
            }

            public static void Update(FrustumCone f, Vector3 origin, Matrix4 view, Matrix4 proj)
            {
                f.Origin = origin;
                var inVP = Matrix4.Mult(view, proj).Inverted();
                f.LeftBottom = GetDir(origin, new Vector2(-1, -1), inVP);
                f.RightBottom = GetDir(origin, new Vector2(1, -1), inVP);
                f.LeftTop = GetDir(origin, new Vector2(-1, 1), inVP);
                f.RightTop = GetDir(origin, new Vector2(1, 1), inVP);
            }

            private static Vector3 GetDir(Vector3 origin, Vector2 uv, Matrix4 inv)
            {
                var clip = Vector4.Transform(new Vector4(uv.X, uv.Y, 0.01f, 1), inv);
                return (clip.Xyz / clip.W - origin).Normalized();
            }
        }

        public Camera(Vector3 position, Vector3 lookAt, Vector3 up, float aspectRatio, float fov, float near, float far)
        {
            Transformation = new TransformationManager(position, Quaternion.Identity, 1.0f);
            Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far, out ProjectionMatrix);
            Far = far;
            if(Current == null)
                Current = this;
            if(MainDisplayCamera == null)
                MainDisplayCamera = this;
            Pitch = 0.0f;
            Roll = 0.0f;
            Transformation.SetOrientation(Matrix4.LookAt(Vector3.Zero, lookAt, up).ExtractRotation().Inverted());
            Update();
        }

        public bool IsPointVisible(Vector3 point)
        {
            Vector4 clip = Vector4.Transform(new Vector4(point, 1.0f), VPMatrix);
            if(clip.Z <= 0)
                return false;
            Vector2 UV = clip.Xy / clip.W;
            if(UV.X <= -1 || UV.X >= 1 || UV.Y < -1 || UV.Y > 1)
                return false;
            return true;
        }

        public bool IsAABBVisible(Vector3 min, Vector3 max)
        {
            return
                IsPointVisible(min) ||
                IsPointVisible(max) ||

                IsPointVisible(new Vector3(min.X, min.Y, max.Z)) ||
                IsPointVisible(new Vector3(min.X, max.Y, max.Z)) ||
                IsPointVisible(new Vector3(min.X, max.Y, min.Z)) ||

                IsPointVisible(new Vector3(max.X, min.Y, min.Z)) ||
                IsPointVisible(new Vector3(max.X, min.Y, min.Z)) ||
                IsPointVisible(new Vector3(max.X, min.Y, max.Z));
        }

        public void UpdatePerspective(float aspectRatio, float fov, float near, float far)
        {
            Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far, out ProjectionMatrix);
            Far = far;
        }

        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4 result)
        {
            result = Matrix4.Identity;

            result.Row0.X = 2.0f / (right - left);
            result.Row1.Y = 2.0f / (top - bottom);
            result.Row2.Z = 2.0f / (zFar - zNear);

            result.Row3.X = -(right + left) / (right - left);
            result.Row3.Y = -(top + bottom) / (top - bottom);
            result.Row3.Z = -(zFar + zNear) / (zFar - zNear);

            /*
            float invRL = 1.0f / (right - left);
            float invTB = 1.0f / (top - bottom);
            float invFN = 1.0f / (zFar - zNear);

            result.Row0.X = 2 * invRL;
            result.Row1.Y = 2 * invTB;
            result.Row2.Z = -2 * invFN;

            result.Row3.X = -(right + left) * invRL;
            result.Row3.Y = -(top + bottom) * invTB;
            result.Row3.Z = -(zFar + zNear) * invFN;*/
        }

        public void UpdatePerspectiveOrtho(float left, float right, float bottom, float top, float near, float far)
        {
            CreateOrthographicOffCenter(left, right, bottom, top, near, far, out ProjectionMatrix);
            Far = far;
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

        public Vector3 GetDirection()
        {
            var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Pitch);
            var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Roll);
            Vector4 direction = Vector4.UnitZ;
            direction = Vector4.Transform(direction, rotationY);
            direction = Vector4.Transform(direction, rotationX);
            return -direction.Xyz;
        }

        public Matrix4 GetProjectionMatrix()
        {
            return ProjectionMatrix;
        }

        public Matrix4 GetRotationMatrix()
        {
            return RotationMatrix;
        }

        public TransformationManager GetTransformationManager()
        {
            return Transformation;
        }

        public Matrix4 GetViewMatrix()
        {
            return ViewMatrix;
        }

        public Matrix4 GetVPMatrix()
        {
            return VPMatrix;
        }

        public void LookAt(Vector3 location)
        {
            RotationMatrix = Matrix4.CreateFromQuaternion(Matrix4.LookAt(Vector3.Zero, -(location - Transformation.GetPosition()).Normalized(), new Vector3(0, 1, 0)).ExtractRotation());
            ViewMatrix = RotationMatrix * Matrix4.CreateTranslation(-Transformation.GetPosition());
            Transformation.SetOrientation(RotationMatrix.ExtractRotation());
            Transformation.ClearModifiedFlag();
            VPMatrix = Matrix4.Mult(ViewMatrix, ProjectionMatrix);
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
        
        public void SetUniforms()
        {
            if(cone == null)
                return;
            lock (Transformation)
            {
                var s = ShaderProgram.Current;
                s.SetUniform("FrustumConeLeftBottom", cone.LeftBottom);
                //s.SetUniform("FrustumConeLeftTop", cone.LeftTop);
                //s.SetUniform("FrustumConeRightBottom", cone.RightBottom);
                //s.SetUniform("FrustumConeRightTop", cone.RightTop);
                s.SetUniform("FrustumConeBottomLeftToBottomRight", cone.RightBottom - cone.LeftBottom);
                s.SetUniform("FrustumConeBottomLeftToTopLeft", cone.LeftTop - cone.LeftBottom);
            }
        }
        public class ConeInfo
        {
            public Vector3 FrustumConeLeftBottom, FrustumConeBottomLeftToBottomRight, FrustumConeBottomLeftToTopLeft;
        }
        public ConeInfo GetConeInfo()
        {
            return new ConeInfo()
            {
                FrustumConeLeftBottom = cone.LeftBottom,
                FrustumConeBottomLeftToBottomRight = cone.RightBottom - cone.LeftBottom,
                FrustumConeBottomLeftToTopLeft = cone.LeftTop - cone.LeftBottom
            };
        }

        public void Update()
        {
            lock (Transformation)
            {
                var tRotationMatrix = Matrix4.CreateFromQuaternion(Transformation.GetOrientation().Inverted());
                var tViewMatrix = Matrix4.CreateTranslation(-Transformation.GetPosition()) * RotationMatrix;

                if(cone == null)
                {
                    var tcone = FrustumCone.Create(Transformation.GetPosition(), tViewMatrix, ProjectionMatrix);
                    cone = tcone;
                } else FrustumCone.Update(cone, Transformation.GetPosition(), tViewMatrix, ProjectionMatrix);

                RotationMatrix = tRotationMatrix;
                ViewMatrix = tViewMatrix;
                VPMatrix = Matrix4.Mult(ViewMatrix, ProjectionMatrix);
            }
        }

        public void UpdateFromRollPitch()
        {
            lock (Transformation)
            {
                var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, Pitch);
                var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, Roll);
                Transformation.SetOrientation(Quaternion.Multiply(rotationX.Inverted(), rotationY.Inverted()));
                var tRotationMatrix = Matrix4.CreateFromQuaternion(rotationX) * Matrix4.CreateFromQuaternion(rotationY);
                var tViewMatrix = Matrix4.CreateTranslation(-Transformation.GetPosition()) * RotationMatrix;

                if(cone == null)
                {
                    var tcone = FrustumCone.Create(Transformation.GetPosition(), tViewMatrix, ProjectionMatrix);
                    cone = tcone;
                }
                else
                    FrustumCone.Update(cone, Transformation.GetPosition(), tViewMatrix, ProjectionMatrix);
                RotationMatrix = tRotationMatrix;
                ViewMatrix = tViewMatrix;
                VPMatrix = Matrix4.Mult(ViewMatrix, ProjectionMatrix);
            }
        }

        public void UpdateInverse()
        {
            RotationMatrix = Matrix4.CreateFromQuaternion(Transformation.GetOrientation());
            ViewMatrix = RotationMatrix * Matrix4.CreateTranslation(Transformation.GetPosition());
            VPMatrix = Matrix4.Mult(ViewMatrix, ProjectionMatrix);
        }
    }
}