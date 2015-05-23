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
            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucymidres.vbo.raw"), Media.Get("lucymidres.indices.raw"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, GenericMaterial.FromMedia("180.jpg", "180_norm.jpg"));
            dragon.Translate(0, 0, 20);
            dragon.Scale(80);
            Add(dragon);
        }

    }
}
