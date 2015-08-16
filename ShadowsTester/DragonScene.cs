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
    public class DragonScene : Scene
    {
        public DragonScene()
        {
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(300);
            whitebox.Translate(0,-2,0);
            Add(whitebox);
            var whiteboxInfo2 = Object3dInfo.LoadFromObjSingle(Media.Get("reflector.obj"));
            //whiteboxInfo2.ScaleUV(33.0f);
            var whitebox2 = new Mesh3d(whiteboxInfo2, new GenericMaterial(Color.White));
            //whitebox2.MainMaterial.SetBumpMapFromMedia("cobblestone.jpg");
            whitebox2.Scale(2);
            whitebox2.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90)));
            Add(whitebox2);
            var lod1 = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            lod1.ScaleUV(8.0f);
            var chairInfo = Object3dInfo.LoadFromObjSingle(Media.Get("nicechair.obj"));
            var chair = new Mesh3d(lod1, GenericMaterial.FromMedia("168.JPG", "168_norm.JPG", "168_norm.JPG"));
            chair.MainMaterial.Roughness = 0.9f;
            //chair.MainMaterial.SetNormalMapFromMedia("clothnorm.png");
            chair.MainMaterial.ReflectionStrength = 1.0f;
            Add(chair);/*
            var scene = Object3dInfo.LoadSceneFromObj(Media.Get("desertcity.obj"), Media.Get("desertcity.mtl"), 1.0f);
            foreach(var ob in scene)
            {
                ob.SetMass(0);
                this.Add(ob);
            }*/
        }

    }
}
