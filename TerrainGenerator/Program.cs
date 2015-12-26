using System;
using OpenTK;
using VEngine;
using VEngine.Generators;
using System.Text.RegularExpressions;
using System.Threading;

namespace TerrainGenerator
{
    internal class Program
    {

        class Settings
        {
            public float Width;
            public float Height;
        }

        private static void Main(string[] args)
        {
            string argsstr = string.Join(" ", args);

            Func<float, float, float> terrainGen = (x, y) =>
            {
                return
                    SimplexNoise.Noise.Generate(x, y) * 10 +
                    (SimplexNoise.Noise.Generate((float)x / 43, (float)y / 43) * 123) +
                    (SimplexNoise.Noise.Generate((float)x / 96, (float)y / 114) * 566) +
                    (SimplexNoise.Noise.Generate((float)x / 326, (float)y / 226) * 800) +
                    (SimplexNoise.Noise.Generate((float)x / 1111, (float)y / 3612) * 1700);
            };

            float segment = 16000.0f / 32.0f;
            int[] lods = new int[] { 16, 32, 48, 72, 128 };
            var pool = new Thread[4];
            for(int x = 0; x < 64; x++)
            {
                for(int y = 0; y < 64; y+=4)
                {
                    for(int tx = 0; tx < 4; tx ++)
                    {
                        int t = tx;
                        var thread = new Thread(new ThreadStart(() =>
                        {
                            float startx = segment * x;
                            float starty = segment * (y + t);
                            int l = 0;
                            foreach(var lod in lods)
                            {
                                Console.WriteLine("Working LOD " + l.ToString() + " :: " + x.ToString() + " " + (y + t).ToString());
                                var gen = Object3dGenerator.CreateTerrain(new Vector2(startx, starty), new Vector2(segment, segment), new Vector2(128, 128), Vector3.UnitY, lod, terrainGen);
                                gen.Vertices.ForEach((a) => a.Position -= new Vector3(-8000, 0, -8000));
                                gen.SaveRaw("outterrain/terrain_lod_"+l.ToString()+"_" + x.ToString() + "_" + (y + t).ToString() + ".raw");
                                l++;
                            }
                        }));
                        pool[t] = thread;
                        thread.Start();
                    }
                    pool[0].Join();
                    pool[1].Join();
                    pool[2].Join();
                    pool[3].Join();
                }
            }

           // Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-16000.0f, -16000.0f), new Vector2(16000.0f, 16000.0f), new Vector2(20, 20), Vector3.UnitY, 1024, terrainGen);
          //  Object3dInfo.CompressAndSave(groundInfo, "terrain4");
            Console.WriteLine("Done 4");

            Console.Read();
        }
    }
}