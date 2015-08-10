using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
                for(int x = 0; x < VBO.Count;x++)
                    i ^= VBO[x].GetHashCode();
                for(int ix = 0; ix < Indices.Count; ix++)
                    i ^= Indices[ix].GetHashCode();
                CachedHash = i;
            }
            return CachedHash;
        }

        public List<uint> Indices;
        public string MaterialName = "";
        public List<float> VBO;
        public bool WireFrameRendering = false;
        //private Object3dInfo Current = null;
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
            if(File.Exists(outfile + ".o3i"))
                File.Delete(outfile + ".o3i");

            FileStream memstream = File.Create(outfile + ".o3i");
            memstream.Write(BitConverter.GetBytes(element.VBO.Count), 0, 4);
            memstream.Write(BitConverter.GetBytes(element.Indices.Count), 0, 4);
            foreach(float v in element.VBO)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            foreach(uint v in element.Indices)
                memstream.Write(BitConverter.GetBytes(v), 0, 4);
            memstream.Flush();
            memstream.Close();
            //File.WriteAllBytes(outfile + ".o3i", memstream.ToArray());
        }


        public static List<Mesh3d> LoadSceneFromCollada(string infile)
        {
            List<Mesh3d> infos = new List<Mesh3d>();

            XDocument xml = XDocument.Load(infile);
            var colladaNode = xml.Elements().First();
            var lib = colladaNode.SelectSingle("library_geometries");
            var geometries = lib.SelectMany("geometry");

            foreach(var geom in geometries)
            {
                try
                {
                    string geoID = geom.Attribute("id").Value;
                    string geoName = geom.Attribute("name").Value;
                    List<float> xyzs = geom.SelectSingle("mesh").SelectMany("source").ElementAt(0).SelectSingle("float_array").Value.Split(new char[] { ' ' }).Select<string, float>((a) => float.Parse(a, System.Globalization.NumberFormatInfo.InvariantInfo)).ToList();
                    List<float> normals = geom.SelectSingle("mesh").SelectMany("source").ElementAt(1).SelectSingle("float_array").Value.Trim().Split(new char[] { ' ' }).Select<string, float>((a) => float.Parse(a, System.Globalization.NumberFormatInfo.InvariantInfo)).ToList();

                    List<float> uvs = null;
                    try
                    {
                        uvs = geom.SelectSingle("mesh").SelectMany("source").ElementAt(2).SelectSingle("float_array").Value.Trim().Split(new char[] { ' ' }).Select<string, float>((a) => float.Parse(a, System.Globalization.NumberFormatInfo.InvariantInfo)).ToList();
                    }
                    catch
                    {
                        uvs = new List<float>();
                    }
                    List<int> indices = geom.SelectSingle("mesh").SelectSingle("polylist").SelectSingle("p").Value.Trim().Split(new char[] { ' ' }).Select<string, int>((a) => int.Parse(a)).ToList();
                    List<float> VBO = new List<float>();
                    List<uint> indicesNew = new List<uint>();
                    uint vcount = 0;
                    for(int i = 0; i < indices.Count; )
                    {
                        int vid = indices[i] * 3;
                        int nid = indices[i + 1] * 3;
                        int uid = indices[i + 2] * 2;
                        indicesNew.Add(vcount++);
                        VBO.AddRange(new float[] { -xyzs[vid + 1], xyzs[vid + 2], -xyzs[vid] });
                        if(uvs.Count > 0)
                        {
                            VBO.AddRange(new float[] { uvs[uid], uvs[uid + 1] });
                            i += 3;
                        }
                        else
                        {
                            VBO.AddRange(new float[] { 0, 0 });
                            i += 2;
                        }
                        VBO.AddRange(new float[] { -normals[nid + 1], normals[nid + 2], -normals[nid] });
                    }
                    var objinfo = new Object3dInfo(VBO, indicesNew);
                    var transformationNode = colladaNode.SelectSingle("library_visual_scenes").SelectSingle("visual_scene").SelectMany("node").First((a) => a.SelectSingle("instance_geometry").Attribute("url").Value == "#" + geoID);
                    var mesh = new Mesh3d(objinfo, new GenericMaterial(Color.White));
                    List<float> transVector = transformationNode.SelectMany("translate").First((a) => a.Attribute("sid").Value == "location").Value.Trim().Split(new char[] { ' ' }).Select<string, float>((a) => float.Parse(a, System.Globalization.NumberFormatInfo.InvariantInfo)).ToList();
                    List<List<float>> rots = transformationNode.SelectMany("rotate").Select<XElement, List<float>>((a) => a.Value.Trim().Split(new char[] { ' ' }).Select<string, float>((ax) => float.Parse(ax, System.Globalization.NumberFormatInfo.InvariantInfo)).ToList()).ToList();
                    List<float> scale = transformationNode.SelectMany("scale").First((a) => a.Attribute("sid").Value == "scale").Value.Trim().Split(new char[] { ' ' }).Select<string, float>((a) => float.Parse(a, System.Globalization.NumberFormatInfo.InvariantInfo)).ToList();
                    mesh.Translate(-transVector[1], transVector[2], -transVector[0]);
                    foreach(var r in rots)
                        mesh.Rotate(Quaternion.FromAxisAngle(new Vector3(-r[1], r[2], -r[0]), MathHelper.DegreesToRadians(r[3])));
                    mesh.Scale(scale[1], scale[2], scale[0]);
                    infos.Add(mesh);
                }
                catch
                {
                }
            }

            return infos;
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
                    currentMaterial.TextureName = Path.GetFileName(match.Groups[1].Value);
                }
                if(line.StartsWith("map_d"))
                {
                    match = Regex.Match(line, @"map_d (.+)");
                    currentMaterial.AlphaMask = Path.GetFileName(match.Groups[1].Value);
                }
                if(line.StartsWith("map_Bump"))
                {
                    match = Regex.Match(line, @"map_Bump (.+)");
                    currentMaterial.BumpMapName = Path.GetFileName(match.Groups[1].Value);
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
            Dictionary<string, GenericMaterial> texCache = new Dictionary<string, GenericMaterial>();
            Dictionary<Color, GenericMaterial> colorCache = new Dictionary<Color, GenericMaterial>();
            Dictionary<GenericMaterial, MaterialInfo> mInfos = new Dictionary<GenericMaterial, MaterialInfo>();
            Dictionary<GenericMaterial, List<Object3dInfo>> linkCache = new Dictionary<GenericMaterial, List<Object3dInfo>>();
            var colorPink = new GenericMaterial(Color.Pink);
            mInfos = new Dictionary<GenericMaterial, MaterialInfo>();
            foreach(var obj in objs)
            {
                var mat = mtllib.ContainsKey(obj.MaterialName) ? mtllib[obj.MaterialName] : null;
                GenericMaterial material = null;
                if(mat != null && mat.TextureName.Length > 0)
                {
                    if(texCache.ContainsKey(mat.TextureName + mat.AlphaMask))
                    {
                        material = texCache[mat.TextureName + mat.AlphaMask];
                        mInfos[material] = mat;
                    }
                    else
                    {
                        var m = GenericMaterial.FromMedia(Path.GetFileName(mat.TextureName));
                        m.NormalMapScale = 10;
                        material = m;
                        mInfos[material] = mat;
                        texCache.Add(mat.TextureName + mat.AlphaMask, material);
                        // material = colorPink;
                    }
                    //material = new GenericMaterial(Color.Pink);
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
                        material = new GenericMaterial(Color.White);
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
                kv.Key.SpecularComponent = 1.0f - mInfos[kv.Key].SpecularStrength + 0.01f;
                kv.Key.Roughness = (mInfos[kv.Key].SpecularStrength);
                kv.Key.ReflectionStrength = 1.0f - (mInfos[kv.Key].SpecularStrength);
                kv.Key.DiffuseComponent = mInfos[kv.Key].DiffuseColor.GetBrightness() + 0.01f;
                if(mInfos[kv.Key].AlphaMask.Length > 1)
                    (kv.Key as GenericMaterial).SetAlphaMaskFromMedia(mInfos[kv.Key].AlphaMask);
                if(mInfos[kv.Key].BumpMapName.Length > 1)
                    ((GenericMaterial)kv.Key).SetBumpMapFromMedia(mInfos[kv.Key].BumpMapName);
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
            GLThread.CheckErrors();
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
        public void Transform(Matrix4 ModelMatrix, Matrix4 RotationMatrix)
        {
            for(int i = 0; i < VBO.Count; i += 8)
            {
                var vertex = Vector3.Transform(new Vector3(VBO[i + 0], VBO[i + 1], VBO[i + 2]), ModelMatrix);
                var normal = Vector3.Transform(new Vector3(VBO[i + 5], VBO[i + 6], VBO[i + 7]), RotationMatrix);

                VBO[i + 0] = vertex.X;
                VBO[i + 1] = vertex.Y;
                VBO[i + 2] = vertex.Z;

                VBO[i + 5] = normal.X;
                VBO[i + 6] = normal.Y;
                VBO[i + 7] = normal.Z;
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
                if((cross - crs1).Length >= 1.0f)
                {
                    uint tmp = Indices[i + 2];
                    Indices[i + 2] = Indices[i];
                    Indices[i] = tmp;
                }
            }
        }
        public List<Vector3> GetOrderedVertices()
        {
            var ot = new List<Vector3>();
            for(int i = 0; i < Indices.Count; i++)
            {
                // for 1
                int vboIndex1 = i * 8;
                var pos1 = new Vector3(VBO[vboIndex1], VBO[vboIndex1 + 1], VBO[vboIndex1 + 2]);
                ot.Add(pos1);
            }
            return ot;
        }
        public List<Vector3> GetOrderedNormals()
        {
            var ot = new List<Vector3>();
            for(int i = 0; i < Indices.Count; i++)
            {
                // for 1
                int vboIndex1 = i * 8;
                var pos1 = new Vector3(VBO[vboIndex1 + 5], VBO[vboIndex1 + 6], VBO[vboIndex1 + 7]);
                ot.Add(pos1);
            }
            return ot;
        }
        public List<int> GetOrderedIndices()
        {
            var ot = new List<int>();
            for(int i = 0; i < Indices.Count; i++)
            {
                ot.Add(i);
            }
            return ot;
        }


        public BvhTriangleMeshShape GetAccurateCollisionShape(float scale = 1.0f)
        {
            //if (CachedBvhTriangleMeshShape != null) return CachedBvhTriangleMeshShape;
            List<Vector3> vectors = GetOrderedVertices();
            var smesh = new TriangleIndexVertexArray(GetOrderedIndices().ToArray(), vectors.ToArray());
            CachedBvhTriangleMeshShape = new BvhTriangleMeshShape(smesh, false);
            //CachedBvhTriangleMeshShape.LocalScaling = new Vector3(scale);
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

        public void FreeCPUMemory()
        {
            VBO = null;
            Indices = null;
            GC.Collect();
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
            return new Vector3(maxx - minx, maxy - miny, maxz - minz);
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

                                out_vertex_buffer.AddRange(new float[] { vertex.X, vertex.Y, vertex.Z, 1.0f, 1.0f, normal.X, normal.Y, normal.Z });
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
            if(!AreBuffersGenerated)
            {
                GenerateBuffers();
            }
            GL.BindVertexArray(VAOHandle);

        }

        private Vector3 CalculateTangent(Vector3 normal, Vector3 v1, Vector3 v2, Vector2 st1, Vector2 st2)
        {
            float coef = 1.0f / (st1.X * st2.Y - st2.X * st1.Y);
            var tangent = Vector3.Zero;

            tangent.X = coef * ((v1.X * st2.Y) + (v2.X * -st1.X));
            tangent.Y = coef * ((v1.Y * st2.Y) + (v2.Y * -st1.X));
            tangent.Z = coef * ((v1.Z * st2.Y) + (v2.Z * -st1.X));

            //float3 binormal = normal.crossProduct(tangent);
            return tangent;
        }

        public void UpdateTangents()
        {
            var floats = new List<float>();
            for(int i = 0; i < Indices.Count; i += 3)
            {
                // 8 vbo stride
                int vboIndex1 = (int)Indices[i] * 8;
                int vboIndex2 = (int)Indices[(i + 1)] * 8;
                int vboIndex3 = (int)Indices[(i + 2)] * 8;
                var pos1 = new Vector3(VBO[vboIndex1], VBO[vboIndex1 + 1], VBO[vboIndex1 + 2]);
                var pos2 = new Vector3(VBO[vboIndex2], VBO[vboIndex2 + 1], VBO[vboIndex2 + 2]);
                var pos3 = new Vector3(VBO[vboIndex3], VBO[vboIndex3 + 1], VBO[vboIndex3 + 2]);
                var uv1 = new Vector2(VBO[vboIndex1 + 3], VBO[vboIndex1 + 4]);
                var uv2 = new Vector2(VBO[vboIndex2 + 3], VBO[vboIndex2 + 4]);
                var uv3 = new Vector2(VBO[vboIndex3 + 3], VBO[vboIndex3 + 4]);
                var nor1 = new Vector3(VBO[vboIndex1 + 5], VBO[vboIndex1 + 6], VBO[vboIndex1 + 7]);
                var nor2 = new Vector3(VBO[vboIndex2 + 5], VBO[vboIndex2 + 6], VBO[vboIndex2 + 7]);
                var nor3 = new Vector3(VBO[vboIndex3 + 5], VBO[vboIndex3 + 6], VBO[vboIndex3 + 7]);


                var tan1 = Vector3.Zero;

                Indices[i] = i == 0 ? 0 : Indices[i - 1] + 1;
                floats.AddRange(new float[]{
                    pos1.X, pos1.Y, pos1.Z, uv1.X, uv1.Y, nor1.X, nor1.Y, nor1.Z, tan1.X, tan1.Y, tan1.Z
                });
                Indices[i + 1] = Indices[i] + 1;
                floats.AddRange(new float[]{
                    pos2.X, pos2.Y, pos2.Z, uv2.X, uv2.Y, nor2.X, nor2.Y, nor2.Z, tan1.X, tan1.Y, tan1.Z
                });
                Indices[i + 2] = Indices[i + 1] + 1;
                floats.AddRange(new float[]{
                    pos3.X, pos3.Y, pos3.Z, uv3.X, uv3.Y, nor3.X, nor3.Y, nor3.Z, tan1.X, tan1.Y, tan1.Z
                });

            }
            VBO = floats;
            for(int i = 0; i < Indices.Count; i += 3)
            {
                // 8 vbo stride
                int vboIndex1 = (int)Indices[i] * 11;
                int vboIndex2 = (int)Indices[(i + 1)] * 11;
                int vboIndex3 = (int)Indices[(i + 2)] * 11;
                var pos1 = new Vector3(VBO[vboIndex1], VBO[vboIndex1 + 1], VBO[vboIndex1 + 2]);
                var pos2 = new Vector3(VBO[vboIndex2], VBO[vboIndex2 + 1], VBO[vboIndex2 + 2]);
                var pos3 = new Vector3(VBO[vboIndex3], VBO[vboIndex3 + 1], VBO[vboIndex3 + 2]);
                var uv1 = new Vector2(VBO[vboIndex1 + 3], VBO[vboIndex1 + 4]);
                var uv2 = new Vector2(VBO[vboIndex2 + 3], VBO[vboIndex2 + 4]);
                var uv3 = new Vector2(VBO[vboIndex3 + 3], VBO[vboIndex3 + 4]);
                var nor1 = new Vector3(VBO[vboIndex1 + 5], VBO[vboIndex1 + 6], VBO[vboIndex1 + 7]);
                var nor2 = new Vector3(VBO[vboIndex2 + 5], VBO[vboIndex2 + 6], VBO[vboIndex2 + 7]);
                var nor3 = new Vector3(VBO[vboIndex3 + 5], VBO[vboIndex3 + 6], VBO[vboIndex3 + 7]);
                float x1 = pos2.X - pos1.X;
                float x2 = pos3.X - pos1.X;
                float y1 = pos2.Y - pos1.Y;
                float y2 = pos3.Y - pos1.Y;
                float z1 = pos2.Z - pos1.Z;
                float z2 = pos3.Z - pos1.Z;

                float s1 = uv2.X - uv1.X;
                float s2 = uv3.X - uv1.X;
                float t1 = uv2.Y - uv1.Y;
                float t2 = uv3.Y - uv1.Y;

                float r = 1.0F / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,
                        (t2 * z1 - t1 * z2) * r);
                VBO[vboIndex1 + 8] += sdir.X;
                VBO[vboIndex1 + 9] += sdir.Y;
                VBO[vboIndex1 + 10] += sdir.Z;
                VBO[vboIndex2 + 8] += sdir.X;
                VBO[vboIndex2 + 9] += sdir.Y;
                VBO[vboIndex2 + 10] += sdir.Z;
                VBO[vboIndex3 + 8] += sdir.X;
                VBO[vboIndex3 + 9] += sdir.Y;
                VBO[vboIndex3 + 10] += sdir.Z;
            }
            for(int i = 0; i < Indices.Count; i++)
            {
                // 8 vbo stride
                int vboIndex1 = (int)Indices[i] * 11;
                var nor1 = new Vector3(VBO[vboIndex1 + 5], VBO[vboIndex1 + 6], VBO[vboIndex1 + 7]);
                var tan1 = new Vector3(VBO[vboIndex1 + 8], VBO[vboIndex1 + 9], VBO[vboIndex1 + 10]);
                var tan = (tan1 - nor1 * Vector3.Dot(nor1, tan1)).Normalized();
                VBO[vboIndex1 + 8] = tan.X;
                VBO[vboIndex1 + 9] = tan.Y;
                VBO[vboIndex1 + 10] = tan.Z;
            }
        }


        private void GenerateBuffers()
        {
            UpdateTangents();
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
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 4 * 11, 0);

            //Enabling 1 location in shaders - There will be UVs
            GL.EnableVertexAttribArray(1);
            // config for 0 location for shader, vec2, float, not normalized, 32 bytes total, stride 12 bytes
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * 11, 4 * 3);

            //Enabling 2 location in shaders
            GL.EnableVertexAttribArray(2);
            // config for 0 location for shader, vec3, float, not normalized, 32 bytes total, stride 12 bytes
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 4 * 11, 4 * 5);

            //Enabling 3 location in shaders
            GL.EnableVertexAttribArray(3);
            // config for 0 location for shader, vec3, float, not normalized, 32 bytes total, stride 12 bytes
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 4 * 11, 4 * 8);

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
                BumpMapName = "";
                NormapMapName = "";
                SpecularMapName = "";
                AlphaMask = "";
            }

            public Color DiffuseColor, SpecularColor, AmbientColor;
            public string TextureName, BumpMapName, NormapMapName, SpecularMapName;
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