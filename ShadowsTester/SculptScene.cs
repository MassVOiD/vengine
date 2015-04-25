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
    public class SculptScene : Scene
    {
        public SculptScene()
        {
            var obj = Object3dInfo.LoadFromObjSingle(Media.Get("sculpture.obj"));
            obj.ScaleUV(100, 100);
            Mesh3d mesh = new Mesh3d(obj, SingleTextureMaterial.FromMedia("183.JPG", "183_norm.JPG"));
            //mesh.Transformation.SetScale(0.1f);
            //mesh.DiffuseComponent = 0.3f;
            //mesh.SpecularComponent = 1;
            //mesh.SpecularSize = 1.2f;
            mesh.Transformation.Translate(new Vector3(0, 0, 10));
            Add(mesh);

            var scale_1meterboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("1m_sized_cube.obj"));
            var box = new Mesh3d(scale_1meterboxInfo, SingleTextureMaterial.FromMedia("boxtex.png"));
            box.Transformation.Translate(new Vector3(0, scale_1meterboxInfo.GetAxisAlignedBox().Y, 0));
            Add(box);

            new HomeScene().Create();
        }

    }
}
