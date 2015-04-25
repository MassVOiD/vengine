using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

//using BEPUutilities;
using BulletSharp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VEngine
{
    public class Object3dInfo
    {
        public Object3dInfo(List<float> vbo, List<uint> indices)
        {
            VBO = vbo;
            Indices = indices;
            AreBuffersGenerated = false;
        }
        public Object3dInfo(float[] vbo, uint[] indices)
        {
            VBO = vbo.ToList();
            Indices = indices.ToList();
            AreBuffersGenerated = false;
        }

        public static Object3dInfo Empty
        {
            get
            {
                return new Object3dInfo(new float[0], new uint[0]);
            }
        }

        private int CachedHash = -123;
        public int GetHash()
        {
            if(CachedHash == -123)
            {
                int i = VBO.Count;
                i ^= VBO.Count;
                foreach(var v in VBO)
                    i ^= v.GetHashCode();
                foreach(var v in Indices)
                    i ^= v.GetHashCode();
                CachedHash = i;
            }
            return CachedHash;
        }

        public List<uint> Indices;
        public string MaterialName = "";
        public List<float> VBO;
        public bool WireFrameRendering = false;
        private static Object3dInfo Current = null;
        private bool AreBuffersGenerated;
        private BvhTriangleMeshShape CachedBvhTriangleMeshShape;
        private int VertexBuffer, IndexBuffer, VAOHandle;

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

        public static void CompressAndSaveSingle(Object3dInfo element, string outfile)
        {
            MemoryStream memstream = new MemoryStream();
            memstream.Write(BitConverter.GetBytes(element.VBO.Count), 0, 4);
            memstream.Write(BitConverter.GetBytes(element.Indices.Count), 0, 4);
            foreach(float v in element.VBO)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            foreach(uint v in element.Indices)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            memstream.Flush();
            if(File.Exists(outfile + ".o3i"))
                File.Delete(outfile + ".o3i");
            File.WriteAllBytes(outfile + ".o3i", memstream.ToArray());
        }

        public static Object3dInfo LoadFromCompressed(string infile)
        {
            using(var fileStream = File.OpenRead(infile))
            {
                MemoryStream inStream = new MemoryStream();
                fileStream.CopyTo(inStream);
                inStream.Seek(0, SeekOrigin.Begin);
                byte[] buf = new byte[64];

                inStream.Read(buf, 0, 8);
                int vcount = BitConverter.ToInt32(buf, 0);
                int icount = BitConverter.ToInt32(buf, 4);

                List<float> vertices = new List<float>();
                List<uint> indices = new List<uint>();
                while(vcount > 0)
                {
                    inStream.Read(buf, 0, 4 * 8);

                    vertices.Add(BitConverter.ToSingle(buf, 0));
                    vertices.Add(BitConverter.ToSingle(buf, 4));
                    vertices.Add(BitConverter.ToSingle(buf, 8));
                    vertices.Add(BitConverter.ToSingle(buf, 12));

                    vertices.Add(BitConverter.ToSingle(buf, 16));
                    vertices.Add(BitConverter.ToSingle(buf, 20));
                    vertices.Add(BitConverter.ToSingle(buf, 24));
                    vertices.Add(BitConverter.ToSingle(buf, 28));
                    vcount -= 8;
                }
                while(icount > 0)
                {
                    inStream.Read(buf, 0, 4 * 3);
                    indices.Add(BitConverter.ToUInt32(buf, 0));
                    indices.Add(BitConverter.ToUInt32(buf, 4));
                    indices.Add(BitConverter.ToUInt32(buf, 8));
                    icount -= 3;
                }

                return new Object3dInfo(vertices, indices);
            }
        }
        public static Object3dInfo LoadFromRaw(string vboFile, string indicesFile)
        {
            var vboBytes = File.ReadAllBytes(vboFile);
            var indicesBytes = File.ReadAllBytes(indicesFile);
            
            var vboFloats = new float[vboBytes.Length / 4];
            Buffer.BlockCopy(vboBytes, 0, vboFloats, 0, vboBytes.Length);

            var indicesUints = new uint[indicesBytes.Length / 4];
            Buffer.BlockCopy(indicesBytes, 0, indicesUints, 0, indicesBytes.Length);

            return new Object3dInfo(vboFloats, indicesUints);
        }

        public static List<Object3dInfo> LoadOBJList(List<string> files)
        {
            var outObjects = new List<Object3dInfo>();
            foreach(var f in files)
            {
                outObjects.Add(LoadFromObjSingle(f));
            }
            return outObjects;
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

        public static Dictionary<string, MaterialInfo> LoadMaterialsFromMtl(string filename)
        {
            Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>();
            MaterialInfo currentMaterial = new MaterialInfo();
            string currentName = "";
            string[] lines = File.ReadAllLines(filename);
            Match match;
            foreach(string line in lines)
            {
                if(line.StartsWith("newmtl"))
                {
                    match = Regex.Match(line, @"newmtl (.+)");
                    if(currentName != "")
                    {
                        materials.Add(currentName, currentMaterial);
                        currentMaterial = new MaterialInfo();
                    }
                    currentName = match.Groups[1].Value;
                }
                if(line.StartsWith("Ns"))
                {
                    match = Regex.Match(line, @"Ns ([0-9.-]+)");
                    float val = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    currentMaterial.SpecularStrength = val;
                }
                if(line.StartsWith("d"))
                {
                    match = Regex.Match(line, @"d ([0-9.-]+)");
                    float val = float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    currentMaterial.Transparency = val;
                }
                if(line.StartsWith("Ka"))
                {
                    match = Regex.Match(line, @"Ka ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    int r = (int)(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    int g = (int)(float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    int b = (int)(float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    if(r > 255)
                        r = 255;
                    if(g > 255)
                        g = 255;
                    if(b > 255)
                        b = 255;
                    currentMaterial.AmbientColor = Color.FromArgb(r, g, b);
                }
                if(line.StartsWith("Kd"))
                {
                    match = Regex.Match(line, @"Kd ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    int r = (int)(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    int g = (int)(float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    int b = (int)(float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    if(r > 255)
                        r = 255;
                    if(g > 255)
                        g = 255;
                    if(b > 255)
                        b = 255;
                    currentMaterial.DiffuseColor = Color.FromArgb(r, g, b);
                }
                if(line.StartsWith("Ks"))
                {
                    match = Regex.Match(line, @"Ks ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)");
                    int r = (int)(float.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    int g = (int)(float.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    int b = (int)(float.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture) * 255);
                    if(r > 255)
                        r = 255;
                    if(g > 255)
                        g = 255;
                    if(b > 255)
                        b = 255;
                    currentMaterial.SpecularColor = Color.FromArgb(r, g, b);
                }
                if(line.StartsWith("map_Kd"))
                {
                    match = Regex.Match(line, @"map_Kd (.+)");
                    currentMaterial.TextureName = match.Groups[1].Value;
                }
                if(line.StartsWith("map_d"))
                {
                    match = Regex.Match(line, @"map_d (.+)");
                    currentMaterial.AlphaMask = match.Groups[1].Value;
                }
            }
            if(currentName != "")
                materials.Add(currentName, currentMaterial);
            return materials;
        }

        public static List<Mesh3d> LoadSceneFromObj(string objfile, string mtlfile, float scale = 1.0f)
        {
            string[] lines = File.ReadAllLines(objfile);
            var objs = ParseOBJString(lines);
            var mtllib = LoadMaterialsFromMtl(mtlfile);
            List<Mesh3d> meshes = new List<Mesh3d>();
            Dictionary<string, IMaterial> texCache = new Dictionary<string, IMaterial>();
            Dictionary<Color, IMaterial> colorCache = new Dictionary<Color, IMaterial>();
            Dictionary<IMaterial, MaterialInfo> mInfos = new Dictionary<IMaterial, MaterialInfo>();
            Dictionary<IMaterial, List<Object3dInfo>> linkCache = new Dictionary<IMaterial, List<Object3dInfo>>();
            var colorPink = new SolidColorMaterial(Color.Pink);
            mInfos = new Dictionary<IMaterial, MaterialInfo>();
            foreach(var obj in objs)
            {
                var mat = mtllib.ContainsKey(obj.MaterialName) ? mtllib[obj.MaterialName] : null;
                IMaterial material = null;
                if(mat != null && mat.TextureName.Length > 0)
                {
                    if(texCache.ContainsKey(mat.TextureName + mat.AlphaMask))
                    {
                        material = texCache[mat.TextureName + mat.AlphaMask];
                        mInfos[material] = mat;
                    }
                    else
                    {
                        var m = SingleTextureMaterial.FromMedia(Path.GetFileName(mat.TextureName));
                        m.NormalMapScale = 10;
                        material = m;
                        mInfos[material] = mat;
                        texCache.Add(mat.TextureName + mat.AlphaMask, material);
                       // material = colorPink;
                    }
                    //material = new SolidColorMaterial(Color.Pink);
                }
                else if(mat != null)
                {
                    if(colorCache.ContainsKey(mat.DiffuseColor))
                    {
                        material = colorCache[mat.DiffuseColor];
                        mInfos[material] = mat;
                    }
                    else
                    {
                        material = new SolidColorMaterial(Color.White);
                        mInfos[material] = mat;
                        colorCache.Add(mat.DiffuseColor, material);
                    }
                }
                else
                {
                    material = colorPink;
                    mInfos[material] = mat;
                }


                for(int i = 0; i < obj.VBO.Count; i += 8)
                {
                    obj.VBO[i] *= scale;
                    obj.VBO[i + 1] *= scale;
                    obj.VBO[i + 2] *= scale;
                }
                var o3di = new Object3dInfo(obj.VBO, obj.Indices);
                if(!linkCache.ContainsKey(material))
                    linkCache.Add(material, new List<Object3dInfo> { o3di });
                else
                    linkCache[material].Add(o3di);
            }
            foreach(var kv in linkCache)
            {
                Object3dInfo o3di = kv.Value[0];
                if(kv.Value.Count > 1)
                {
                    foreach(var e in kv.Value.Skip(1))
                        o3di.Append(e);
                }
                var trans = o3di.GetAverageTranslationFromZero();
                o3di.OriginToCenter();
                //o3di.CorrectFacesByNormals();
                // o3di.CorrectFacesByNormals();
                Mesh3d mesh = new Mesh3d(o3di, kv.Key);
                mesh.SpecularComponent = mInfos[kv.Key].SpecularStrength + 0.01f;
                mesh.DiffuseComponent = mInfos[kv.Key].DiffuseColor.GetBrightness() + 0.01f;
                if(mInfos[kv.Key].AlphaMask.Length > 1) mesh.UseAlphaMaskFromMedia(mInfos[kv.Key].AlphaMask);
               // mesh.SpecularComponent = kv.Key.SpecularStrength;
                mesh.Transformation.Translate(trans);
                // mesh.SetCollisionShape(o3di.GetConvexHull(mesh.Transformation.GetPosition(), 1.0f, 1.0f));
                meshes.Add(mesh);
            }
            return meshes;
        }

        public Object3dInfo Copy()
        {
            return new Object3dInfo(VBO, Indices);
        }

        public void Dispose()
        {
            Indices = new List<uint>();
            Indices = null;
            VBO = new List<float>();
            VBO = null;
            GC.Collect();
        }

        public void Draw()
        {
            DrawPrepare();
            GL.DrawElements(ShaderProgram.Current.UsingTesselation ? PrimitiveType.Patches : PrimitiveType.Triangles, Indices.Count,
                    DrawElementsType.UnsignedInt, IntPtr.Zero);
            //GLThread.CheckErrors();
        }

        public void DrawInstanced(int count)
        {
            DrawPrepare();
            GL.DrawElementsInstanced(ShaderProgram.Current.UsingTesselation ? PrimitiveType.Patches : PrimitiveType.Triangles, Indices.Count,
                    DrawElementsType.UnsignedInt, IntPtr.Zero, count);
            //GLThread.CheckErrors();
        }

        public void FlipFaces()
        {
            for(int i = 0; i < Indices.Count; i += 3)
            {
                uint tmp = Indices[i + 2];
                Indices[i + 2] = Indices[i];
                Indices[i] = tmp;
            }
            for(int i = 0; i < VBO.Count; i += 8)
            {
                VBO[i + 5] *= -1;
                VBO[i + 6] *= -1;
                VBO[i + 7] *= -1;
            }
        }
        public void MakeDoubleFaced()
        {
            var copy = this.CopyDeep();
            copy.FlipFaces();
            Append(copy);
        }

        public void CorrectFacesByNormals()
        {
            for(int i = 0; i < Indices.Count; i += 3)
            {
                // for 1
                int vboIndex1 = i * 8;
                int vboIndex2 = (i + 1) * 8;
                int vboIndex3 = (i + 2) * 8;
                var pos1 = new Vector3(VBO[vboIndex1], VBO[vboIndex1 + 1], VBO[vboIndex1 + 2]);
                var pos2 = new Vector3(VBO[vboIndex2], VBO[vboIndex2 + 1], VBO[vboIndex2 + 2]);
                var pos3 = new Vector3(VBO[vboIndex3], VBO[vboIndex3 + 1], VBO[vboIndex3 + 2]);
                var nor1 = new Vector3(VBO[vboIndex1 + 5], VBO[vboIndex1 + 6], VBO[vboIndex1 + 7]);
                var nor2 = new Vector3(VBO[vboIndex2 + 5], VBO[vboIndex2 + 6], VBO[vboIndex2 + 7]);
                var nor3 = new Vector3(VBO[vboIndex3 + 5], VBO[vboIndex3 + 6], VBO[vboIndex3 + 7]);
                var crs1 = Vector3.Cross(pos1, pos2);
                var crs2 = Vector3.Cross(pos2, pos3);
                var crs3 = Vector3.Cross(pos3, pos1);
                var cross = Vector3.Cross(pos1, pos2).Normalized();
                if((cross - crs1).Length < 1.0f)
                {
                    uint tmp = Indices[i + 2];
                    Indices[i + 2] = Indices[i];
                    Indices[i] = tmp;
                }
            }
        }


        public BvhTriangleMeshShape GetAccurateCollisionShape(float scale = 1.0f)
        {
            //if (CachedBvhTriangleMeshShape != null) return CachedBvhTriangleMeshShape;
            List<Vector3> vectors = new List<Vector3>();
            for(int i = 0; i < VBO.Count; i += 8)
                vectors.Add(new Vector3(VBO[i] * scale, VBO[i + 1] * scale, VBO[i + 2] * scale));
            var smesh = new TriangleIndexVertexArray(Indices.Select<uint, int>(a => (int)a).ToArray(), vectors.ToArray());
            CachedBvhTriangleMeshShape = new BvhTriangleMeshShape(smesh, false);
            return CachedBvhTriangleMeshShape;
        }

        public Vector3 GetAverageTranslationFromZero()
        {
            float averagex = 0, averagey = 0, averagez = 0;
            for(int i = 0; i < VBO.Count; i += 8)
            {
                var vertex = new Vector3(VBO[i], VBO[i + 1], VBO[i + 2]);
                averagex += vertex.X;
                averagey += vertex.Y;
                averagez += vertex.Z;
            }
            averagex /= VBO.Count / 8.0f;
            averagey /= VBO.Count / 8.0f;
            averagez /= VBO.Count / 8.0f;
            return new Vector3(averagex, averagey, averagez);
        }
        public void ScaleUV(float x, float y)
        {
            for(int i = 0; i < VBO.Count; i += 8)
            {
                VBO[i + 3] *= x;
                VBO[i + 4] *= y;
            }
        }
        public Object3dInfo CopyDeep()
        {
            return new Object3dInfo(VBO.ToArray(), Indices.ToArray());
        }

        class VertexInfo
        {
            public Vector3 Position, Normal;
            public Vector2 UV;

            public List<float> ToFloatList()
            {
                return new List<float> { Position.X, Position.Y, Position.Z, UV.X, UV.Y, Normal.X, Normal.Y, Normal.Z };
            }
        }

        class TriangleInfo
        {
            VertexInfo V1, V2, V3;
            public List<float> ToFloatList()
            {
                var list = new List<float>();
                list.AddRange(V1.ToFloatList());
                list.AddRange(V2.ToFloatList());
                list.AddRange(V3.ToFloatList());
                return list;
            }
        }

        private Object3dInfo CreateTriangle(TriangleInfo info, bool doubleSided = false)
        {
            return new Object3dInfo(info.ToFloatList().ToArray(), doubleSided ? new uint[] { 0, 1, 2 } : new uint[] { 0, 1, 2, 0, 2, 1 });
        }
        /*
        public Object3dInfo[] Knife(float xmult, float ymult)
        {
            var vbo1 = new List<float>();
            var vbo2 = new List<float>();
            var ind1 = new List<uint>();
            var ind2 = new List<uint>(); 
            List<Vector3> vectors = new List<Vector3>();
            for(int i = 0; i < Indices.Count; i++)
            {
                int vboIndex = i * 8;1
                vectors.Add(new Vector3(VBO[vboIndex] * scale, VBO[vboIndex + 1] * scale, VBO[vboIndex + 2] * scale));
            }
            return new Object3dInfo(VBO.ToArray(), Indices.ToArray());
        }*/

        public void Append(Object3dInfo info)
        {
            VBO.AddRange(info.VBO);
            int startIndex = Indices.Count;
            Indices.AddRange(info.Indices.Select(a => (uint)(a + startIndex)));
        }

        public void ForceRegenerate()
        {
            if(AreBuffersGenerated)
            {
                GL.DeleteBuffer(VertexBuffer);
                GL.DeleteBuffer(IndexBuffer);
                GL.DeleteBuffer(VAOHandle);
                AreBuffersGenerated = false;
            }
        }

        public Vector3 GetAxisAlignedBox()
        {
            float maxx = 0, maxy = 0, maxz = 0;
            float minx = 999999, miny = 999999, minz = 999999;
            for(int i = 0; i < VBO.Count; i += 8)
            {
                var vertex = new Vector3(VBO[i], VBO[i + 1], VBO[i + 2]);

                maxx = maxx < vertex.X ? vertex.X : maxx;
                maxy = maxy < vertex.Y ? vertex.Y : maxy;
                maxz = maxz < vertex.Z ? vertex.Z : maxz;

                minx = minx > vertex.X ? vertex.X : minx;
                miny = miny > vertex.Y ? vertex.Y : miny;
                minz = minz > vertex.Z ? vertex.Z : minz;
            }
            return new Vector3(maxx - minx, maxy - miny, maxz - minz) / 2.0f;
        }

        public ConvexHullShape GetConvexHull(float scale = 1.0f)
        {
            //if (CachedBvhTriangleMeshShape != null) return CachedBvhTriangleMeshShape;
            List<Vector3> vectors = new List<Vector3>();
            for(int i = 0; i < Indices.Count; i++)
            {
                int vboIndex = i * 8;
                vectors.Add(new Vector3(VBO[vboIndex] * scale, VBO[vboIndex + 1] * scale, VBO[vboIndex + 2] * scale));
            }
            var convex = new ConvexHullShape(vectors.ToArray());
            return convex;
        }
        public void ScaleUV(float scale)
        {
            for(int i = 0; i < VBO.Count; i += 8)
            {
                VBO[i + 3] *= scale;
                VBO[i + 4] *= scale;
            }
        }

        public void Normalize()
        {
            List<Vector3> vectors = new List<Vector3>();
            float maxval = 0.0001f;
            for(int i = 0; i < VBO.Count; i += 8)
            {
                var vertex = new Vector3(VBO[i], VBO[i + 1], VBO[i + 2]);
                if(vertex.Length > maxval)
                    maxval = vertex.Length;
            }
            for(int i = 0; i < VBO.Count; i += 8)
            {
                VBO[i] /= maxval;
                VBO[i + 1] /= maxval;
                VBO[i + 2] /= maxval;
            }
        }

        public void OriginToCenter()
        {
            float averagex = 0, averagey = 0, averagez = 0;
            for(int i = 0; i < VBO.Count; i += 8)
            {
                var vertex = new Vector3(VBO[i], VBO[i + 1], VBO[i + 2]);
                averagex += vertex.X;
                averagey += vertex.Y;
                averagez += vertex.Z;
            }
            averagex /= VBO.Count / 8.0f;
            averagey /= VBO.Count / 8.0f;
            averagez /= VBO.Count / 8.0f;
            for(int i = 0; i < VBO.Count; i += 8)
            {
                VBO[i] -= averagex;
                VBO[i + 1] -= averagey;
                VBO[i + 2] -= averagez;
            }
        }

        private static List<ObjFileData> ParseOBJString(string[] lines)
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
            string currentMaterial = "";

            Match match = Match.Empty;
            foreach(string line in lines)
            {
                if(line.StartsWith("o"))
                {
                    match = Regex.Match(line, @"o (.+)");
                    current.VBO = out_vertex_buffer;
                    current.Indices = index_buffer;
                    if(current.VBO.Count >= 8)
                    {
                        current.MaterialName = currentMaterial;
                        objects.Add(current);
                    }
                    current = new ObjFileData();
                    current.Name = match.Groups[1].Value;
                    vcount = 0;
                    //temp_vertices = new List<Vector3>();
                    //temp_normals = new List<Vector3>();
                    //temp_uvs = new List<Vector2>();
                    out_vertex_buffer = new List<float>();
                    index_buffer = new List<uint>();
                }
                if(line.StartsWith("usemtl"))
                {
                    match = Regex.Match(line, @"usemtl (.+)");
                    currentMaterial = match.Groups[1].Value;
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
            current.MaterialName = currentMaterial;
            objects.Add(current);
            current = new ObjFileData();
            current.Name = match.Groups[1].Value;
            return objects;
        }

        private static ObjFileData ParseOBJStringSingle(string[] lines)
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

        private void DrawPrepare()
        {
            if(Current == null || Current.GetHash() != this.GetHash())
            {
                if(!AreBuffersGenerated)
                {
                    GenerateBuffers();
                }
                GL.BindVertexArray(VAOHandle);
            }
            else
            {
                //Console.WriteLine("yay cached");
            }
            //ShaderProgram.Current.Use();
            Current = this;
        }

        private void GenerateBuffers()
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

        public class MaterialInfo
        {
            public MaterialInfo()
            {
                DiffuseColor = Color.White;
                SpecularColor = Color.White;
                AmbientColor = Color.White;
                Transparency = 1.0f;
                SpecularStrength = 1.0f;
                TextureName = "";
                AlphaMask = "";
            }

            public Color DiffuseColor, SpecularColor, AmbientColor;
            public string TextureName;
            public string AlphaMask;
            public float Transparency, SpecularStrength;
        }

        private class ObjFileData
        {
            public List<uint> Indices;
            public string Name, MaterialName;
            public List<float> VBO;
        }
    }
}