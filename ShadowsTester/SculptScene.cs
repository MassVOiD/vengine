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
            var dragon3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("shadowtest.obj"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, new GenericMaterial(Color.WhiteSmoke));
            dragon.Translate(0, 0, 20);
            dragon.Scale(5);
            Add(dragon);
        }

    }
}
