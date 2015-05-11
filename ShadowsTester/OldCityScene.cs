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
                //ob.SetCollisionShape(ob.MainObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                ob.ReflectionStrength = 1;
                //ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
               // ob.Material = new SolidColorMaterial(new Vector4(1, 1, 1, 0.1f));
                this.Add(ob);
            }
            var protagonist = Object3dInfo.LoadSceneFromObj(Media.Get("protagonist.obj"), Media.Get("protagonist.mtl"), 1.0f);
            foreach(var o in protagonist)
                Add(o);
             /*
            var fountainWaterObj = Object3dInfo.LoadFromObjSingle(Media.Get("glass.obj"));
            var water = new Mesh3d(fountainWaterObj, new SolidColorMaterial(new Vector4(0, 0, 0, 0.02f)));
            water.DisableDepthWrite = true;
            water.Transformation.Scale(0.4f);

            Add(water);*/

        }

    }
}
