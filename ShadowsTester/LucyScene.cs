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
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("kamaz.obj"));
            var dragon = new Mesh3d(dragon3dInfo, new SolidColorMaterial(Color.PapayaWhip));
            dragon.Transformation.Scale(4);
            Add(dragon);
            
            /*
            var ballobj = Object3dInfo.LoadFromRaw(Media.Get("mustang.vbo.raw"), Media.Get("mustang.indices.raw"));
            ballobj.MakeDoubleFaced();

            var ball2 = new Mesh3d(ballobj, new SolidColorMaterial(Color.Red));
            ball2.Transformation.Scale(4);
            Add(ball2);
            */
            /*var ballobj = Object3dInfo.LoadFromObjSingle(Media.Get("sphere.obj"));
            Random rand = new Random();
            for(float x = -20; x < 20; x += 10)
            {
                for(float y = -20; y < 20; y += 10)
                {

                    var ball2 = new Mesh3d(ballobj, new SolidColorMaterial(Color.White)
                    {
                        Color = new Vector4((float)rand.NextDouble() * 60.0f, (float)rand.NextDouble() * 60.0f, (float)rand.NextDouble() * 60.0f, 1)
                    });
                    ball2.Transformation.Translate(new Vector3(x, 0, y));
                    Add(ball2);
                }
            }*/



        }

    }
}
