using OpenTK;
using VDGTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VDGTech.Generators
{
    public class Object3dGenerator
    {

        public static Object3dInfo CreateGround(Vector2 start, Vector2 end, Vector2 uvScale, Vector3 normal)
        {
            float[] VBO = {
                start.X, 0, end.Y, 0, 0, normal.X, normal.Y, normal.Z,
                end.X, 0, end.Y, 0, uvScale.Y, normal.X, normal.Y, normal.Z,
                start.X, 0, start.Y, uvScale.X, 0, normal.X, normal.Y, normal.Z,
                end.X, 0, start.Y, uvScale.X, uvScale.Y, normal.X, normal.Y, normal.Z,
            };
            uint[] indices = {
                0, 1, 2, 3, 2, 1
            };
            return new Object3dInfo(VBO.ToList(), indices.ToList());
        }
    }
}
