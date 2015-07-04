using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using VEngine;
using VEngine.Generators;
using OpenTK;

namespace ShadowsTester
{
    public class PlanetScene : Scene
    {
        public PlanetScene()
        {
            var sphere = Object3dInfo.LoadFromObjSingle(Media.Get("planet.obj"));
            var planet = new Mesh3d(sphere, new GenericMaterial(Color.Red));
            planet.Scale(30000);
            (planet.MainMaterial as GenericMaterial).Type = GenericMaterial.MaterialType.PlanetSurface;
            (planet.MainMaterial as GenericMaterial).TesselationMultiplier = 0.007f;
            planet.Translate(0, -30500, 0);
            
            Add(planet);
        }

    }
}
