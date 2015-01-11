using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VDGTech
{
    public class Object3dInfo
    {
        private List<uint> Indices;
        private List<float> VBO;
        private int VertexBuffer, IndexBuffer, VAOHandle;

        public Object3dInfo(List<float> vbo, List<uint> indices)
        {
            VBO = vbo;
            Indices = indices;
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
            var varray = vbo.ToArray();
            var iarray = indices.ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * vbo.Count), varray, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * indices.Count), iarray, BufferUsageHint.StaticDraw);

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
            return new Object3dInfo(out_vertex_buffer, index_buffer);
        }

        public void Draw()
        {
            GL.BindVertexArray(VAOHandle);
            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, Indices.Count,
                DrawElementsType.UnsignedInt, IntPtr.Zero, 0);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine(error.ToString());
                throw new ApplicationException("OpenGL error");
            }
            GL.BindVertexArray(0);
        }
    }
}