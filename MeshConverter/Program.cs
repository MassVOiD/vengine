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
            Console.WriteLine("Done");

        }
    }
}
