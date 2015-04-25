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
    public class OldCityScene : Scene
    {
        public OldCityScene()
        {
            var scene = Object3dInfo.LoadSceneFromObj(Media.Get("cryteksponza.obj"), Media.Get("cryteksponza.mtl"), 0.03f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in scene)
            {
                ob.SetMass(0);
               // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                //ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
               // ob.Material = new SolidColorMaterial(new Vector4(1, 1, 1, 0.1f));
                this.Add(ob);
            }
            //var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            /*var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("drops.obj"));
            dragon3dInfo.ScaleUV(10);
            var dragon = new Mesh3d(dragon3dInfo, new SolidColorMaterial(new Vector4(1, 1, 1, 0.1f)));
            //var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("hearts.png"));
            dragon.Transformation.Scale(0.2f);
            //dragon.Translate(0, 5, 0);
            //dragon.DrawOddOnly = true;
            dragon.DisableDepthWrite = true;
            dragon.DiffuseComponent = 0.5f;
            dragon.SpecularSize = 28.0f;
            //dragon.SetCollisionShape(new BulletSharp.BoxShape(1.6308f*2));
            Add(dragon);
            */
             
            var fountainWaterObj = Object3dInfo.LoadFromObjSingle(Media.Get("glass.obj"));
            var water = new Mesh3d(fountainWaterObj, new SolidColorMaterial(new Vector4(10, 10, 10, 0.2f)));
            water.DisableDepthWrite = true;
            water.Transformation.Scale(0.4f);

            Add(water);

        }

    }
}
