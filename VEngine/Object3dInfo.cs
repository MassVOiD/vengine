using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Linq;
//using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities.Prefabs;

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

        public void Dispose()
        {
            Indices = new List<uint>();
            Indices = null;
            VBO = new List<float>();
            VBO = null;
            GC.Collect();
        }

        void GenerateBuffers()
        {
            // Here I create VAO handle
            VAOHandle = GL.GenVertexArray();
            // Here I bind this VAO
            GL.BindVertexArray(VAOHandle);

            // Now, I create 2 VBOs for vertices and inices
            VertexBuffer = GL.GenBuffer();
            IndexBuffer = GL.GenBuffer();

            // Bind it - I THINK - it will bind those VBOs to VAO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer);

            // don't mind my array casting
            var varray = VBO.ToArray();
            var iarray = Indices.ToArray();

            // Here I buffer data in those VBOs and fills it with vertices and indices
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * VBO.Count), varray, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * Indices.Count), iarray, BufferUsageHint.StaticDraw);

            //Enabling 0 location in shaders - There will be vertex model space positions
            GL.EnableVertexAttribArray(0);
            // config for 0 location for shader, vec3, float, not normalized, 32 bytes total, stride 0 bytes
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 4 * 8, 0);

            //Enabling 1 location in shaders - There will be UVs
            GL.EnableVertexAttribArray(1);
            // config for 0 location for shader, vec2, float, not normalized, 32 bytes total, stride 12 bytes
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * 8, 4 * 3);

            //Enabling 2 location in shaders
            GL.EnableVertexAttribArray(2);
            // config for 0 location for shader, vec3, float, not normalized, 32 bytes total, stride 12 bytes
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 4 * 8, 4 * 5);

            //Unbind VAO
            GL.BindVertexArray(0);
            //unbinnd VBOs
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            AreBuffersGenerated = true;
        }

        public static Object3dInfo[] LoadFromObj(string infile)
        {
            string[] lines = File.ReadAllLines(infile);
            var data = ParseOBJString(lines);
            return data.Select<ObjFileData, Object3dInfo>(a => new Object3dInfo(a.VBO, a.Indices)).ToArray();
        }
        public static Object3dInfo LoadFromObjSingle(string infile)
        {
            string[] lines = File.ReadAllLines(infile);
            var data = ParseOBJStringSingle(lines);
            return new Object3dInfo(data.VBO, data.Indices);
        }

        class ObjFileData
        {
            public List<float> VBO;
            public List<uint> Indices;
            public string Name;
        }

        public static void CompressAndSave(string infile, string outdir)
        {
            string[] lines = File.ReadAllLines(infile);
            var data = ParseOBJString(lines);
            foreach(var element in data)
            {
                MemoryStream memstream = new MemoryStream();
                memstream.Write(BitConverter.GetBytes(element.VBO.Count), 0, 4);
                memstream.Write(BitConverter.GetBytes(element.Indices.Count), 0, 4);
                foreach(float v in element.VBO)
                    memstream.Write(BitConverter.GetBytes(v), 0, 4);
                foreach(uint v in element.Indices)
                    memstream.Write(BitConverter.GetBytes(v), 0, 4);
                memstream.Flush();
                if(File.Exists(outdir + "/" + element.Name + ".o3i"))
                    File.Delete(outdir + "/" + element.Name + ".o3i");
                File.WriteAllBytes(outdir + "/" + element.Name + ".o3i", memstream.ToArray());
            }
        }
        public static void CompressAndSaveSingle(string infile, string outdir)
        {
            string[] lines = File.ReadAllLines(infile);
            var element = ParseOBJStringSingle(lines);

            MemoryStream memstream = new MemoryStream();
            memstream.Write(BitConverter.GetBytes(element.VBO.Count), 0, 4);
            memstream.Write(BitConverter.GetBytes(element.Indices.Count), 0, 4);
            foreach(float v in element.VBO)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            foreach(uint v in element.Indices)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            memstream.Flush();
            if(File.Exists(outdir + ".o3i"))
                File.Delete(outdir + ".o3i");
            File.WriteAllBytes(outdir + ".o3i", memstream.ToArray());

        }
        public static void CompressAndSave(Object3dInfo data, string outfile)
        {
            MemoryStream memstream = new MemoryStream();
            memstream.Write(BitConverter.GetBytes(data.VBO.Count), 0, 4);
            memstream.Write(BitConverter.GetBytes(data.Indices.Count), 0, 4);
            foreach(float v in data.VBO)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            foreach(uint v in data.Indices)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            memstream.Flush();
            if(File.Exists(outfile + ".o3i"))
                File.Delete(outfile + ".o3i");
            File.WriteAllBytes(outfile + ".o3i", memstream.ToArray());

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
            while(vcount-- > 0)
            {
                inStream.Read(buf, 0, 4);
                vertices.Add(BitConverter.ToSingle(buf, 0));
            }
            while(icount-- > 0)
            {
                inStream.Read(buf, 0, 4);
                indices.Add(BitConverter.ToUInt32(buf, 0));
            }
            inStream.Close();
            return new Object3dInfo(vertices, indices);
        }

        static List<ObjFileData> ParseOBJString(string[] lines)
        {
            List<ObjFileData> objects = new List<ObjFileData>();
            List<Vector3> temp_vertices = new List<Vector3>(), temp_normals = new List<Vector3>();
            List<Vector2> temp_uvs = new List<Vector2>();
            List<float> out_vertex_buffer = new List<float>();
            List<uint> index_buffer = new List<uint>();
            HashSet<Tuple<int, int, int>> indicesRedecer = new HashSet<Tuple<int, int, int>>();
            ;
            //out_vertex_buffer.AddRange(Enumerable.Repeat<double>(0, 8));
            uint vcount = 0;

            ObjFileData current = new ObjFileData();

            Match match = Match.Empty;
            foreach(string line in lines)
            {
                if(line.StartsWith("o"))
                {
                    match = Regex.Match(line, @"o (.+)");
                    current.VBO = out_vertex_buffer;
                    current.Indices = index_buffer;
                    objects.Add(current);
                    current = new ObjFileData();
                    current.Name = match.Groups[1].Value;
                    vcount = 0;
                    //temp_vertices = new List<Vector3>();
                    //temp_normals = new List<Vector3>();
                    //temp_uvs = new List<Vector2>();
                    out_vertex_buffer = new List<float>();
                    index_buffer = new List<uint>();
                }
                if(line.StartsWith("vt"))
                {
                    match = Regex.Match(line, @"vt ([0-9.-]+) ([0-9.-]+)");
                    temp_uvs.Add(new Vector2(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if(line.StartsWith("vn"))
                {
                    match = Regex.Match(line, @"vn ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    temp_normals.Add(new Vector3(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if(line.StartsWith("v"))
                {
                    match = Regex.Match(line, @"v ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    temp_vertices.Add(new Vector3(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if(line.StartsWith("f"))
                {
                    match = Regex.Match(line, @"f ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+)");
                    if(match.Success)
                    {
                        for(int i = 1; ; )
                        {
                            Vector3 vertex = temp_vertices[int.Parse(match.Groups[i++].Value) - 1];
                            Vector2 uv = temp_uvs[int.Parse(match.Groups[i++].Value) - 1];
                            Vector3 normal = temp_normals[int.Parse(match.Groups[i++].Value) - 1];

                            out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, uv.X, uv.Y, normal.X, normal.Y, normal.Z });
                            index_buffer.Add(vcount++);
                            if(i >= 9)
                                break;
                        }
                    }
                    else
                    {
                        match = Regex.Match(line, @"f ([0-9]+)//([0-9]+) ([0-9]+)//([0-9]+) ([0-9]+)//([0-9]+)");
                        if(match.Success)
                        {
                            for(int i = 1; ; )
                            {
                                Vector3 vertex = temp_vertices[int.Parse(match.Groups[i++].Value) - 1];
                                Vector3 normal = temp_normals[int.Parse(match.Groups[i++].Value) - 1];

                                out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, 0.0f, 0.0f, normal.X, normal.Y, normal.Z });
                                index_buffer.Add(vcount++);
                                if(i >= 6)
                                    break;
                            }
                        }
                    }
                }
            }
            current.VBO = out_vertex_buffer;
            current.Indices = index_buffer;
            objects.Add(current);
            current = new ObjFileData();
            current.Name = match.Groups[1].Value;
            return objects;
        }


        static ObjFileData ParseOBJStringSingle(string[] lines)
        {
            List<ObjFileData> objects = new List<ObjFileData>();
            List<Vector3> temp_vertices = new List<Vector3>(), temp_normals = new List<Vector3>();
            List<Vector2> temp_uvs = new List<Vector2>();
            List<float> out_vertex_buffer = new List<float>();
            List<uint> index_buffer = new List<uint>();
            HashSet<Tuple<int, int, int>> indicesRedecer = new HashSet<Tuple<int, int, int>>();
            ;
            //out_vertex_buffer.AddRange(Enumerable.Repeat<double>(0, 8));
            uint vcount = 0;

            ObjFileData current = new ObjFileData();

            Match match = Match.Empty;
            foreach(string line in lines)
            {
                if(line.StartsWith("vt"))
                {
                    match = Regex.Match(line, @"vt ([0-9.-]+) ([0-9.-]+)");
                    temp_uvs.Add(new Vector2(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if(line.StartsWith("vn"))
                {
                    match = Regex.Match(line, @"vn ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    temp_normals.Add(new Vector3(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if(line.StartsWith("v"))
                {
                    match = Regex.Match(line, @"v ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    temp_vertices.Add(new Vector3(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture), float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture)));
                }
                else if(line.StartsWith("f"))
                {
                    match = Regex.Match(line, @"f ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+)");
                    if(match.Success)
                    {
                        for(int i = 1; ; )
                        {
                            Vector3 vertex = temp_vertices[int.Parse(match.Groups[i++].Value) - 1];
                            Vector2 uv = temp_uvs[int.Parse(match.Groups[i++].Value) - 1];
                            Vector3 normal = temp_normals[int.Parse(match.Groups[i++].Value) - 1];

                            out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, uv.X, uv.Y, normal.X, normal.Y, normal.Z });
                            index_buffer.Add(vcount++);
                            if(i >= 9)
                                break;
                        }
                    }
                    else
                    {
                        match = Regex.Match(line, @"f ([0-9]+)//([0-9]+) ([0-9]+)//([0-9]+) ([0-9]+)//([0-9]+)");
                        if(match.Success)
                        {
                            for(int i = 1; ; )
                            {
                                Vector3 vertex = temp_vertices[int.Parse(match.Groups[i++].Value) - 1];
                                Vector3 normal = temp_normals[int.Parse(match.Groups[i++].Value) - 1];

                                out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, 0.0f, 0.0f, normal.X, normal.Y, normal.Z });
                                index_buffer.Add(vcount++);
                                if(i >= 6)
                                    break;
                            }
                        }
                    }
                }
            }
            current.VBO = out_vertex_buffer;
            current.Indices = index_buffer;
            objects.Add(current);
            current = new ObjFileData();
            current.Name = match.Groups[1].Value;
            return objects.First();
        }

        static Object3dInfo Current = null;
        private StaticMesh CachedBvhTriangleMeshShape;

        public StaticMesh GetAccurateCollisionShape(Vector3 position, float scale = 1.0f)
        {

            //if (CachedBvhTriangleMeshShape != null) return CachedBvhTriangleMeshShape;
            List<BEPUutilities.Vector3> vectors = new List<BEPUutilities.Vector3>();
            for(int i = 0; i < VBO.Count; i += 8)
                vectors.Add(new BEPUutilities.Vector3(VBO[i] * scale, VBO[i + 1] * scale, VBO[i + 2] * scale) + position.ToBepu());

            var staticMesh = new StaticMesh(
                vectors.ToArray(),
                Indices.Select<uint, int>(a => (int)a).ToArray(),
                new BEPUutilities.AffineTransform(BEPUutilities.Matrix3x3.CreateFromAxisAngle(BEPUutilities.Vector3.Up, 0), Vector3.Zero));
            staticMesh.Sidedness = BEPUutilities.TriangleSidedness.DoubleSided;

            CachedBvhTriangleMeshShape = staticMesh;
            return CachedBvhTriangleMeshShape;
        }

        void DrawPrepare()
        {
            if(!AreBuffersGenerated)
            {
                GenerateBuffers();
            }
            Current = this;
            GL.BindVertexArray(VAOHandle);
            ShaderProgram.Current.Use();
        }

        public void Draw()
        {
            DrawPrepare();
            GL.DrawElementsBaseVertex(ShaderProgram.Current.UsingTesselation ? PrimitiveType.Patches : PrimitiveType.Triangles, Indices.Count,
                    DrawElementsType.UnsignedInt, IntPtr.Zero, 0);
            GLThread.CheckErrors();

        }
        public void DrawInstanced(int count)
        {
            DrawPrepare();
            GL.DrawElementsInstancedBaseVertex(PrimitiveType.Triangles, Indices.Count,
                    DrawElementsType.UnsignedInt, IntPtr.Zero, count, 0);
            GLThread.CheckErrors();
        }
    }
}