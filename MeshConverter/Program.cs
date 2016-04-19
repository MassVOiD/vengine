using System;
using System.IO;
using VEngine;
using Assimp;
using System.Text;
using OpenTK;
using System.Collections.Generic;

namespace MeshConverter
{
    internal class Program
    {
        static string ftos(float v)
        {
            return v.ToString().Replace(',', '.');
        }

        private static void Main(string[] args)
        {
            string mode = args[0];
            string infile = args[1];
            string outfile = args[2];
            Media.SearchPath = "media";

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
                    StringBuilder materialb = new StringBuilder();
                    var mat = m.GetLodLevel(0).Material;
                    var i3d = m.GetLodLevel(0).Info3d;
                    var inst = m.GetInstance(0);
                    string meshname = m.GetInstance(0).Name != null && m.GetInstance(0).Name.Length > 0 ? (m.GetInstance(0).Name + (unnamed++).ToString()) : ("unnamed_" + (unnamed++).ToString());
                    string matname = mat.Name != null && mat.Name.Length > 0 ? (mat.Name + (unnamed++).ToString()) : ("unnamed_" + (unnamed++).ToString());
                    meshname = meshname + ".mesh3d";
                    matname = matname + ".material";

                    string rawfile = i3d.Manager.Name;
                    i3d.Manager.ReverseYUV(1);
                    i3d.Manager.SaveRawWithTangents(outfile + "/" + rawfile);

                    materialb.AppendLine(string.Format("diffuse {0} {1} {2}", ftos(mat.DiffuseColor.X), ftos(mat.DiffuseColor.Y), ftos(mat.DiffuseColor.Z)));
                    materialb.AppendLine(string.Format("roughness {0}", ftos(mat.Roughness)));
                    materialb.AppendLine(string.Format("metalness {0}", ftos((float)rand.NextDouble())));
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
                    sceneb.AppendLine("mesh3d " + meshname);
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
            if(mode == "obj2rawtang")
            {
                var element = Object3dManager.LoadFromObjSingle(infile);
                element.SaveRawWithTangents(outfile);
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


            Console.WriteLine("Done");
        }
    }
}