using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEngine.PathTracing;
using OpenTK;

namespace VEngine.PathTracing
{
    class SceneOctalTree
    {
        private const int MAX_LEAF_TRIANGLES = 40;

        // debugging
        public static int TotalNodes = 0;
        public static int TotalLeaves = 0;

        class Box
        {
            public Vector3 Center;
            public float Radius;
            public List<Triangle> Triangles;
            public List<Box> Children;

            public Box(Vector3 center, float radius)
            {
                Center = center;
                Radius = radius;
                Triangles = new List<Triangle>();
                Children = new List<Box>();
                TotalNodes++;
                TotalLeaves++;
            }

            public bool TestPoint(Vector3 point)
            {
                if(point.X >= Center.X - Radius && point.X <= Center.X + Radius &&
                    point.Y >= Center.Y - Radius && point.Y <= Center.Y + Radius &&
                    point.Z >= Center.Z - Radius && point.Z <= Center.Z + Radius)
                    return true;
                else
                    return false;
            }

            public bool TestTriangle(Triangle triangle)
            {
                foreach(var vertex in triangle.Vertices) if(TestPoint(vertex.Position))
                        return true;
                return false;
            }

            public void CheckSanity()
            {
                // this is because im stupid
                if(Children.Count > 0 && Triangles.Count > 0)
                    throw new Exception("Octree node cannot contain both nodes and leaves");
            }

            public void Flatten()
            {
                if(Children.Count == 1)
                {
                    Triangles = Children[0].Triangles;
                    Children = Children[0].Children;
                    CheckSanity();
                }
            }

            public void RecursiveDivide(int iterationLimit = 15)
            {
                CheckSanity();
                if(Triangles.Count < MAX_LEAF_TRIANGLES || iterationLimit <= 0)
                    return;

                var newBoxes = new List<Box>();
                float rd2 = Radius / 2;

                newBoxes.Add(new Box(Center + new Vector3(rd2, rd2, rd2), rd2));
                newBoxes.Add(new Box(Center + new Vector3(-rd2, rd2, rd2), rd2));
                newBoxes.Add(new Box(Center + new Vector3(rd2, -rd2, rd2), rd2));
                newBoxes.Add(new Box(Center + new Vector3(rd2, rd2, -rd2), rd2));

                newBoxes.Add(new Box(Center + new Vector3(-rd2, -rd2, rd2), rd2));
                newBoxes.Add(new Box(Center + new Vector3(rd2, -rd2, -rd2), rd2));
                newBoxes.Add(new Box(Center + new Vector3(-rd2, rd2, -rd2), rd2));
                newBoxes.Add(new Box(Center + new Vector3(-rd2, -rd2, -rd2), rd2));

                foreach(var b in newBoxes)
                {
                    foreach(var t in Triangles)
                    {
                        if(b.TestTriangle(t))
                        {
                            b.Triangles.Add(t);
                        }
                    }
                    b.RecursiveDivide(iterationLimit - 1);
                }
                newBoxes = newBoxes.Where((b) => b.Children.Count > 0 || b.Triangles.Count > 0).ToList();
                TotalNodes -= 8 - newBoxes.Count;
                Triangles.Clear();
                TotalNodes--;
                Children = newBoxes;
                Flatten();
                CheckSanity();
            }
        } // here box class ends


        Box BoxTree;

        public SceneOctalTree()
        {
        }

        private float Max(float a, float b)
        {
            return a > b ? a : b;
        }
        private float Min(float a, float b)
        {
            return a < b ? a : b;
        }
        private float Abs(float a)
        {
            return a < 0 ? -a : a;
        }

        public void CreateFromTriangleList(List<Triangle> triangles)
        {
            Vector3 max = new Vector3(0), min = new Vector3(0);
            foreach(var t in triangles)
            {
                foreach(var v in t.Vertices)
                {
                    max.X = Max(max.X, v.Position.X);
                    max.Y = Max(max.Y, v.Position.Y);
                    max.Z = Max(max.Z, v.Position.Z);

                    min.X = Min(min.X, v.Position.X);
                    min.Y = Min(min.Y, v.Position.Y);
                    min.Z = Min(min.Z, v.Position.Z);
                }
            }
            Vector3 center = (max + min) / 2;
            float radius =
                Max(
                    Max(
                        Abs(min.X - max.X), Abs(min.Y - max.Y))
                    , Abs(min.Z - max.Z));
            BoxTree = new Box(center, radius);
            BoxTree.Triangles = triangles;
            BoxTree.RecursiveDivide();

            Console.WriteLine(TotalNodes);
            Console.WriteLine(TotalLeaves);
        }

    }
}
