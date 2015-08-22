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
    public class HomeScene : Scene
    {
        public HomeScene()
        {
            var testroom = Object3dInfo.LoadSceneFromObj(Media.Get("houses.obj"), Media.Get("houses.mtl"), 3.0f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in testroom)
            {
                ob.SetMass(0);
                this.Add(ob);
            }
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(300);
            whitebox.Translate(0, -2, 0);
            Add(whitebox);
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY, 333, (x, y) => 0);
            var color = GenericMaterial.FromMedia("checked.png");
            //color.SetBumpMapFromMedia("lightref.png");
            color.Roughness = 1.0f;
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            //water.Translate(0, -0.941f * 2.0f, 0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            Add(water);
        }

    }
}
