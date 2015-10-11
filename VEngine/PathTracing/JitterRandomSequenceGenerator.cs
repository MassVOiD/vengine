using System;
using System.Collections.Generic;
using System.Linq;

namespace VEngine.PathTracing
{
    public static class JitterRandomSequenceGenerator
    {
        public static List<float> Generate(int gridSize, int samples, bool shuffle)
        {
            if(samples % gridSize != 0)
                throw new Exception("Samples count must be divideable by grid size");
            var rand = new Random();
            var floats = new List<float>();
            int samplesPerGrid = samples / gridSize;
            for(int i = 0; i < gridSize; i++)
            {
                for(int g = 0; g < samplesPerGrid; g++)
                {
                    float val = (float)rand.NextDouble() + i;
                    floats.Add(val);
                }
            }
            floats = floats.Select((a) => a / gridSize).ToList();
            if(shuffle)
                floats = floats.OrderBy((a) => rand.Next(int.MinValue, int.MaxValue)).ToList();
            return floats;
        }
    }
}