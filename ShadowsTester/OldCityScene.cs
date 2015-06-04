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
                //(ob.MainMaterial as AbsMaterial).Type = AbsMaterial.MaterialType.WetDrops;
                (ob.MainMaterial as AbsMaterial).BumpMap = null;
                this.Add(ob);
            }
            //var protagonist = Object3dInfo.LoadSceneFromObj(Media.Get("protagonist.obj"), Media.Get("protagonist.mtl"), 1.0f);
            //foreach(var o in protagonist)
            //    Add(o);
             /*
            var fountainWaterObj = Object3dInfo.LoadFromObjSingle(Media.Get("cylinder.obj"));
            var water = new Mesh3d(fountainWaterObj, new GenericMaterial(new Vector4(1, 1, 0, 1)));
            water.DisableDepthWrite = true;
            water.UseAlphaMaskFromMedia("alphamask.png");
            water.Transformation.Scale(1.0f);
            Add(water);*/

            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY, 333, (x, y) => 0);


            var color = new GenericMaterial(Color.Green);
            color.SetBumpMapFromMedia("grassbump.png");
            color.Type = AbsMaterial.MaterialType.Grass;
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.Translate(0, 1, 0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            Add(water);

            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucymidres.vbo.raw"), Media.Get("lucymidres.indices.raw"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(Color.White));
            //dragon.Translate(0, 0, 20);
            dragon.Scale(80);
            Add(dragon);

           

        }

    }
}
