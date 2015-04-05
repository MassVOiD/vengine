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
            Random rand = new Random();
            InstancedMesh3d im = new InstancedMesh3d(ballobj, new SolidColorMaterial(Color.White)
            {
                Color = new Vector4((float)rand.NextDouble() * 80.0f, (float)rand.NextDouble() * 80.0f, (float)rand.NextDouble() * 80.0f, 1)
            });
            for(float x = -130; x < 130; x += 10)
            {
                for(float y = -130; y < 130; y += 10)
                {
                    for(float z = 0; z < 30; z += 10)
                    {

                        im.Transformations.Add(new TransformationManager(new Vector3(x, z, y)));
                        im.Instances++;
                    }
                }
            }
            im.UpdateMatrix();
            Add(im);


        }

    }
}
