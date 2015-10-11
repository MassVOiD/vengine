using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.Generators;

namespace ShadowsTester
{
    public class DragonScene
    {
        public DragonScene()
        {
            var scene = World.Root.RootScene;
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(3000);
            whitebox.Translate(0, -1500, 0);
            scene.Add(whitebox);
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
            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-3000, -3000), new Vector2(3000, 3000), new Vector2(1120, 1120), Vector3.UnitY, 11, (x, y) => 0);
            Object3dInfo waterInfo = Object3dInfo.LoadFromRaw(Media.Get("Realistic tree.vbo.raw"), Media.Get("Realistic tree.indices.raw"));
            Object3dInfo waterInfo2 = Object3dInfo.LoadFromObjSingle(Media.Get("terrain_simplified_normalized.obj"));
            Object3dInfo[] domks = Object3dInfo.LoadFromObj(Media.Get("test_ball.obj"));

            var roof = new Mesh3d(domks[0], new GenericMaterial(Color.White));
            var walls = new Mesh3d(domks[1], new GenericMaterial(Color.White));
            // scene.Add(roof); scene.Add(walls);

            var color = new GenericMaterial(Color.White);
            //color.SetNormalMapFromMedia("151_norm.JPG");
            color.Roughness = 1.0f;
            //color.SetBumpMapFromMedia("terrain_grassmap.png");
            // color.Type = GenericMaterial.MaterialType.Grass;
            var color2 = GenericMaterial.FromMedia("coastbeach01.jpg");
            color2.SetNormalMapFromMedia("coastbeach01_n.jpg");
            waterInfo.Normalize();
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.Scale(2.2f);
            scene.Add(water);

            Mesh3d water2 = new Mesh3d(groundInfo, color2);
            water2.SetMass(0);
            //water2.Scale(300);
            // color2.SetBumpMapFromMedia("usamap.png");
            //color2.Type = GenericMaterial.MaterialType.TessellatedTerrain;
            scene.Add(water2);

            /*for(int i = 0; i < 12; i++)
            {
                // Object3dInfo gridx = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
                Object3dInfo gridx = Object3dInfo.LoadFromObjSingle(Media.Get("flagplane.obj"));
                gridx.Normalize();
                var gcx = new GenericMaterial(Color.White);
                gcx.Type = GenericMaterial.MaterialType.Flag;
                gcx.Roughness = 1.0f;
                // gcx.IgnoreLighting = true;
                Mesh3d gex = new Mesh3d(gridx, gcx);
                gex.SetMass(0);
                gex.Scale(4f, 12, 4);
                gex.Translate(i * 4, 0, 0);
                scene.Add(gex);
            }*/

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