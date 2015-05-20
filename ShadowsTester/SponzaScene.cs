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
    public class SponzaScene : Scene
    {
        public SponzaScene()
        {
          /*  Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-2048, -2048), new Vector2(2048, 2048), new Vector2(496, 496), Vector3.UnitY);
            var waterMat = new SolidColorMaterial(new Vector4(0.55f, 0.74f, 0.97f, 1.0f));
            waterMat.SetNormalMapFromMedia("waternormal.png");
            var water = new Mesh3d(waterInfo, waterMat);
            water.Transformation.Translate(0, 1, 0);
            //water.DisableDepthWrite = true;
            Add(water);*/
            var scene = Object3dInfo.LoadSceneFromObj(Media.Get("sibenik.obj"), Media.Get("sibenik.mtl"), 1f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in scene)
            {
                //ob.SetMass(0);
                // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                ob.Translate(0, 10, 0);
                ob.MainObjectInfo.MakeDoubleFaced();
                this.Add(ob);
            }
        }

    }
}
