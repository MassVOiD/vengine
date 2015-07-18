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
        private const int MAX_LEAF_TRIANGLES = 48;

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
                if(newBoxes.Count > 0) Triangles.Clear();
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
            TotalBoxesCount = FlatBoxList.Count;
        }

        public void Serialize(Box box)
        {
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
