using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;

namespace ShadowsTester
{
    partial class Airplane : IRenderable, IPhysical
    {
        
        void UpdateSterring()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            var bodyDirection = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
           // var gmepad = OpenTK.Input.GamePad.GetState(0);

            if (keyboard.IsKeyDown(OpenTK.Input.Key.W))
            {
                //if (Body.GetCollisionShape().LinearVelocity.Length() < 625.0f)
                //{
                    Body.GetCollisionShape().LinearVelocity += bodyDirection.ToBepu() * 1.0f;
                //}
            }
            Body.GetCollisionShape().LinearVelocity += bodyDirection.ToBepu() * 0.5f;
            if (keyboard.IsKeyDown(OpenTK.Input.Key.S))
            {
                var bodyDirectionDown = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                //if (Body.GetCollisionShape().LinearVelocity.Length() < 625.0f)
                //{
                Body.GetCollisionShape().LinearVelocity += bodyDirectionDown.ToBepu() * 1.0f;
            }
            float rollMult = 35.0f;
            float pitchMult = 45.0f;
            if (keyboard.IsKeyDown(OpenTK.Input.Key.A))
            {
                var down = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                var left = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 4.35f;
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + right, down * damping * rollMult);
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + left, up * damping * rollMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.D))
            {
                var down = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                var left = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 4.35f;
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + right, up * damping * rollMult);
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + left, down * damping * rollMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.J))
            {
                var forward = Body.Transformation.GetOrientation().ToDirection();
                var backward = -Body.Transformation.GetOrientation().ToDirection();
                var down = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + forward, up * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + backward, down * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.U))
            {
                var forward = Body.Transformation.GetOrientation().ToDirection();
                var backward = -Body.Transformation.GetOrientation().ToDirection();
                var down = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Down);
                var up = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Up);
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + backward, up * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + forward, down * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.H))
            {
                var left = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                var forward = Body.Transformation.GetOrientation().ToDirection();
                var backward = -Body.Transformation.GetOrientation().ToDirection();
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + backward, left * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.GetPosition() + forward, right * damping * pitchMult);
            }
            if (keyboard.IsKeyDown(OpenTK.Input.Key.K))
            {
                var left = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left);
                var right = Body.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Right);
                var forward = Body.Transformation.GetOrientation().ToDirection();
                var backward = -Body.Transformation.GetOrientation().ToDirection();
                float damping = 1.0f / (Body.GetCollisionShape().LinearVelocity.Length() + 0.13f) + 0.85f;
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + backward, right * damping * pitchMult);
                Body.GetCollisionShape().ApplyImpulse(Body.Transformation.GetPosition() + forward, left * damping * pitchMult);
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
                if(Camera.Current != Program.FreeCam.Cam)
                    Camera.Current = Program.FreeCam.Cam;
            }
        }

    }
}
