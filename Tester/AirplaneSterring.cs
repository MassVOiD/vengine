using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;

namespace Tester
{
    partial class Airplane : IRenderable, IPhysical
    {
        
        void UpdateSterring()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            var gmepad = OpenTK.Input.GamePad.GetState(0);

            if (keyboard.IsKeyDown(OpenTK.Input.Key.W))
            {
                var bodyDirection = Body.GetOrientation().ToDirection();
                //if (Body.GetCollisionShape().LinearVelocity.Length() < 625.0f)
                //{
                    Body.GetCollisionShape().LinearVelocity += bodyDirection.ToBepu() * 14.0f;
                //}
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                Body.GetCollisionShape().LinearVelocity *= 0.985f;
            }
            float rollMult = 25.0f;
            float pitchMult = 58.0f;
            if (keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                var left = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 4.35f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + right, down * damping * rollMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + left, up * damping * rollMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                var left = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 4.35f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + right, up * damping * rollMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + left, down * damping * rollMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.J))
            {
                var forward = Body.GetOrientation().ToDirection();
                var backward = -Body.GetOrientation().ToDirection();
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + forward, up * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + backward, down * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.U))
            {
                var forward = Body.GetOrientation().ToDirection();
                var backward = -Body.GetOrientation().ToDirection();
                var down = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + backward, up * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + forward, down * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.H))
            {
                var left = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                var forward = Body.GetOrientation().ToDirection();
                var backward = -Body.GetOrientation().ToDirection();
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + backward, left * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + forward, right * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.K))
            {
                var left = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                var forward = Body.GetOrientation().ToDirection();
                var backward = -Body.GetOrientation().ToDirection();
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + backward, right * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + forward, left * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.Number0))
            {
                Mode = CameraMode.BehindTowardsVelocity;
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.Number1))
            {
                Mode = CameraMode.Free;
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.Number2))
            {
                Mode = CameraMode.StrictBehind;
            }
            if (Mode == CameraMode.Free)
            {
                if (keyboard.IsKeyDown(OpenTK.Input.Key.Up))
                {
                    var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Camera.Current.Pitch);
                    var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Camera.Current.Roll);
                    Vector4 direction = Vector4.UnitZ;
                    direction = Vector4.Transform(direction, rotationY);
                    direction = Vector4.Transform(direction, rotationX);
                    Camera.Current.Position -= direction.Xyz;
                    Camera.Current.UpdateFromRollPitch();
                }
                if (keyboard.IsKeyDown(OpenTK.Input.Key.Down))
                {
                    var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Camera.Current.Pitch);
                    var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Camera.Current.Roll);
                    Vector4 direction = -Vector4.UnitZ;
                    direction = Vector4.Transform(direction, rotationY);
                    direction = Vector4.Transform(direction, rotationX);
                    Camera.Current.Position -= direction.Xyz;
                    Camera.Current.UpdateFromRollPitch();
                }
                if (keyboard.IsKeyDown(OpenTK.Input.Key.Left))
                {
                    var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Camera.Current.Pitch);
                    var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Camera.Current.Roll);
                    Vector4 direction = Vector4.UnitX;
                    direction = Vector4.Transform(direction, rotationY);
                    direction = Vector4.Transform(direction, rotationX);
                    Camera.Current.Position -= direction.Xyz;
                    Camera.Current.UpdateFromRollPitch();
                }
                if (keyboard.IsKeyDown(OpenTK.Input.Key.Right))
                {
                    var rotationX = Quaternion.FromAxisAngle(Vector3.UnitY, -Camera.Current.Pitch);
                    var rotationY = Quaternion.FromAxisAngle(Vector3.UnitX, -Camera.Current.Roll);
                    Vector4 direction = -Vector4.UnitX;
                    direction = Vector4.Transform(direction, rotationY);
                    direction = Vector4.Transform(direction, rotationX);
                    Camera.Current.Position -= direction.Xyz;
                    Camera.Current.UpdateFromRollPitch();
                }
            }
        }

    }
}
