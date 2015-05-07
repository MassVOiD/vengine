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
    public class BoxesScene : Scene
    {
        public BoxesScene()
        {
            var obj = Object3dInfo.LoadFromObjSingle(Media.Get("head.obj"));
            //obj.MakeDoubleFaced();
            var mat = SingleTextureMaterial.FromMedia("lambertian.jpg");
            //mat.SetBumpMapFromMedia("bumplow.png");
            var head = new Mesh3d(obj, mat);
            head.SpecularComponent = 0f;
            head.Transformation.Translate(new Vector3(30, 0, 0));
            Add(head);
        }

    }
}
