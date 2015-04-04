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
    public class LucyScene : Scene
    {
        public LucyScene()
        {
            /*
            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucyhires.vbo.raw"), Media.Get("lucyhires.indices.raw"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("180.jpg", "180_norm.jpg"));
            Add(dragon);*/
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("head.obj"));
            var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("lambertian.jpg", "bumplownormal2.png"));
            dragon.SpecularComponent = 0.1f;
            dragon.SpecularSize = 2.1f;
            Add(dragon);

        }

    }
}
