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
    public class HallScene : Scene
    {
        public HallScene()
        {
            var testroom = Object3dInfo.LoadSceneFromObj(Media.Get("testroom2.obj"), Media.Get("testroom2.mtl"), 1.0f);
           // var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in testroom)
            {
               /* if(ob.ObjectInfo.GetAxisAlignedBox().Length > 20.0f)
                {
                    ob.SetMass(0);
                    ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                }
                else
                {
                    ob.SetMass(3.0f);
                    ob.SetCollisionShape(ob.ObjectInfo.GetConvexHull());
                }*/
                ob.Translate(0, 0.5f, 0);
                this.Add(ob);
            }
        }

    }
}
