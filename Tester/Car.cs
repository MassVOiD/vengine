using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BulletSharp;

namespace Tester
{
    class Car : IRenderable
    {
        Mesh3d Body;
        Mesh3d[] Wheels;

        Object3dInfo blackhawk_body_info, blackhawk_rotor_info;

        public Car()
        {
            blackhawk_body_info = Object3dInfo.LoadFromObj(Media.Get("blackhawk_body.obj"));
            blackhawk_rotor_info = Object3dInfo.LoadFromObj(Media.Get("blackhawk_rotor.obj"));
/*
            body = new Mesh3d(blackhawk_body_info, new SolidColorMaterial(Color.Blue));
            body.UpdateMatrix();

            rotor = new Mesh3d(blackhawk_rotor_info, new SolidColorMaterial(Color.Red));
            rotor.UpdateMatrix();
            */
            GLThread.OnUpdate += OnUpdate;
        }

        public void Delete()
        {
            GLThread.OnUpdate -= OnUpdate;
        }

        void OnUpdate(object sender, EventArgs e)
        {
           
        }

        public void Draw()
        {
          /*  if (!BuffersGenerated)
            {
                blackhawk_rotor_info.GenerateBuffers();
                blackhawk_body_info.GenerateBuffers();
                BuffersGenerated = true;
            }
            body.Draw();
            rotor.Draw();*/
        }
    }
}
