using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VEngine;
using VEngine.Generators;
using OpenTK;

namespace ShadowsTester
{
    public class SculptScene : Scene
    {
        public SculptScene()
        {
            var sun = new Sun(new Vector3(0.1f, -1, 0).ToQuaternion(Vector3.UnitY), new Vector4(1, 0.97f, 0.92f, 120), 100, 70, 10, 4, 1);
            GLThread.OnUpdate += (o, e) =>
            {
                var kb = OpenTK.Input.Keyboard.GetState();
                if(kb.IsKeyDown(OpenTK.Input.Key.U))
                {
                    var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), -0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.J))
                {
                    var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), 0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.H))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, -0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.K))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
            };
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("desertcity.obj"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(Color.WhiteSmoke));
            dragon.Translate(0, 0, 20);
            dragon.Scale(5);
            Add(dragon);
        }

    }
}
