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
            var scene = Object3dInfo.LoadSceneFromObj(Media.Get("sponzasimple.obj"), Media.Get("sponzasimple.mtl"), 1f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in scene)
            {
                //ob.SetMass(0);
                // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                ob.Translate(0, 10, 0);
                //ob.MainObjectInfo.MakeDoubleFaced();
                this.Add(ob);
            }
        }

    }
}
