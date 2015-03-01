using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;

namespace VDGTech
{
    public class Line2d
    {
        public Vector3 Start, End;
        public Color Colour;
        public Line2d(Vector3 start, Vector3 end, Color color)
        {
            Start = start;
            End = end;
            Colour = color;
        }
    }
}
