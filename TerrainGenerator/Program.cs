using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDGTech;
using VDGTech.Generators;
using OpenTK;
using System.Threading;

namespace TerrainGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = new Thread(() =>
            {
                Func<uint, uint, float> terrainGen = (x, y) =>
                {
                    return
                        SimplexNoise.Noise.Generate(x, y) +
                        (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 11) +
                        (SimplexNoise.Noise.Generate((float)x / 14, (float)y / 14) * 160) +
                        (SimplexNoise.Noise.Generate((float)x / 26, (float)y / 26) * 300);
                };
                Console.WriteLine("Working 1");
                Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-160000.0f, -160000.0f), new Vector2(160000.0f, 160000.0f), new Vector2(20, 20), Vector3.UnitY, 512, terrainGen);
                Object3dInfo.CompressAndSave(groundInfo, "terrain1");
                Console.WriteLine("Done 1");
            });
            var t2 = new Thread(() =>
            {
                Func<uint, uint, float> terrainGen = (x, y) =>
                {
                    return
                        SimplexNoise.Noise.Generate(x, y) * 2+
                        (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 11) +
                        (SimplexNoise.Noise.Generate((float)x / 14, (float)y / 14) * 360) +
                        (SimplexNoise.Noise.Generate((float)x / 26, (float)y / 26) * 300);
                };
                Console.WriteLine("Working 2");
                Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-160000.0f, -160000.0f), new Vector2(160000.0f, 160000.0f), new Vector2(20, 20), Vector3.UnitY, 512, terrainGen);
                Object3dInfo.CompressAndSave(groundInfo, "terrain2");
                Console.WriteLine("Done 2");
            });
            var t3 = new Thread(() =>
            {
                Func<uint, uint, float> terrainGen = (x, y) =>
                {
                    return
                        SimplexNoise.Noise.Generate(x, y) * 3+
                        (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 90) +
                        (SimplexNoise.Noise.Generate((float)x / 14, (float)y / 44) * 160) +
                        (SimplexNoise.Noise.Generate((float)x / 126, (float)y / 26) * 1111);
                };
                Console.WriteLine("Working 3");
                Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-160000.0f, -160000.0f), new Vector2(160000.0f, 160000.0f), new Vector2(20, 20), Vector3.UnitY, 512, terrainGen);
                Object3dInfo.CompressAndSave(groundInfo, "terrain3");
                Console.WriteLine("Done 3");
            });
            var t4 = new Thread(() =>
            {
                Func<uint, uint, float> terrainGen = (x, y) =>
                {
                    return
                        SimplexNoise.Noise.Generate(x, y) *10 +
                        (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 123) +
                        (SimplexNoise.Noise.Generate((float)x / 14, (float)y / 14) * 566) +
                        (SimplexNoise.Noise.Generate((float)x / 26, (float)y / 26) * 1700);
                };
                Console.WriteLine("Working 4");
                Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-160000.0f, -160000.0f), new Vector2(160000.0f, 160000.0f), new Vector2(20, 20), Vector3.UnitY, 512, terrainGen);
                Object3dInfo.CompressAndSave(groundInfo, "terrain4");
                Console.WriteLine("Done 4");
            });
            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            Console.Read();
        }
    }
}
