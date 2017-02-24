using System;
using System.IO;
using VEngine;
using Assimp;
using System.Text;
using OpenTK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeshConverter
{
    internal class Program
    {
        static string ftos(float v)
        {
            return v.ToString().Replace(',', '.');
        }

        static Dictionary<string, string> Arguments;

        static void RequireArgs(params string[] args)
        {
            bool fail = false;
            foreach(var a in args)
            {
                if(!Arguments.ContainsKey(a))
                {
                    Console.WriteLine("Argument not found: " + a);
                    fail = true;
                }
            }
            if(fail)
                throw new ArgumentException();
        }


        static bool doublefaced = false;
        static void SaveMeshToFile(Mesh3d m, string name, string outfile)
        {
            StringBuilder materialb = new StringBuilder();
            var mat = m.GetLodLevel(0).Material;
            var i3d = m.GetLodLevel(0).Info3d;
            var inst = m.GetInstance(0);
            string meshname = name + ".mesh3d";
            string matname = name + ".material";

            string rawfile = name + ".raw";
            i3d.Manager.ReverseYUV(1);
            i3d.Manager.SaveRawWithTangents(outfile + "/" + rawfile);

            materialb.AppendLine(string.Format("diffuse {0} {1} {2}", ftos(mat.DiffuseColor.X), ftos(mat.DiffuseColor.Y), ftos(mat.DiffuseColor.Z)));
            materialb.AppendLine(string.Format("roughness {0}", ftos(mat.Roughness)));
            materialb.AppendLine(string.Format("metalness {0}", 0.2f));
            materialb.AppendLine();
            if(mat.NormalsTexture != null)
            {
                materialb.AppendLine("node");
                materialb.AppendLine(string.Format("texture {0}", mat.NormalsTexture.FileName));
                materialb.AppendLine("mix REPLACE");
                materialb.AppendLine("target NORMAL");
                materialb.AppendLine();
            }
            if(mat.DiffuseTexture != null)
            {
                materialb.AppendLine("node");
                materialb.AppendLine(string.Format("texture {0}", mat.DiffuseTexture.FileName));
                materialb.AppendLine("mix REPLACE");
                materialb.AppendLine("target DIFFUSE");
                materialb.AppendLine("modifier LINEARIZE");
                materialb.AppendLine();
            }
            if(mat.BumpTexture != null)
            {
                materialb.AppendLine("node");
                materialb.AppendLine(string.Format("texture {0}", mat.BumpTexture.FileName));
                materialb.AppendLine("mix REPLACE");
                materialb.AppendLine("target BUMP");
                materialb.AppendLine();
            }
            if(mat.RoughnessTexture != null)
            {
                materialb.AppendLine("node");
                materialb.AppendLine(string.Format("texture {0}", mat.RoughnessTexture.FileName));
                materialb.AppendLine("mix REPLACE");
                materialb.AppendLine("target ROUGHNESS");
                materialb.AppendLine();
            }
            File.WriteAllText(outfile + "/" + matname, materialb.ToString());

            StringBuilder meshb = new StringBuilder();
            meshb.AppendLine("lodlevel");
            meshb.AppendLine("start 0");
            meshb.AppendLine("end 99999");
            meshb.AppendLine(string.Format("info3d {0}", rawfile));
            meshb.AppendLine(string.Format("material {0}", matname));
            meshb.AppendLine();
            meshb.AppendLine("instance");
            meshb.AppendLine(string.Format("translate {0} {1} {2}", ftos(inst.Transformation.Position.R.X), ftos(inst.Transformation.Position.R.Y), ftos(inst.Transformation.Position.R.Z)));
            meshb.AppendLine(string.Format("scale {0} {1} {2}", ftos(inst.Transformation.ScaleValue.R.X), ftos(inst.Transformation.ScaleValue.R.Y), ftos(inst.Transformation.ScaleValue.R.Z)));
            meshb.AppendLine(string.Format("rotate {0} {1} {2} {3}", ftos(inst.Transformation.Orientation.R.X), ftos(inst.Transformation.Orientation.R.Y), ftos(inst.Transformation.Orientation.R.Z), ftos(inst.Transformation.Orientation.R.W)));
            File.WriteAllText(outfile + "/" + meshname, meshb.ToString());
        }

        static Dictionary<int, string> matdict = new Dictionary<int, string>();
        static int unnamed = 0;
        static List<string> usednames = new List<string>();
        static string mode, infile, outfile;
        private static void Main(string[] args)
        {
            Arguments = new Dictionary<string, string>();
            foreach(var a in args)
            {
                int i = a.IndexOf('=');
                if(i > 0)
                {
                    string k = a.Substring(0, i);
                    string v = a.Substring(i + 1);
                    Arguments.Add(k, v);
                }
            }
            mode = args[0];
            infile = args[1];
            outfile = args[2];
            Media.SearchPath = ".";

            if(mode == "scene2assets")
            {
                string scenename = args[3];
                var element = new VEngine.FileFormats.GameScene(infile);
                element.Load();
                int unnamed = 0;
                StringBuilder sceneb = new StringBuilder();
                var rand = new Random();
                foreach(var m in element.Meshes)
                {
                    string name = "mesh" + (unnamed++);
                    SaveMeshToFile(m, name, outfile);
                    sceneb.AppendLine("mesh3d " + name + ".mesh3d");
                }
                File.WriteAllText(outfile + "/" + scenename, sceneb.ToString());
            }
            if(mode == "obj2raw")
            {
                var element = Object3dManager.LoadFromObjSingle(infile);
                element.SaveRaw(outfile);
            }
            if(mode == "obj2rawtangsmooth")
            {
                var element = Object3dManager.LoadFromObjSingle(infile);
                element.RecalulateNormals(Object3dManager.NormalRecalculationType.Smooth, 1);
                element.SaveRawWithTangents(outfile);
            }
            if(mode == "raw2rawtang")
            {
                var element = Object3dManager.LoadFromRaw(infile);
                element.SaveRawWithTangents(outfile);
            }
            if(mode == "raw2rawtangfake")
            {
                var element = Object3dManager.LoadFromRaw(infile);
                element.SaveRawWithTangents2(outfile);
            }
            if(mode == "obj2rawtang")
            {
                var element = Object3dManager.LoadFromObjSingle(infile);
                element.SaveRawWithTangents(outfile);
            }
            if(mode == "objscene2assets")
            {
                Console.WriteLine("Conversion started");
                var elements = Object3dManager.LoadSceneFromObj(infile + ".obj", infile + ".mtl");

                var map = new List<string>();
                var r = new Random();
                StringBuilder sceneb = new StringBuilder();
                string scenename = args[3];

                Console.WriteLine("Found elements " + elements.Count);
                foreach(var m in elements)
                {
                    string n = m.GetInstance(0).Name;
                    if(n == null || n.Length == 0 || map.Contains(n))
                        n = m.GetLodLevel(0).Info3d.Manager.Name;
                    if(n == null || n.Length == 0 || map.Contains(n))
                        n = m.GetLodLevel(0).Material.Name;
                    if(n == null || n.Length == 0 || map.Contains(n))
                        n = Path.GetFileNameWithoutExtension(m.GetLodLevel(0).Material.DiffuseTexture.FileName);
                    if(n == null || n.Length == 0 || map.Contains(n))
                        n = Path.GetFileNameWithoutExtension(m.GetLodLevel(0).Material.BumpTexture.FileName);
                    if(n == null || n.Length == 0 || map.Contains(n))
                        n = Path.GetFileNameWithoutExtension(m.GetLodLevel(0).Material.NormalsTexture.FileName);
                    while(n == null || n.Length == 0 || map.Contains(n))
                        n = "unknown_" + r.Next();
                    Console.WriteLine("Converting mesh " + n);

                    SaveMeshToFile(m, n, outfile);
                    sceneb.AppendLine("mesh3d " + n + ".mesh3d");
                }
                Console.WriteLine("Saving scene");
                File.WriteAllText(outfile + "/" + scenename, sceneb.ToString());
            }

            if(mode == "ply2raw")
            {
                var ai = new AssimpContext();
                var vertexinfos = new List<VertexInfo>();
                var mi = ai.ImportFile(infile, PostProcessSteps.Triangulate);
                foreach(var m in mi.Meshes)
                {
                    var indices = m.GetIndices();

                    for(int i = 0; i < indices.Length; i++)
                    {
                        int f = indices[i];
                        var vp = m.Vertices[f];
                        var vn = m.Normals[f];
                        var vt = (m.TextureCoordinateChannels.Length == 0 || m.TextureCoordinateChannels[0].Count <= f) ? new Assimp.Vector3D(0) : m.TextureCoordinateChannels[0][f];
                        var vi = new VertexInfo()
                        {
                            Position = new Vector3(vp.X, vp.Y, vp.Z),
                            Normal = new Vector3(vn.X, vn.Y, vn.Z),
                            UV = new Vector2(vt.X, vt.Y)
                        };
                        vertexinfos.Add(vi);
                    }
                }

                var element = new Object3dManager(vertexinfos);
                element.SaveRaw(outfile);
            }
            if(mode == "generateterrain")
            {
                RequireArgs("resolution", "parts", "in", "inx", "iny", "out", "size", "uvscale", "height");
                var imgraw = File.ReadAllBytes(Arguments["in"]);
                var imgdata = new short[imgraw.Length / 2];
                Buffer.BlockCopy(imgraw, 0, imgdata, 0, imgraw.Length);
                int resolution = int.Parse(Arguments["resolution"]);
                int imgwidth = int.Parse(Arguments["inx"]);
                int imgheight = int.Parse(Arguments["iny"]);
                string ofile = Arguments["out"];
                float size = float.Parse(Arguments["size"], System.Globalization.CultureInfo.InvariantCulture) / 2.0f;
                float uvscale = float.Parse(Arguments["uvscale"], System.Globalization.CultureInfo.InvariantCulture);
                float height = float.Parse(Arguments["height"], System.Globalization.CultureInfo.InvariantCulture);
                int lx = 1;
                int parts = int.Parse(Arguments["parts"]);
                var start = new Vector2(-size);
                var end = new Vector2(size);
                var stepsize = (end - start) / ((float)parts);
                var realstepsize = (1.0f) / ((float)parts);
                for(int ApartX = 0; ApartX < parts; ApartX++)
                {
                    var tasks = new List<Task>();
                    for(int ApartY = 0; ApartY < parts; ApartY++)
                    {
                        int partX = ApartX;
                        int partY = ApartY;
                        var tx = new Task(new Action(() =>
                        {
                            var partstart = start + new Vector2(stepsize.X * (float)partX, stepsize.Y * (float)partY);
                            var partend = start + new Vector2(stepsize.X * (float)(partX + 1), stepsize.Y * (float)(partY + 1));
                            var t = VEngine.Generators.Object3dGenerator.CreateTerrain(partstart, partend, new Vector2(uvscale), Vector3.UnitY, resolution, (x, y) =>
                            {
                                float rx = realstepsize * (float)partX + realstepsize * (x);
                                float ry = realstepsize * (float)partY + realstepsize * (y);
                                int xpx = (int)(rx * imgwidth);
                                int ypx = (int)(ry * imgheight);
                                if(xpx >= imgwidth)
                                    xpx = imgwidth - 1;
                                if(ypx >= imgheight)
                                    ypx = imgheight - 1;
                                byte b0 = imgraw[(xpx + ypx * imgwidth) * 2];
                                byte b1 = imgraw[(xpx + ypx * imgwidth) * 2 + 1];
                                var col = BitConverter.ToUInt16(new byte[] { b0, b1 }, 0);
                                int zxzs = (int)(x * 100.0);
                                if(zxzs > lx)
                                    Console.WriteLine(zxzs);
                                if(zxzs > lx)
                                    lx = zxzs;
                                return ((float)(col) / ushort.MaxValue) * height;
                            });
                            Console.WriteLine("Starting saving " + ofile + "_" + partX + "x" + partY + ".raw");
                            t.ExtractTranslation2DOnly();
                            //t.RecalulateNormals(Object3dManager.NormalRecalculationType.Smooth, 0.0f);
                            t.Vertices.ForEach((a) =>
                            {
                                a.UV.X = realstepsize * (float)partX + realstepsize * (a.UV.X);
                                a.UV.Y = realstepsize * (float)partY + realstepsize * (a.UV.Y);
                            });
                            t.SaveRawWithTangents(ofile + "_" + partX + "x" + partY + ".raw");
                        }));
                        tasks.Add(tx);
                    }
                    tasks.ForEach((a) => a.Start());
                    tasks.ForEach((a) => a.Wait());
                }
            }
            if(mode == "generateterraingrass")
            {
                RequireArgs("resolution", "in", "out", "size", "uvscale", "height", "threshold");
                var img = new System.Drawing.Bitmap(Arguments["in"]);
                int resolution = int.Parse(Arguments["resolution"]);
                string ofile = Arguments["out"];
                float size = float.Parse(Arguments["size"], System.Globalization.CultureInfo.InvariantCulture) / 2.0f;
                float uvscale = float.Parse(Arguments["uvscale"], System.Globalization.CultureInfo.InvariantCulture);
                float height = float.Parse(Arguments["height"], System.Globalization.CultureInfo.InvariantCulture);
                float threshold = float.Parse(Arguments["threshold"], System.Globalization.CultureInfo.InvariantCulture);
                int lx = 1;
                var start = new Vector2(-size);
                var end = new Vector2(size);
                var mixdir = (end - start);

                StringBuilder sb = new StringBuilder();

                var gethfn = new Func<float, float, float>((x, y) =>
                {
                    int xpx = (int)(x * img.Size.Width);
                    int ypx = (int)(y * img.Size.Height);
                    if(xpx >= img.Size.Width)
                        xpx = img.Size.Width - 1;
                    if(ypx >= img.Size.Height)
                        ypx = img.Size.Height - 1;
                    var col = img.GetPixel(xpx, ypx);
                    int zxzs = (int)(x * 100.0);
                    // if(zxzs > lx)
                    //  Console.WriteLine(zxzs);
                    if(zxzs > lx)
                        lx = zxzs;
                    return ((float)(col.R) / 255.0f) * height;
                });

                for(int x = 0; x < resolution; x++)
                {
                    for(int y = 0; y < resolution; y++)
                    {
                        float fx = (float)x / (float)resolution;
                        float fy = (float)y / (float)resolution;
                        float h = gethfn(fx, fy);
                        //  Console.WriteLine(h);
                        if(h > threshold)
                        {
                            Vector2 vx = start + mixdir * new Vector2(fx, fy);
                            sb.AppendLine("instance");
                            sb.Append("translate ");
                            sb.Append(vx.X);
                            sb.Append(" ");
                            sb.Append(h);
                            sb.Append(" ");
                            sb.Append(vx.Y);
                            sb.AppendLine();
                        }
                    }
                }
                File.WriteAllText(ofile, sb.ToString());


            }
            if(mode == "assimp2assets")
            {
                // convert.exe assimp2assets infile.dae outdir outname.scene
                var ai = new AssimpContext();
                var usednames = new List<string>();
                var mi = ai.ImportFile(infile, PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals);

                int cnt = 0;
                var scenesb = new StringBuilder();
                string scenename = args[3];
                doublefaced = args.Length == 5 && args[4] == "doublefaced";
                foreach(var m in mi.Materials)
                {
                    string name = usednames.Contains(m.Name) ? (m.Name + (unnamed++).ToString()) : m.Name;
                    var sb = new StringBuilder();
                    sb.AppendLine(string.Format("diffuse {0} {1} {2}", ftos(m.ColorDiffuse.R), ftos(m.ColorDiffuse.G), ftos(m.ColorDiffuse.B)));
                    sb.AppendLine(string.Format("roughness {0}", ftos(1.0f / (m.Shininess + 1.0f))));
                    sb.AppendLine(string.Format("metalness {0}", ftos(0.0f)));
                    sb.AppendLine();

                    if(m.HasTextureDiffuse)
                    {
                        sb.AppendLine("node");
                        sb.AppendLine(string.Format("texture {0}", Path.GetFileName(m.TextureDiffuse.FilePath)));
                        sb.AppendLine("mix REPLACE");
                        sb.AppendLine("target DIFFUSE");
                        sb.AppendLine("modifier LINEARIZE");
                        sb.AppendLine();
                    }
                    if(m.HasTextureReflection)
                    {
                        sb.AppendLine("node");
                        sb.AppendLine(string.Format("texture {0}", Path.GetFileName(m.TextureReflection.FilePath)));
                        sb.AppendLine("mix REPLACE");
                        sb.AppendLine("target ROUGHNESS");
                        sb.AppendLine();
                    }
                    if(m.HasTextureSpecular)
                    {
                        sb.AppendLine("node");
                        sb.AppendLine(string.Format("texture {0}", Path.GetFileName(m.TextureSpecular.FilePath)));
                        sb.AppendLine("mix REPLACE");
                        sb.AppendLine("target ROUGHNESS");
                        sb.AppendLine();
                    }
                    if(m.HasTextureNormal)
                    {
                        sb.AppendLine("node");
                        sb.AppendLine(string.Format("texture {0}", Path.GetFileName(m.TextureNormal.FilePath)));
                        sb.AppendLine("mix REPLACE");
                        sb.AppendLine("target NORMAL");
                        sb.AppendLine();
                    }
                    if(m.HasTextureDisplacement)
                    {
                        sb.AppendLine("node");
                        sb.AppendLine(string.Format("texture {0}", Path.GetFileName(m.TextureDisplacement.FilePath)));
                        sb.AppendLine("mix REPLACE");
                        sb.AppendLine("target BUMP");
                        sb.AppendLine();
                    }
                    Console.WriteLine("Saving " + outfile + "/" + name + ".material");
                    File.WriteAllText(outfile + "/" + name + ".material", sb.ToString());
                    matdict.Add(cnt, outfile + "/" + name + ".material");
                    cnt++;
                }
                recurseNode(mi, mi.RootNode, scenesb, Matrix4x4.Identity);
                Console.WriteLine("Saving " + outfile + "/" + scenename);
                File.WriteAllText(outfile + "/" + scenename, scenesb.ToString());

            }


            Console.WriteLine("Done");
        }

        static void recurseNode(Assimp.Scene scn, Node node, StringBuilder scenesb, Matrix4x4 matrix)
        {
            Console.WriteLine("Scanning node " + node.Name);
            foreach(var mindex in node.MeshIndices)
            {
                var m = scn.Meshes[mindex];
                string name = usednames.Contains(m.Name) ? (m.Name + (unnamed++).ToString()) : m.Name;
                var sb = new StringBuilder();
                if(!File.Exists(outfile + "/" + name + ".mesh3d"))
                {
                    var vertexinfos = new List<VertexInfo>();
                    var indices = m.GetIndices();

                    for(int i = 0; i < indices.Length; i++)
                    {
                        int f = indices[i];
                        var vp = m.Vertices[f];
                        var vn = m.Normals[f];
                        var vt = (m.TextureCoordinateChannels.Length == 0 || m.TextureCoordinateChannels[0].Count <= f) ? new Assimp.Vector3D(0) : m.TextureCoordinateChannels[0][f];
                        var vi = new VertexInfo()
                        {
                            Position = new Vector3(vp.X, vp.Y, vp.Z),
                            Normal = new Vector3(vn.X, vn.Y, vn.Z),
                            UV = new Vector2(vt.X, vt.Y)
                        };
                        vertexinfos.Add(vi);
                    }
                    if(doublefaced)
                    {

                        for(int i = indices.Length - 1; i >= 0; i--)
                        {
                            int f = indices[i];
                            var vp = m.Vertices[f];
                            var vn = m.Normals[f];
                            var vt = (m.TextureCoordinateChannels.Length == 0 || m.TextureCoordinateChannels[0].Count <= f) ? new Assimp.Vector3D(0) : m.TextureCoordinateChannels[0][f];
                            var vi = new VertexInfo()
                            {
                                Position = new Vector3(vp.X, vp.Y, vp.Z),
                                Normal = new Vector3(vn.X, vn.Y, vn.Z),
                                UV = new Vector2(vt.X, vt.Y)
                            };
                            vertexinfos.Add(vi);
                        }
                    }
                    var element = new Object3dManager(vertexinfos);
                    Console.WriteLine("Saving " + outfile + "/" + name + ".raw");
                    element.SaveRawWithTangents(outfile + "/" + name + ".raw");

                    string matname = matdict[m.MaterialIndex];
                    scenesb.AppendLine("mesh3d " + outfile + "/" + name + ".mesh3d");
                    sb.AppendLine("lodlevel");
                    sb.AppendLine("start 0.0");
                    sb.AppendLine("end 99999.0");
                    sb.AppendLine("info3d " + outfile + "/" + name + ".raw");
                    sb.AppendLine("material " + matname);
                    sb.AppendLine();
                }
                sb.AppendLine("instance");
                Assimp.Vector3D pvec, pscl;
                Assimp.Quaternion pquat;
                var a = new Assimp.Quaternion(new Vector3D(1, 0, 0), MathHelper.DegreesToRadians(-90)).GetMatrix();
                var a2 = Matrix4x4.FromScaling(new Vector3D(0.01f));
                (matrix * node.Transform * new Assimp.Matrix4x4(a) * a2).Decompose(out pscl, out pquat, out pvec);
                var q1 = new OpenTK.Quaternion(pquat.X, pquat.Y, pquat.Z, pquat.W).Inverted();
                var m1 = Matrix4.CreateFromQuaternion(q1);
                m1[2, 0] = -m1[2, 0];
                m1[2, 1] = -m1[2, 1];
                m1[0, 2] = -m1[0, 2];
                m1[1, 2] = -m1[1, 2];
                var q2 = m1.ExtractRotation(true);

                sb.AppendLine(string.Format("translate {0} {1} {2}", ftos(pvec.X), ftos(pvec.Y), ftos(pvec.Z)));
                sb.AppendLine(string.Format("rotate {0} {1} {2} {3}", ftos(q1.X), ftos(q1.Y), ftos(q1.Z), ftos(q1.W)));
                sb.AppendLine(string.Format("scale {0} {1} {2}", ftos(pscl.X), ftos(pscl.Y), ftos(pscl.Z)));
                if(!File.Exists(outfile + "/" + name + ".mesh3d"))
                {
                    Console.WriteLine("Saving " + outfile + "/" + name + ".mesh3d");
                    File.WriteAllText(outfile + "/" + name + ".mesh3d", sb.ToString());
                }
                else
                {
                    Console.WriteLine("Extending " + outfile + "/" + name + ".mesh3d");
                    File.AppendAllText(outfile + "/" + name + ".mesh3d", sb.ToString());
                }
            }
            foreach(var c in node.Children)
                if(c != node)
                    recurseNode(scn, c, scenesb, matrix * node.Transform);
        }
    }
}