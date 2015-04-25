using System;
using OpenTK;
using VEngine;
using VEngine.Generators;

namespace TerrainGenerator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                return
                    SimplexNoise.Noise.Generate(x, y) * 10 +
                    (SimplexNoise.Noise.Generate((float)x / 43, (float)y / 43) * 123) +
                    (SimplexNoise.Noise.Generate((float)x / 96, (float)y / 114) * 566) +
                    (SimplexNoise.Noise.Generate((float)x / 326, (float)y / 226) * 1700);
            };
            Console.WriteLine("Working 4");
            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-16000.0f, -16000.0f), new Vector2(16000.0f, 16000.0f), new Vector2(20, 20), Vector3.UnitY, 1024, terrainGen);
            Object3dInfo.CompressAndSave(groundInfo, "terrain4");
            Console.WriteLine("Done 4");

            Console.Read();
        }
    }
}