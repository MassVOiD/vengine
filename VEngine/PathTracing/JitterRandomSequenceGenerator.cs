using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace VEngine.PathTracing
{
    public static class JitterRandomSequenceGenerator
    {
        private static Random Rand = new Random();
        public static List<float> Generate(int gridSize, int samples, bool shuffle)
        {
            if(samples % gridSize != 0)
                throw new Exception("Samples count must be divideable by grid size");
            var floats = new List<float>();
            int samplesPerGrid = samples / gridSize;
            for(int i = 0; i < gridSize; i++)
            {
                for(int g = 0; g < samplesPerGrid; g++)
                {
                    float val = (float)Rand.NextDouble() + i;
                    floats.Add(val);
                }
            }
            floats = floats.Select((a) => a / gridSize).ToList();
            if(shuffle)
                floats = floats.OrderBy((a) => Rand.Next(int.MinValue, int.MaxValue)).ToList();
            return floats;
        }

        private static float PI = 3.14159265f;

        private static float sin(float i)
        {
            return (float)Math.Sin(i);
        }
        private static float cos(float i)
        {
            return (float)Math.Cos(i);
        }
        private static float sqrt(float i)
        {
            return (float)Math.Sqrt(i);
        }

        private static Vector3 FromUV(float u, float v) {
            /* float phi = v * 2.0f * PI;
             float cosTheta = sqrt(1.0f - u);
             float sinTheta = sqrt(1.0f - cosTheta * cosTheta);
             return new Vector3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);*/
            float r = sqrt(1.0f - u * u);
            float phi = 2 * PI * v;

            return new Vector3(cos(phi) * r, sin(phi) * r, u);
        }

        public static List<Vector3> EvenlySampledHemisphere(int jitterGridSize, int samples)
        {
            var gridX = Generate(jitterGridSize, samples, false);
            var gridY = Generate(jitterGridSize, samples, false);
            var output = new List<Vector3>();
            foreach(var u in gridX)
                foreach(var v in gridY)
                    output.Add(FromUV(u, v));
            return output;
        }
    }
}