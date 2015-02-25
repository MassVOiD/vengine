using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BEPUphysics.Entities.Prefabs;

namespace ShadowsTester
{
    partial class Airplane : IRenderable, IPhysical
    {
        public Mesh3d Body;

        Object3dInfo Body3dInfo;
        CameraMode Mode;
        MeshLinker.LinkInfo Link;
        public Airplane()
        {
            Body3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("helibody.obj"));
            Object3dInfo motor3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("helimotor.obj"));
            Mode = CameraMode.BehindTowardsVelocity;
            Body = new Mesh3d(Body3dInfo, new SolidColorMaterial(Color.Cyan));
            Body.SetMass(1.0f);
            Body.SetPosition(new Vector3(20, 20, 0));
            Body.SetCollisionShape(new Box(Body.GetPosition(), 10,2,10, 1110.0f));
            Body.SetOrientation(Quaternion.FromAxisAngle(new Vector3(1, 1, 1), 0.4f));
            Body.GetCollisionShape().PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;

            Mesh3d motor = new Mesh3d(motor3dInfo, new SolidColorMaterial(Color.Red));
            Link = MeshLinker.Link(Body, motor, new Vector3(0, 0, 0), Quaternion.Identity);

            GLThread.OnUpdate += OnUpdate;
            GLThread.OnBeforeDraw += GLThread_OnBeforeDraw;
            GLThread.OnMouseMove += GLThread_OnMouseMove;
        }

        void GLThread_OnMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
        {
            /*if (Mode == CameraMode.Free)
            {
                Camera.Current.Pitch += (float)e.XDelta / 100.0f;
                if (Camera.Current.Pitch > MathHelper.TwoPi) Camera.Current.Pitch = 0.0f;

                Camera.Current.Roll += (float)e.YDelta / 100.0f;
                if (Camera.Current.Roll > MathHelper.Pi / 2) Camera.Current.Roll = MathHelper.Pi / 2;
                if (Camera.Current.Roll < -MathHelper.Pi / 2) Camera.Current.Roll = -MathHelper.Pi / 2;

                Camera.Current.UpdateFromRollPitch();
            }*/
        }

        enum CameraMode
        {
            Free,
            StrictBehind,
            BehindNoRoll,
            BehindTowardsVelocity
        }
        
        void GLThread_OnBeforeDraw(object sender, EventArgs e)
        {
            Link.Rotation = Quaternion.Multiply(Link.Rotation, Quaternion.FromAxisAngle(Vector3.UnitY, 0.1f));
        }

        public void Delete()
        {
            GLThread.OnUpdate -= OnUpdate;
        }
        public void Draw()
        {
            Body.Draw();
            Link.Child.Draw();
        }

    }
}
