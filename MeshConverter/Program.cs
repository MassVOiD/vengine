using System;
using System.IO;
using VEngine;

namespace MeshConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string mode = args[0];
            string infile = args[1];
            string outfile = args[2];
            
            if(mode == "raw")
            {
                var element = Object3dManager.LoadFromObjSingle(infile);
                element.SaveRaw(outfile);
            }
            Console.WriteLine("Done");
        }
    }
}