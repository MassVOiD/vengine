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
             Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-2120, -1120), new Vector2(2120, 1120), new Vector2(-1, 1), Vector3.UnitY, 10, (x, y) => 0);
             var color = GenericMaterial.FromMedia("168.JPG");
            color.SetBumpMapFromMedia("testbump.png");
            color.Type = GenericMaterial.MaterialType.TessellatedTerrain;
            //color.SetBumpMapFromMedia("1bump.jpg");
            color.Roughness = 1.0f;
             Mesh3d water = new Mesh3d(waterInfo, color);
             water.SetMass(0);
            // color.Type = GenericMaterial.MaterialType.Grass;
            //  color.TesselationMultiplier = 0.1f;
            // water.Translate(0, -0.941f*2.0f, 0);
            // water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            //  Add(water);

            Object3dInfo gridx = Object3dInfo.LoadFromObjSingle(Media.Get("3axisplanes.obj"));
            var gcx = GenericMaterial.FromMedia("3daxisalbedo.png");
            gcx.SetNormalMapFromMedia("3daxisnormal.png");
            gcx.SetAlphaMaskFromMedia("3daxisalpha.png");
            gcx.Roughness = 1.0f;
           // gcx.IgnoreLighting = true;
            Mesh3d gex = new Mesh3d(gridx, gcx);
            gex.SetMass(0);
           // gex.Translate(0, x, 0);
            Add(gex);

            
           /* for(int z = 0; z < 20; z++)
            {
                Object3dInfo grid = Object3dGenerator.CreateTerrain(new Vector2(-20, -20), new Vector2(20, 20), new Vector2(-4, 4), Vector3.UnitY, 2, (x2, y2) => 0);
                var gc = new GenericMaterial(Color.White);
                gc.SetAlphaMaskFromMedia("gridalphamask.png");
                gc.Roughness = 1.0f;
                gc.IgnoreLighting = true;
                Mesh3d ge = new Mesh3d(grid, gc);
                ge.SetMass(0);
                ge.Translate(z, 0, 0);
                ge.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(90)));
                Add(ge);

            }*//*
            var lod1 = Object3dInfo.LoadSceneFromObj(Media.Get("shipment.obj"), Media.Get("shipment.mtl"));
             lod1.ForEach((a) => Add(a));
            
            var chair = new Mesh3d(lod1, GenericMaterial.FromMedia("Cerberus_A.png"));
            chair.MainMaterial.SetNormalMapFromMedia("Cerberus_N.png");
            chair.MainMaterial.SetSpecularMapFromMedia("Cerberus_M.png");
            chair.MainMaterial.SetMetalnessMapFromMedia("Cerberus_M.png");
            chair.MainMaterial.SetRoughnessMapFromMedia("Cerberus_R.png");
            chair.Scale(10);
            Add(chair);*/

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
