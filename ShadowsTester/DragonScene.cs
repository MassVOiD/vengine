using System;
using System.Collections.Generic;
using OpenTK;
using VEngine;
using System.Linq;
using VEngine.Generators;
using BulletSharp;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace ShadowsTester
{
    public class DragonScene
    {
        private class VegetationPart
        {
            public int Count;
            public float Scale, ScaleVariation;
            public string Texture, Model;

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
            var scene = Game.World.Scene;
            var whiteboxInfo = Object3dManager.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = Mesh3d.Create(new Object3dInfo(whiteboxInfo.Vertices), new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
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
            Object3dInfo groundInfo = new Object3dInfo(Object3dGenerator.CreateTerrain(new Vector2(-1112, -1112), new Vector2(1112, 1112), new Vector2(1, 1), Vector3.UnitY, 15, (x, y) => 0).Vertices);
            var color2 = GenericMaterial.FromColor(Color.Green);
            color2.Roughness = 1.0f;
            color2.Metalness = 0.0f;
            var w5 = Mesh3d.Create(groundInfo, GenericMaterial.FromColor(Color.Green));
            scene.Add(w5);
            // Mesh3d water4 = Mesh3d.Create(groundInfo, new GenericMaterial(Color.White));
            //   water4.GetInstance(0).Scale(1);
            // scene.Add(water4);

            //Game.World.Physics.Gravity = new Vector3(0, -1, 0);

            Random rand = new Random();

            /*
            var barrelinfo = Object3dInfo.LoadFromObjSingle(Media.Get("barrel.obj"));
            var barrelshape = Physics.CreateConvexCollisionShape(barrelinfo);
            var barrels = Mesh3d.Create(barrelinfo, GenericMaterial.FromColor(Color.White));
            barrels.AutoRecalculateMatrixForOver16Instances = true;
            barrels.GetLodLevel(0).Material.Metalness = 0.0f;
            barrels.GetLodLevel(0).Material.Roughness = 0.0f;
            barrels.ClearInstances();

            Game.OnKeyUp += (ox, oe) =>
            {
                if(oe.Key == OpenTK.Input.Key.Keypad0)
                {
                    var instance = barrels.AddInstance(new TransformationManager(Camera.MainDisplayCamera.GetPosition()));
                    var phys = Game.World.Physics.CreateBody(0.7f, instance, barrelshape);
                    phys.Enable();
                    phys.Body.LinearVelocity += Camera.MainDisplayCamera.GetDirection()*6;
                }
            };
            Game.World.Scene.Add(barrels);

            ComputeShader grs = new ComputeShader("GrassHeightWriter.compute.glsl");
            Game.Invoke(() =>
            {
                grfbo.Use();
                grfbo.RevertToDefault();
            });
            Game.OnBeforeDraw += (od, oe) =>
            {
                grs.Use();
                grs.SetUniform("BarrelsCount", barrels.GetInstances().Count);
                grs.SetUniform("GrassSurfaceMin", new Vector3(-12, 0, -12));
                grs.SetUniform("GrassSurfaceMax", new Vector3(12, 0, 12));
                var i = barrels.GetInstances().Select<Mesh3dInstance, Vector3>((a) => a.GetPosition()).ToArray();
                grs.SetUniformArray("Barrels", i);
                GL.BindImageTexture(0, grfbo.TexColor, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32f);
                GL.DispatchCompute(1024, 1024, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            };
            */

            
            List<VegetationPart> vegs = new List<VegetationPart>();
            // grass
            vegs.Add(new VegetationPart("vurt_reachflowers2.dds", "6billboardsgrass.obj", 0.8f, 1.4f, 35381));
            vegs.Add(new VegetationPart("vurt_AssortedPlants.dds", "6billboardsgrass.obj", 0.8f, 1.2f, 35301));
            vegs.Add(new VegetationPart("vurt_brownplants.dds", "6billboardsgrass.obj", 0.8f, 2.2f, 315381));
            vegs.Add(new VegetationPart("snowgrass01.dds", "6billboardsgrass.obj", 0.8f, 1.2f, 315381));

            //tress
            //vegs.Add(new VegetationPart("vurt_ heather.dds", "4billboardstrees.obj", 15.0f, 6.6f, 89));
           // vegs.Add(new VegetationPart("vurt_aspenleaves.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));
           // vegs.Add(new VegetationPart("vurt_FFleavesG2.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));
           // vegs.Add(new VegetationPart("vurt_PineSnowy.dds", "4billboardstrees.obj", 17.0f, 6.6f, 89));

            float vegarea = 1000;
            foreach(var v in vegs)
            {
                Mesh3d ioc = Mesh3d.Create(new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get(v.Model)).Vertices), new GenericMaterial(Color.White));
                ioc.ClearInstances();
                ioc.GetLodLevel(0).Material.SetTextureFromMedia(v.Texture);
                for(int i = 0; i < v.Count; i++)
                    ioc.AddInstance(new Mesh3dInstance(new TransformationManager(new Vector3((float)(rand.NextDouble() * 2.0 - 1.0) * vegarea,
                        0.0f,
                        (float)(rand.NextDouble() * 2.0 - 1.0) * vegarea), Quaternion.FromAxisAngle(Vector3.UnitY, (float)(rand.NextDouble() + 0.25f) * 3.0f), (float)(rand.NextDouble()) * v.ScaleVariation + v.Scale), v.Model));
                ioc.UpdateMatrix();
                scene.Add(ioc);
            }

            Object3dManager[] tree_0 = Object3dManager.LoadFromObj(Media.Get("tree_1_lod0.obj"));
            Object3dManager[] tree_1 = Object3dManager.LoadFromObj(Media.Get("tree_1_lod1.obj"));
            Object3dManager[] tree_2 = Object3dManager.LoadFromObj(Media.Get("tree_1_lod2.obj"));
            Object3dManager[] tree_3 = Object3dManager.LoadFromObj(Media.Get("tree_1_lod3.obj"));

            var tree_root_0 = new Object3dInfo(tree_0[1].Vertices);
            var tree_root_1 = new Object3dInfo(tree_1[0].Vertices);
            var tree_root_2 = new Object3dInfo(tree_2[1].Vertices);
            var tree_root_3 = new Object3dInfo(tree_3[1].Vertices);

            var tree_leaves_0 = new Object3dInfo(tree_0[0].Vertices);
            var tree_leaves_1 = new Object3dInfo(tree_1[1].Vertices);
            var tree_leaves_2 = new Object3dInfo(tree_2[0].Vertices);
            var tree_leaves_3 = new Object3dInfo(tree_3[0].Vertices);

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
            for(int i = 0; i < 151; i++)
            {
                var inst = new Mesh3dInstance(
                        new TransformationManager(
                            new Vector3((float)(rand.NextDouble() * 2.0 - 1.0) * 1000,
                                        0,
                                        (float)(rand.NextDouble() * 2.0 - 1.0) * 1000),
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
            Game.CreateTimer(() =>
            {
                roots.UpdateMatrix();
                leaves.UpdateMatrix();
                level++;
                if(level > 3)
                    level = 0;
            }, 1150).Start();

            //  var sph1 = Object3dInfo.LoadFromObjSingle(Media.Get("sph1.obj"));

            //  var terrain = Mesh3d.Create(Object3dInfo.LoadFromObjSingle(Media.Get("pisa.obj")), GenericMaterial.FromColor(Color.LightSlateGray));
            //  terrain.GetInstance(0).Scale(1.0f);
            //  scene.Add(terrain);

            /*     var ferrari = new GameScene("ferrari.scene");
                 ferrari.Load();
                 ferrari.Meshes.ForEach((a) =>
             {
                 a.GetLodLevel(0).Info3d.DrawGridInsteadOfTriangles = true;
                 scene.Add(a);
             });*/

            //      var dmk = Object3dInfo.LoadSceneFromObj(Media.Get("conf.obj"), Media.Get("conf.mtl"));

            //      dmk.ForEach((a) =>
            //    {
            //  a.GetLodLevel(0).Info3d.DrawGridInsteadOfTriangles = true;
            //       scene.Add(a);
            //   });

            PostProcessing pp = new PostProcessing(128, 128);
            CubeMapFramebuffer cubens = new CubeMapFramebuffer(128, 128);
            var tex = new CubeMapTexture(cubens.TexColor);
            cubens.SetPosition(new Vector3(0, 1, 0));
            Game.OnKeyPress += (ao, e) =>
            {
                if(e.KeyChar == 'z')
                    cubens.SetPosition(Camera.MainDisplayCamera.GetPosition());
                if(e.KeyChar == 'x')
                {
                    Game.Invoke(() =>
                    {
                        pp.RenderToCubeMapFramebuffer(cubens);
                        Game.DisplayAdapter.Pipeline.PostProcessor.CubeMap = tex;
                        tex.Handle = cubens.TexColor;
                    });
                }
            };
        }
    }
}