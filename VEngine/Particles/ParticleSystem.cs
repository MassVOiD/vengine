using System.Collections.Generic;

namespace VDGTech.Particles
{
    public class ParticleSystem
    {
        public static List<ParticleGenerator> Generators = new List<ParticleGenerator>();

        public static void DrawAll(bool depthOnly = false)
        {
            Generators.ForEach(a => a.Draw(depthOnly));
        }
    }
}