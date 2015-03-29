using System;
using System.IO;
using VDGTech;

namespace MeshConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string mode = args[0];
            string infile = args[1];
            string outfile = args[2];

            if(mode == "separate")
            {
                Object3dInfo.CompressAndSave(infile, outfile);
            }
            else if(mode == "single")
            {
                Object3dInfo.CompressAndSaveSingle(infile, outfile);
            }
            else if(mode == "raw")
            {
                var element = Object3dInfo.LoadFromObjSingle(infile);

                MemoryStream vboStream = new MemoryStream();
                MemoryStream indicesStream = new MemoryStream();

                foreach(float v in element.VBO)
                    vboStream.Write(BitConverter.GetBytes(v), 0, 4);
                foreach(uint v in element.Indices)
                    indicesStream.Write(BitConverter.GetBytes(v), 0, 4);

                vboStream.Flush();
                indicesStream.Flush();

                if(File.Exists(outfile + ".vbo.raw"))
                    File.Delete(outfile + ".vbo.raw");
                File.WriteAllBytes(outfile + ".vbo.raw", vboStream.ToArray());

                if(File.Exists(outfile + ".indices.raw"))
                    File.Delete(outfile + ".indices.raw");
                File.WriteAllBytes(outfile + ".indices.raw", indicesStream.ToArray());
            }
            Console.WriteLine("Done");
        }
    }
}