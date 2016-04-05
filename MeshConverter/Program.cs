using System;
using System.IO;
using VEngine;
using Assimp;
using OpenTK;
using System.Collections.Generic;

namespace MeshConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string mode = args[0];
            string infile = args[1];
            string outfile = args[2];

            if(mode == "obj2raw")
            {
                var element = Object3dManager.LoadFromObjSingle(infile);
                element.SaveRaw(outfile);
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