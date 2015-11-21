using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VEngine;
using VEngine.FileFormats;
using VEngine.Generators;

namespace ShadowsTester
{
    public class DragonScene
    {
        class VegetationPart
        {
            public string Texture, Model;
            public float Scale, ScaleVariation;
            public int Count;
            public VegetationPart(string t, string m, float s, float sv, int c)
            {
                Texture = t;
                Model = m;
                Scale = s;
                ScaleVariation = sv;
                Count = c;
            }
        }
        public DragonScene()
        {
            var scene = World.Root.RootScene;
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = Mesh3d.Create(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.GetInstance(0).Scale(3000);
            whitebox.GetInstance(0).Translate(0, -1500, 0);
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
            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-12, -12), new Vector2(12, 12), new Vector2(600, 600), Vector3.UnitY, 3, (x, y) => 0);
            //var color2 = GenericMaterial.FromMedia("3a.jpg", "3n.jpg");
            var color2 = new GenericMaterial(Color.White);
            Mesh3d water3 = Mesh3d.Create(groundInfo, color2);
            water3.GetInstance(0).Scale(30);
            scene.Add(water3);


            Random rand = new Random();
            /*
            List<VegetationPart> vegs = new List<VegetationPart>();
            // grass
            vegs.Add(new VegetationPart("vurt_reachflowers2.dds", "6billboardsgrass.obj", 2.0f, 1.4f, 38001));
            vegs.Add(new VegetationPart("vurt_AssortedPlants.dds", "6billboardsgrass.obj", 2.0f, 1.2f, 35010));
            vegs.Add(new VegetationPart("vurt_brownplants.dds", "6billboardsgrass.obj", 2.0f, 2.2f, 36010));
            vegs.Add(new VegetationPart("snowgrass01.dds", "6billboardsgrass.obj", 2.4f, 1.2f, 37010));

            //tress
            vegs.Add(new VegetationPart("vurt_ heather.dds", "4billboardstrees.obj", 15.0f, 6.6f, 89));
            vegs.Add(new VegetationPart("vurt_aspenleaves.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));
            vegs.Add(new VegetationPart("vurt_FFleavesG2.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));
            vegs.Add(new VegetationPart("vurt_PineSnowy.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));

            float vegarea = 340;
            foreach(var v in vegs)
            {
                Mesh3d ioc = Mesh3d.Create(Object3dInfo.LoadFromObjSingle(Media.Get(v.Model)), new GenericMaterial(Color.White));
                ioc.ClearInstances();
                ioc.GetLodLevel(0).Material.SetTextureFromMedia(v.Texture);
                for(int i = 0; i < v.Count; i++)
                    ioc.AddInstance(new Mesh3dInstance(new TransformationManager(new Vector3((float)(rand.NextDouble() * 2.0 - 1.0) * vegarea,
                        0.0f,
                        (float)(rand.NextDouble() * 2.0 - 1.0) * vegarea), Quaternion.FromAxisAngle(Vector3.UnitY, (float)(rand.NextDouble() + 0.25f) * 3.0f), (float)(rand.NextDouble()) * v.ScaleVariation + v.Scale), v.Model));
                ioc.UpdateMatrix();
                scene.Add(ioc);
            }
            */

            Object3dInfo ml0 = Object3dInfo.LoadFromObjSingle(Media.Get("monkey0.obj"));
            Object3dInfo ml1 = Object3dInfo.LoadFromObjSingle(Media.Get("monkey1.obj"));
            Object3dInfo ml2 = Object3dInfo.LoadFromObjSingle(Media.Get("monkey2.obj"));
            Object3dInfo ml3 = Object3dInfo.LoadFromObjSingle(Media.Get("monkey3.obj"));

            GenericMaterial gm0 = new GenericMaterial(Color.Blue);
            GenericMaterial gm1 = new GenericMaterial(Color.Green);
            GenericMaterial gm2 = new GenericMaterial(Color.Yellow);
            GenericMaterial gm3 = new GenericMaterial(Color.Red);

            LodLevel ll0 = new LodLevel(ml0, gm0, 0, 10);
            LodLevel ll1 = new LodLevel(ml1, gm1, 10, 20);
            LodLevel ll2 = new LodLevel(ml2, gm2, 20, 30);
            LodLevel ll3 = new LodLevel(ml3, gm3, 30, 200);

            Mesh3d ioc = Mesh3d.Empty;
            ioc.AddLodLevel(ll0);
            ioc.AddLodLevel(ll1);
            ioc.AddLodLevel(ll2);
            ioc.AddLodLevel(ll3);

            for(int i = 0; i < 10000; i++)
            {
                ioc.AddInstance(
                    new Mesh3dInstance(
                        new TransformationManager(
                            new Vector3((float)(rand.NextDouble() * 2.0 - 1.0) * 100,
                                        (float)(rand.NextDouble()) * 2.0f + 1.0f,
                                        (float)(rand.NextDouble() * 2.0 - 1.0) * 100),
                            Quaternion.FromAxisAngle(Vector3.UnitY, (float)(rand.NextDouble() + 0.25f) * 3.0f), 
                            1.0f
                        ), 
                    "")
                );
            }

            ioc.UpdateMatrix();
            scene.Add(ioc);
            
            var sph1 = Object3dInfo.LoadFromObjSingle(Media.Get("sph1.obj"));
            
        }
    }
}