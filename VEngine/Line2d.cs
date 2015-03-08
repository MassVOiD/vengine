using System.Drawing;
using OpenTK;

namespace VDGTech
{
    public class Line2d
    {
        public Line2d(Vector3 start, Vector3 end, Color color)
        {
            Start = start;
            End = end;
            Colour = color;
        }

        public Color Colour;
        public Vector3 Start, End;
    }
}