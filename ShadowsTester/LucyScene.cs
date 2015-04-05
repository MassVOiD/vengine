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
            var ballobj = Object3dInfo.LoadFromObjSingle(Media.Get("sphere.obj"));
            var ball1 = new Mesh3d(ballobj, new SolidColorMaterial(Color.Green));
            ball1.Transformation.Translate(new Vector3(0, 0, 10));
            Add(ball1);

            var ball2 = new Mesh3d(ballobj, new SolidColorMaterial(Color.White)
            {
                Color = new Vector4(10, 10, 10, 1)
            });
            ball2.SpecularSize = 0;
            Add(ball2);

            var ball3 = new Mesh3d(ballobj, new SolidColorMaterial(Color.Red));
            ball3.Transformation.Translate(new Vector3(0, 0, -10));
            Add(ball3);

        }

    }
}
