using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
