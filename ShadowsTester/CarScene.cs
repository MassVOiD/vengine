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
    public class CarScene : Scene
    {
        public CarScene()
        {
            var obj = Object3dInfo.LoadFromObjSingle(Media.Get("carel.obj"));
            obj.OriginToCenter();
            obj.Normalize();
            var mesh = new Mesh3d(obj, new SolidColorMaterial(Color.Red));
            mesh.Transformation.SetScale(3.0f);
            mesh.Transformation.SetPosition(new Vector3(0, 10, 0));
            mesh.SetMass(100.0f);
            mesh.SetCollisionShape(new BulletSharp.BoxShape(obj.GetAxisAlignedBox() * 3.0f));
            mesh.SpecularComponent = 2.0f;
            mesh.DiffuseComponent = 0.2f;
            Add(mesh);
        }

    }
}
