using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDGTech;

namespace MeshConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string infile = args[0];
            string outfile = args[1];
            Object3dInfo.CompressAndSave(infile, outfile);
            Console.WriteLine("Done");
        }
    }
}
