using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using BulletSharp;
using System.Linq;

namespace VDGTech
{
    public class Object3dInfo
    {
        private List<uint> Indices;
        private List<float> VBO;
        private int VertexBuffer, IndexBuffer, VAOHandle;
        private bool AreBuffersGenerated;
        public bool WireFrameRendering = false;

        public Object3dInfo(List<float> vbo, List<uint> indices)
        {
            VBO = vbo;
            Indices = indices;
            AreBuffersGenerated = false;
        }

        public void GenerateBuffers()
        {
            VertexBuffer = GL.GenBuffer();
            IndexBuffer = GL.GenBuffer();

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
            var varray = VBO.ToArray();
            var iarray = Indices.ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * VBO.Count), varray, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * Indices.Count), iarray, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            VAOHandle = GL.GenVertexArray();
            GL.BindVertexArray(VAOHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);
            GL.EnableVertexAttribArray(0);

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }

            GL.VertexAttribPointer(
                0,                  // attribute 0
                3,                  // size
                VertexAttribPointerType.Float,           // type
                false,           // normalized?
                4 * 8,                  // stride
                0            // array buffer offset
                );

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2,
                VertexAttribPointerType.Float,
                false,
                4 * 8,
                4 * 3);

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3,
                VertexAttribPointerType.Float,
                false,
                4 * 8,
                4 * 5);

            GL.BindVertexArray(0);
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }
        }

        public static Object3dInfo LoadFromObj(string infile)
        {
            string[] lines = File.ReadAllLines(infile);
            var data = ParseOBJString(lines);
            return new Object3dInfo(data.Item1, data.Item2);
        }

        public static void CompressAndSave(string infile, string outfile)
        {
            string[] lines = File.ReadAllLines(infile);
            var data = ParseOBJString(lines);
            if (File.Exists(outfile)) File.Delete(outfile);

            MemoryStream memstream = new MemoryStream();
            memstream.Write(BitConverter.GetBytes(data.Item1.Count), 0, 4);
            memstream.Write(BitConverter.GetBytes(data.Item2.Count), 0, 4);
            foreach (float v in data.Item1) memstream.Write(BitConverter.GetBytes(v), 0, 4);
            foreach (uint v in data.Item2) memstream.Write(BitConverter.GetBytes(v), 0, 4);
            memstream.Flush();

            File.WriteAllBytes(outfile, memstream.ToArray());
        }
        public static Object3dInfo LoadFromCompressed(string infile)
        {
            var inStream = File.OpenRead(infile);

            byte[] buf = new byte[4];

            inStream.Read(buf, 0, 4);
            int vcount = BitConverter.ToInt32(buf, 0);
            inStream.Read(buf, 0, 4);
            int icount = BitConverter.ToInt32(buf, 0);

            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            while (vcount-- > 0)
            {
                inStream.Read(buf, 0, 4);
                vertices.Add(BitConverter.ToSingle(buf, 0));
            }
            while (icount-- > 0)
            {
                inStream.Read(buf, 0, 4);
                indices.Add(BitConverter.ToUInt32(buf, 0));
            }

            return new Object3dInfo(vertices, indices);
        }

        static Tuple<List<float>, List<uint>> ParseOBJString(string[] lines)
        {

            List<Vector3> temp_vertices = new List<Vector3>(), temp_normals = new List<Vector3>();
            List<Vector2> temp_uvs = new List<Vector2>();
            List<float> out_vertex_buffer = new List<float>();
            List<uint> index_buffer = new List<uint>();
            HashSet<Tuple<int, int, int>> indicesRedecer = new HashSet<Tuple<int, int, int>>(); ;
            //out_vertex_buffer.AddRange(Enumerable.Repeat<double>(0, 8));
            uint vcount = 0;

            Match match = Match.Empty;
            foreach (string line in lines)
            {
                if (line.StartsWith("vt"))
                {
                    match = Regex.Match(line, @"vt ([0-9.-]+) ([0-9.-]+)");
                    temp_uvs.Add(new Vector2(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (line.StartsWith("vn"))
                {
                    match = Regex.Match(line, @"vn ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    temp_normals.Add(new Vector3(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (line.StartsWith("v"))
                {
                    match = Regex.Match(line, @"v ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    temp_vertices.Add(new Vector3(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if (line.StartsWith("f"))
                {
                    match = Regex.Match(line, @"f ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+)");
                    if (match.Success)
                    {
                        for (int i = 1; ; )
                        {
                            Vector3 vertex = temp_vertices[int.Parse(match.Groups[i++].Value) - 1];
                            Vector2 uv = temp_uvs[int.Parse(match.Groups[i++].Value) - 1];
                            Vector3 normal = temp_normals[int.Parse(match.Groups[i++].Value) - 1];

                            out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, uv.X, uv.Y, normal.X, normal.Y, normal.Z });
                            index_buffer.Add(vcount++);
                            if (i >= 9) break;
                        }
                    }
                    else
                    {
                        match = Regex.Match(line, @"f ([0-9]+)//([0-9]+) ([0-9]+)//([0-9]+) ([0-9]+)//([0-9]+)");
                        if (match.Success)
                        {
                            for (int i = 1; ; )
                            {
                                Vector3 vertex = temp_vertices[int.Parse(match.Groups[i++].Value) - 1];
                                Vector3 normal = temp_normals[int.Parse(match.Groups[i++].Value) - 1];

                                out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, 0.0f, 0.0f, normal.X, normal.Y, normal.Z });
                                index_buffer.Add(vcount++);
                                if (i >= 6) break;
                            }
                        }
                    }
                }
            }
            return new Tuple<List<float>, List<uint>>(out_vertex_buffer, index_buffer);
        }

        static Object3dInfo Current = null;
        private BvhTriangleMeshShape CachedBvhTriangleMeshShape; 

        public BvhTriangleMeshShape GetAccurateCollisionShape(float scale = 1.0f)
        {
            //if (CachedBvhTriangleMeshShape != null) return CachedBvhTriangleMeshShape;
            List<Vector3> vectors = new List<Vector3>();
            for (int i = 0; i < VBO.Count; i += 8) vectors.Add(new Vector3(VBO[i] * scale, VBO[i + 1] * scale, VBO[i + 2] * scale));
            var smesh = new TriangleIndexVertexArray(Indices.Select<uint, int>(a => (int)a).ToArray(), vectors.ToArray());
            CachedBvhTriangleMeshShape = new BvhTriangleMeshShape(smesh, false);
            return CachedBvhTriangleMeshShape;
        }

        public void Draw()
        {
            if (!AreBuffersGenerated)
            {
                //GenerateBuffers();
                AreBuffersGenerated = true;
            }
            if (Current != this)
            {
                Current = this;
                GL.BindVertexArray(VAOHandle);
                ShaderProgram.Current.Use();
                if (WireFrameRendering)
                {
                    GL.DrawElementsBaseVertex(PrimitiveType.LineStrip, Indices.Count,
                        DrawElementsType.UnsignedInt, IntPtr.Zero, 0);
                }
                else
                {
                    GL.DrawElementsBaseVertex(PrimitiveType.Triangles, Indices.Count,
                        DrawElementsType.UnsignedInt, IntPtr.Zero, 0);
                }
                var error = GL.GetError();
                if (error != ErrorCode.NoError)
                {
                    Console.WriteLine(error.ToString());
                    throw new ApplicationException("OpenGL error");
                }
            }
            else
            {
                if (WireFrameRendering)
                {
                    GL.DrawElementsBaseVertex(PrimitiveType.LineStrip, Indices.Count,
                        DrawElementsType.UnsignedInt, IntPtr.Zero, 0);
                }
                else
                {
                    GL.DrawElementsBaseVertex(PrimitiveType.Triangles, Indices.Count,
                        DrawElementsType.UnsignedInt, IntPtr.Zero, 0);
                }
            }
        }
    }
}