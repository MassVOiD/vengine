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
    public class DragonScene : Scene
    {
        public DragonScene()
        {
            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("dragon.vbo.raw"), Media.Get("dragon.indices.raw"));
            var dragon = new Mesh3d(dragon3dInfo, new SolidColorMaterial(Color.White));
            Add(dragon);
        }

    }
}
