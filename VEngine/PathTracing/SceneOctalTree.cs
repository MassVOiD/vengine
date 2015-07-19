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
        private const int MAX_LEAF_TRIANGLES = 150;

        // debugging
        public static int TotalNodes = 0;
        public static int TotalLeaves = 0;

        public class Box
        {
            public Vector3 Center;
            public float Radius;
            public List<Triangle> Triangles;
            public List<Box> Children;
            public Box Parent = null;

            public Box(Vector3 center, float radius)
            {
                Center = center;
                Radius = radius;
                Triangles = new List<Triangle>();
                Children = new List<Box>();
                TotalNodes++;
                TotalLeaves++;
            }

            private float Max(float a, float b)
            {
                return a > b ? a : b;
            }
            private float Min(float a, float b)
            {
                return a < b ? a : b;
            }
            private Vector3 Max(Vector3 a, Vector3 b)
            {
                return new Vector3(
                    Max(a.X, b.X),
                    Max(a.Y, b.Y),
                    Max(a.Z, b.Z)
                );
            }
            private Vector3 Min(Vector3 a, Vector3 b)
            {
                return new Vector3(
                    Min(a.X, b.X),
                    Min(a.Y, b.Y),
                    Min(a.Z, b.Z)
                );
            }
            private Vector3 Divide(Vector3 a, Vector3 b)
            {
                return new Vector3(
                    a.X / b.X,
                    a.Y / b.Y,
                    a.Z / b.Z
                );
            }
            private float Abs(float a)
            {
                return a < 0 ? -a : a;
            }
            private Vector3 Cross(Vector3 a, Vector3 b)
            {
                return Vector3.Cross(a, b);
            }
            private float Dot(Vector3 a, Vector3 b)
            {
                return Vector3.Dot(a, b);
            }


            Vector2 IntersectBox(Vector3 origin, Vector3 direction, Vector3 min, Vector3 max)
            {
                Vector3 tMin = Divide((min - origin), direction);
                Vector3 tMax = Divide((max - origin), direction);
                Vector3 t1 = Min(tMin, tMax);
                Vector3 t2 = Max(tMin, tMax);
                float tNear = Max(Max(t1.X, t1.Y), t1.Z);
                float tFar = Min(Min(t2.X, t2.Y), t2.Z);
                return new Vector2(tFar, tNear);
            }

            float IntersectTriangle(Vector3 origin, Vector3 direction, Vector3[] vertices)
            {
                Vector3 e0 = vertices[1] - vertices[0];
                Vector3 e1 = vertices[2] - vertices[0];

                Vector3 h = Cross(direction, e1);
                float a = Dot(e0, h);

                float f = 1.0f / a;

                Vector3 s = origin - vertices[0];
                float u = f * Dot(s, h);

                Vector3 q = Cross(s, e0);
                float v = f * Dot(direction, q);

                Vector3 incidentPosition = vertices[0] + (vertices[1] - vertices[0]) * u + (vertices[2] - vertices[0]) * v;

                float t = f * Dot(e1, q);

                return t > 0.0 && t < float.PositiveInfinity &&
                u >= 0.0 && u <= 1.0 &&
                v >= 0.0 && u + v <= 1.0 &&
                t >= float.Epsilon ? (origin - incidentPosition).Length : 0;
            }

            class Ray
            {
                public Vector3 Origin, Direction;
                public float EstimatedLength = float.PositiveInfinity;
                public Ray(Vector3 origin, Vector3 direction)
                {
                    Origin = origin;
                    Direction = direction.Normalized();
                }
                public Ray(Vector3 origin, Vector3 direction, float estimatedLength)
                {
                    Origin = origin;
                    Direction = direction.Normalized();
                    EstimatedLength = estimatedLength;
                }
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
                
                // a bad part begins here
                // intersect every box edge with a triangle and
                // intersect every triangle edge with box
                    
                // triangle edges vs box part
                List<Ray> triangleRays = new List<Ray>();

                /*
                 * 0 1
                 * 0 2
                 * 1 0
                 * 1 2
                 * 2 0
                 * 2 1
                 */

                triangleRays.Add(new Ray(triangle.Vertices[0].Position,
                    triangle.Vertices[0].Position - triangle.Vertices[1].Position,
                    (triangle.Vertices[0].Position - triangle.Vertices[1].Position).Length));

                triangleRays.Add(new Ray(triangle.Vertices[0].Position,
                    triangle.Vertices[0].Position - triangle.Vertices[2].Position,
                    (triangle.Vertices[0].Position - triangle.Vertices[2].Position).Length));

                triangleRays.Add(new Ray(triangle.Vertices[1].Position,
                    triangle.Vertices[1].Position - triangle.Vertices[0].Position,
                    (triangle.Vertices[1].Position - triangle.Vertices[0].Position).Length));

                triangleRays.Add(new Ray(triangle.Vertices[1].Position,
                    triangle.Vertices[1].Position - triangle.Vertices[2].Position,
                    (triangle.Vertices[1].Position - triangle.Vertices[2].Position).Length));

                triangleRays.Add(new Ray(triangle.Vertices[2].Position,
                    triangle.Vertices[2].Position - triangle.Vertices[0].Position,
                    (triangle.Vertices[2].Position - triangle.Vertices[0].Position).Length));

                triangleRays.Add(new Ray(triangle.Vertices[2].Position,
                    triangle.Vertices[2].Position - triangle.Vertices[1].Position,
                    (triangle.Vertices[2].Position - triangle.Vertices[1].Position).Length));

                Vector3 bMin = Center - new Vector3(Radius);
                Vector3 bMax = Center + new Vector3(Radius);

                // since NO origin will be in a box there should be 2 or 0 intersections
                foreach(var r in triangleRays)
                {
                    Vector2 ires = IntersectBox(r.Origin, r.Direction, bMin, bMax);
                    if(ires.X >= ires.Y && ires.Y >= -r.EstimatedLength && ires.Y <= r.EstimatedLength)
                        return true;
                }
                
                List<Ray> boxEdgesRays = new List<Ray>();

                boxEdgesRays.Add(new Ray(bMin,
                    new Vector3(Radius * 2, 0, 0),
                    Radius * 2));
                boxEdgesRays.Add(new Ray(bMin,
                    new Vector3(0, Radius * 2, 0),
                    Radius * 2));
                boxEdgesRays.Add(new Ray(bMin,
                    new Vector3(0, 0, Radius * 2),
                    Radius * 2));

                boxEdgesRays.Add(new Ray(bMax,
                    new Vector3(Radius * 2, 0, 0),
                    Radius * 2));
                boxEdgesRays.Add(new Ray(bMax,
                    new Vector3(0, Radius * 2, 0),
                    Radius * 2));
                boxEdgesRays.Add(new Ray(bMax,
                    new Vector3(0, 0, Radius * 2),
                    Radius * 2));

                foreach(var r in boxEdgesRays)
                {
                    float ires = IntersectTriangle(r.Origin, r.Direction, triangle.Vertices.Select<Vertex, Vector3>((a) => a.Position).ToArray());
                    if(ires > 0 && ires <= r.EstimatedLength)
                        return true;
                }

                return false;
            }

            public void CheckSanity()
            {
                // this is because im stupid
                if(Children.Count > 0 && Triangles.Count > 0)
                    throw new Exception("Octree node cannot contain both nodes and leaves");
                if(Children.Contains(this))
                    throw new Exception("Octree node contains itself");
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

            public void RecursiveDivide(int iterationLimit = 5)
            {
                CheckSanity();
                Console.WriteLine(Triangles.Count);
                if(Triangles.Count < MAX_LEAF_TRIANGLES || iterationLimit <= 0)
                    return;

                var newBoxes = new List<Box>();
                float rd2 = Radius / 2.0f;
                //if(rd2 < 0.3f)
                //    return;

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
                if(newBoxes.Count > 0)
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
            TrianglesIds = new Dictionary<Triangle, int>();
            int i = 0;
            foreach(var t in triangles)
                TrianglesIds.Add(t, i++);

            Vector3 max = new Vector3(0), min = new Vector3(float.PositiveInfinity);
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
                    Max(max.X - min.X, max.Y - min.Y)
                    , max.Z - min.Z) / 2;
            BoxTree = new Box(center, radius);
            BoxTree.Triangles = triangles;
            BoxTree.RecursiveDivide();

            Console.WriteLine(TotalNodes);
            Console.WriteLine(TotalLeaves);
        }

        List<int> ContainerIndices;
        Dictionary<Triangle, int> TrianglesIds;
        Dictionary<Box, int> BoxesIds;
        List<byte> SerializerBytes;
        int BoxCursor = 0;
        Queue<Box> SerializationQueue;
        List<Box> FlatBoxList;
        public int TotalBoxesCount = 0;

        public void Serialize()
        {
            ContainerIndices = new List<int>();
            SerializerBytes = new List<byte>();
            BoxesIds = new Dictionary<Box, int>();
            FlatBoxList = new List<Box>();
            BoxCursor = 0;
            TotalBoxesCount = 0;
            CreateList(BoxTree);
            SortListByDepth();
            SetBoxIDOrdered(BoxTree);
            foreach(var b in FlatBoxList)
            {
                Serialize(b);
            }
        }

        public void CreateList(Box box)
        {
            FlatBoxList.Add(box);
            foreach(var c in box.Children)
                CreateList(c);
        }
        public void SortListByDepth()
        {
            FlatBoxList = FlatBoxList.OrderBy((a) =>
            {
                // determine depth
                Box cursor = a;
                int depth = 0;
                for(int i = 0; i < FlatBoxList.Count; i++)
                {
                    var b = FlatBoxList[i];
                    if(b.Children.Contains(cursor))
                    {
                        depth++;
                        cursor.Parent = b;
                        cursor = b;
                        i = 0;
                    }
                }
                return depth;
            }).ToList();
        }
        public void SetBoxIDOrdered(Box box)
        {
            foreach(var c in FlatBoxList)
                BoxesIds.Add(c, BoxCursor++);
        }

        public void Serialize(Box box)
        {
            if(box.Triangles.Count == 0)
                return;
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Center.X));
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Center.Y));
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Center.Z));
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Radius));

            for(int i = 0; i < 8; i++)
            {
                SerializerBytes.AddRange(BitConverter.GetBytes(i < box.Children.Count ? BoxesIds[box.Children[i]] : -1)); //  this is not obvious
            }
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Parent == null ? -1 : BoxesIds[box.Parent]));
            SerializerBytes.AddRange(BitConverter.GetBytes(ContainerIndices.Count));
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Triangles.Count));
            SerializerBytes.AddRange(BitConverter.GetBytes(box.Triangles.Count));
            foreach(var t in box.Triangles)
            {
                ContainerIndices.Add(TrianglesIds[t]);
            }
            TotalBoxesCount++;
        }

        public void PopulateSSBOs(ShaderStorageBuffer triangleStream, ShaderStorageBuffer boxes)
        {
            Serialize();
            while(ContainerIndices.Count % 4 != 0)
                ContainerIndices.Add(0);
            triangleStream.MapData(ContainerIndices.ToArray());
            boxes.MapData(SerializerBytes.ToArray());
        }


    }
}
