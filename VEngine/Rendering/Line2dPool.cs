using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VEngine
{
    public class Line2dPool : List<Line2d>
    {
        public Vector3[] GetColors()
        {
            return this.Select<Line2d, Vector3>(a => new Vector3((float)a.Colour.R / 255.0f, (float)a.Colour.G / 255.0f, (float)a.Colour.B / 255.0f)).ToArray();
        }

        public Vector3[] GetEndsVectors()
        {
            return this.Select<Line2d, Vector3>(a => a.End).ToArray();
        }

        public Vector3[] GetStartsVectors()
        {
            return this.Select<Line2d, Vector3>(a => a.Start).ToArray();
        }
    }
}