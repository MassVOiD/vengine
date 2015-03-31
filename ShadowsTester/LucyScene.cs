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
            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            var dragon = new Mesh3d(dragon3dInfo, new SolidColorMaterial(Color.White));
            Add(dragon);
        }

    }
}
