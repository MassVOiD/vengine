using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BulletSharp;

namespace Tester
{
    class Helicopter : IRenderable
    {
        Mesh3d rotor, body;
        public float RotorSpeed = 0.1f;
        float Rotor_YRotation = 0;
        bool BuffersGenerated = false;
        Object3dInfo blackhawk_body_info, blackhawk_rotor_info;
        public Helicopter()
        {
            blackhawk_body_info = Object3dInfo.LoadFromObj(Media.Get("blackhawk_body.obj"));
            blackhawk_rotor_info = Object3dInfo.LoadFromObj(Media.Get("blackhawk_rotor.obj"));

            blackhawk_rotor_info.GenerateBuffers();
            blackhawk_body_info.GenerateBuffers();

            body = new Mesh3d(blackhawk_body_info, new SolidColorMaterial(Color.Blue));
            body.UpdateMatrix();

            rotor = new Mesh3d(blackhawk_rotor_info, new SolidColorMaterial(Color.Red));
            rotor.UpdateMatrix();

            GLThread.OnUpdate += OnUpdate;
        }

        public void Delete()
        {
            GLThread.OnUpdate -= OnUpdate;
        }

        void OnUpdate(object sender, EventArgs e)
        {
            Rotor_YRotation += RotorSpeed;
            if (Rotor_YRotation > MathHelper.TwoPi) Rotor_YRotation -= MathHelper.TwoPi;
            rotor.SetPosition(body.GetPosition());

            Quaternion rotor_new_rotation = body.GetOrientation() * Quaternion.FromAxisAngle(Vector3.UnitY, Rotor_YRotation);

            rotor.SetOrientation(rotor_new_rotation);
        }

        public void Draw()
        {
            if (!BuffersGenerated)
            {
                BuffersGenerated = true;
            }
            body.Draw();
            rotor.Draw();
        }
    }
}
