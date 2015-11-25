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
            var color2 = new GenericMaterial(Color.Green);
            Mesh3d water3 = Mesh3d.Create(groundInfo, color2);
            water3.GetInstance(0).Scale(30);
            scene.Add(water3);


            Random rand = new Random();
            /*
            List<VegetationPart> vegs = new List<VegetationPart>();
            // grass
            vegs.Add(new VegetationPart("vurt_reachflowers2.dds", "6billboardsgrass.obj", 0.8f, 1.4f, 3801));
            vegs.Add(new VegetationPart("vurt_AssortedPlants.dds", "6billboardsgrass.obj", 0.8f, 1.2f, 3510));
            vegs.Add(new VegetationPart("vurt_brownplants.dds", "6billboardsgrass.obj", 0.8f, 2.2f, 3610));
            vegs.Add(new VegetationPart("snowgrass01.dds", "6billboardsgrass.obj", 0.8f, 1.2f, 3710));

            //tress
            //vegs.Add(new VegetationPart("vurt_ heather.dds", "4billboardstrees.obj", 15.0f, 6.6f, 89));
           // vegs.Add(new VegetationPart("vurt_aspenleaves.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));
           // vegs.Add(new VegetationPart("vurt_FFleavesG2.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));
           // vegs.Add(new VegetationPart("vurt_PineSnowy.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));

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
            }*/
            
            
            Object3dInfo[] tree_0 = Object3dInfo.LoadFromObj(Media.Get("tree_1_lod0.obj"));
            Object3dInfo[] tree_1 = Object3dInfo.LoadFromObj(Media.Get("tree_1_lod1.obj"));
            Object3dInfo[] tree_2 = Object3dInfo.LoadFromObj(Media.Get("tree_1_lod2.obj"));
            Object3dInfo[] tree_3 = Object3dInfo.LoadFromObj(Media.Get("tree_1_lod3.obj"));

            var tree_root_0 = tree_0[1];
            var tree_root_1 = tree_1[0];
            var tree_root_2 = tree_2[1];
            var tree_root_3 = tree_3[1];

            var tree_leaves_0 = tree_0[0];
            var tree_leaves_1 = tree_1[1];
            var tree_leaves_2 = tree_2[0];
            var tree_leaves_3 = tree_3[0];

            GenericMaterial rootmaterial = GenericMaterial.FromMedia("env1.dds", "env1n.dds");
            GenericMaterial leavesmaterial = GenericMaterial.FromMedia("tree_leaves.png");

            LodLevel rootl0 = new LodLevel(tree_root_0, rootmaterial, 0, 30);
            LodLevel rootl1 = new LodLevel(tree_root_1, rootmaterial, 30, 60);
            LodLevel rootl2 = new LodLevel(tree_root_2, rootmaterial, 60, 180);
            LodLevel rootl3 = new LodLevel(tree_root_3, rootmaterial, 180, 300);

            LodLevel leavesl0 = new LodLevel(tree_leaves_0, leavesmaterial, 0, 30);
            LodLevel leavesl1 = new LodLevel(tree_leaves_1, leavesmaterial, 30, 60);
            LodLevel leavesl2 = new LodLevel(tree_leaves_2, leavesmaterial, 60, 180);
            LodLevel leavesl3 = new LodLevel(tree_leaves_3, leavesmaterial, 180, 3100);

            Mesh3d roots = Mesh3d.Empty;
            roots.AddLodLevel(rootl0);
            roots.AddLodLevel(rootl1);
            roots.AddLodLevel(rootl2);
            roots.AddLodLevel(rootl3);
            Mesh3d leaves = Mesh3d.Empty;
            leaves.AddLodLevel(leavesl0);
            leaves.AddLodLevel(leavesl1);
            leaves.AddLodLevel(leavesl2);
            leaves.AddLodLevel(leavesl3);

            for(int i = 0; i < 29110; i++)
            {
                var inst = new Mesh3dInstance(
                        new TransformationManager(
                            new Vector3((float)(rand.NextDouble() * 2.0 - 1.0) * 400,
                                        0,
                                        (float)(rand.NextDouble() * 2.0 - 1.0) * 400),
                            Quaternion.FromAxisAngle(Vector3.UnitY, (float)(rand.NextDouble() + 0.25f) * 3.0f),
                            1.0f
                        ),
                    "");
                roots.AddInstance(inst);
                leaves.AddInstance(inst);
            }

            roots.UpdateMatrix();
            leaves.UpdateMatrix();
            scene.Add(roots);
            scene.Add(leaves);
            int level = 0;
            GLThread.CreateTimer(() =>
            {
                roots.UpdateMatrix();
                leaves.UpdateMatrix();
                level++;
                if(level > 3)
                    level = 0;
            }, 1150).Start();

            //  var sph1 = Object3dInfo.LoadFromObjSingle(Media.Get("sph1.obj"));

           // var terrain = Mesh3d.Create(Object3dInfo.LoadFromObjSingle(Media.Get("terrain1.obj")), GenericMaterial.FromColor(Color.Aquamarine));
           // terrain.GetInstance(0).Scale(1000.0f);
          //  scene.Add(terrain);
        
          //  var ferrari = new GameScene("ferrari.scene");
          //  ferrari.Load();
          //  ferrari.Meshes.ForEach((a) => scene.Add(a));
        }
    }
}