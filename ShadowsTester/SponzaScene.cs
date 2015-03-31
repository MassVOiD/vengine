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
    public class SponzaScene : Scene
    {
        public SponzaScene()
        {
            var obj = Object3dInfo.LoadFromRaw(Media.Get("sponza.vbo.raw"), Media.Get("sponza.indices.raw"));
            Mesh3d mesh = new Mesh3d(obj, new SolidColorMaterial(Color.White));
            mesh.Transformation.SetScale(0.3f);
           // mesh.SetMass(0);
           // mesh.SetCollisionShape(obj.GetAccurateCollisionShape());
            Add(mesh);
        }

    }
}
