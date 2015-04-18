using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VDGTech;
using VDGTech.Generators;
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
                //ob.SetMass(0);
               // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                this.Add(ob);
            }
            //var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("lightsphere.obj"));
            dragon3dInfo.ScaleUV(100);
            var dragon = new Mesh3d(dragon3dInfo, new SolidColorMaterial(new Vector4(1, 1, 1, 0.1f)));
            //var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("180.JPG", "180_norm.JPG"));
            dragon.Transformation.Scale(2);
            dragon.Translate(0, 5, 0);
            //dragon.DrawOddOnly = true;
            dragon.DisableDepthWrite = true;
            dragon.DiffuseComponent = 0.5f;
            dragon.SpecularSize = 28.0f;
            Add(dragon);
            
        }

    }
}
