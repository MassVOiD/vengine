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
    public class SculptScene : Scene
    {
        public SculptScene()
        {
            var obj = Object3dInfo.LoadFromObjSingle(Media.Get("typek.obj"));
            obj.ScaleUV(100, 100);
            Mesh3d mesh = new Mesh3d(obj, SingleTextureMaterial.FromMedia("160.JPG", "160_norm.JPG"));
            mesh.Transformation.SetScale(10.0f);
            mesh.DiffuseComponent = 0.3f;
            mesh.SpecularComponent = 1;
            mesh.SpecularSize = 1.2f;
            Add(mesh);
        }

    }
}
