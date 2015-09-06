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
    public class DragonScene : Scene
    {
        public DragonScene()
        {
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(3000);
            whitebox.Translate(0, -1500, 0);
            Add(whitebox);
          /*  Func<uint, uint, float> terrainGen = (x, y) =>
            {
                return
                    (SimplexNoise.Noise.Generate((float)x , (float)y) * 12) +
                    (SimplexNoise.Noise.Generate((float)x / 11, (float)y / 22) * 70) +
                    (SimplexNoise.Noise.Generate((float)x / 210, (float)y / 228) * 118) +
                    (SimplexNoise.Noise.Generate((float)x / 634, (float)y / 532) * 555) +
                    (SimplexNoise.Noise.Generate((float)x / 1696, (float)y / 1793) * 870);
            };
            Object3dGenerator.UseCache = false;
            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-3000, -3000), new Vector2(3000, 3000), new Vector2(1120, 1120), Vector3.UnitY, 800, terrainGen);
            */
             Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY, 333, (x, y) => 0);
             var color = GenericMaterial.FromMedia("bluetex.png");
            //color.SetNormalMapFromMedia("06_NORMAL.jpg");
            color.SetBumpMapFromMedia("1bump.jpg");
            color.Roughness = 1.0f;
             Mesh3d water = new Mesh3d(waterInfo, color);
             water.SetMass(0);
           // color.Type = GenericMaterial.MaterialType.Grass;
          //  color.TesselationMultiplier = 0.1f;
            // water.Translate(0, -0.941f*2.0f, 0);
            // water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
             Add(water);
            
            var lod1 = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            lod1.ScaleUV(8.0f);

            var chairInfo = Object3dInfo.LoadFromObjSingle(Media.Get("nicechair.obj"));
            var chair = new Mesh3d(lod1, GenericMaterial.FromMedia("168.JPG"));
           // var chair = new Mesh3d(lod1, new GenericMaterial(Color.Yellow));
          //  chair.MainMaterial.Roughness = 0.7f;
          //  chair.MainMaterial.SetNormalMapFromMedia("clothnorm.png");
          //  chair.MainMaterial.ReflectionStrength = 1.0f;
            Add(chair);
           /* var scene = Object3dInfo.LoadSceneFromObj(Media.Get("gold.obj"), Media.Get("gold.mtl"), 1.0f);
            foreach(var ob in scene)
            {
                ob.SetMass(0);
                ob.MainMaterial.Roughness = 0.05f;
                ob.MainMaterial.Metalness = 0.7f;
                ob.MainMaterial.Color = new Vector4(((float)229 / (float)0xFF), ((float)179 / (float)0xFF), ((float)44 / (float)0xFF), 1);
                ob.MainMaterial.Mode = GenericMaterial.DrawMode.ColorOnly;
                this.Add(ob);
            }*/

        }

    }
}
