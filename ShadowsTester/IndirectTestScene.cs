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
    public class IndirectTestScene : Scene
    {
        public IndirectTestScene()
        {
            var obj = Object3dInfo.LoadFromObjSingle(Media.Get("indirect.obj"));
            obj.MakeDoubleFaced();
            Mesh3d mesh = new Mesh3d(obj, new GenericMaterial(Color.White));
            mesh.Transformation.SetScale(0.2f);
           // mesh.SetMass(0);
           // mesh.SetCollisionShape(obj.GetAccurateCollisionShape());
            Add(mesh);
        }

    }
}
