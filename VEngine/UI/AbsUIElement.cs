using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VDGTech.Generators;

namespace VDGTech.UI
{
    public abstract class AbsUIElement
    {
        public Vector2 Position;
        public Vector2 Size;
        public static Object3dInfo Info3d = Object3dGenerator.CreatePlane(new Vector3(-1, -1, 0), new Vector3(1, -1, 0),
            new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector2(-1, -1), Vector3.UnitZ);

        public abstract void Draw();
    }
}
