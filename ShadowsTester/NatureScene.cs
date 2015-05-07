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
    public class NatureScene : Scene
    {
        public NatureScene()
        {
            /*
            var protagonist = Object3dInfo.LoadSceneFromObj(Media"protagonist.obj", "protagonist.mtl", 1.0f);
            foreach(var o in protagonist)
                Add(o);*/
            
            var hmap = System.IO.File.ReadAllBytes(Media.Get("pearlharbor.raw"));
            int count = 0;
            int scale = 1;
            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                int yy = (int)(((double)y / (750.0 / scale)) * 5425);
                int xx = (int)(((double)x / (750.0 / scale)) * 6804);
                int ix = (int)(yy * 6804  + xx) * 3;
                count++;
                if(count % 1000 == 0)
                    Console.WriteLine(count * 100 / hmap.Length);
                return ix < hmap.Length ? hmap[ix] * 0.5f : 0;
            };
            Object3dGenerator.UseCache = false;
            Object3dInfo terrainInfo = Object3dGenerator.CreateTerrain(new Vector2(-1830 / scale, -1600 / scale), new Vector2(1830 / scale, 1600 / scale), new Vector2(4096, 4096), Vector3.UnitY, 1024 / scale, terrainGen);
            hmap = null;
            GC.Collect();

            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-2048 / scale, -2048 / scale), new Vector2(2048 / scale, 2048 / scale), new Vector2(496, 496), Vector3.UnitY);

            var terrain = new Mesh3d(terrainInfo, new SolidColorMaterial(Color.Green));
            Add(terrain);

            var waterMat = new SolidColorMaterial(new Vector4(0.55f, 0.74f, 0.97f, 1.0f));
            waterMat.SetNormalMapFromMedia("waternormal.png");
            var water = new Mesh3d(waterInfo, waterMat);
            water.Transformation.Translate(0, 10, 0);
            //water.DisableDepthWrite = true;
            //Add(water);
            

            var daetest = Object3dInfo.LoadFromObjSingle(Media.Get("carreragt.obj"));
            daetest.CorrectFacesByNormals();
            var car = new Mesh3d(daetest, new SolidColorMaterial(Color.Red));
            Add(car);
            var rand = new Random();
            /*
            var scene1 = Object3dInfo.LoadSceneFromObj(Media.Get("tree1.obj"), Media.Get("tree1.mtl"), 12);
            List<Mesh3d> trees = new List<Mesh3d>();
            for(int i = 0; i < 50; i++)
            {
                foreach(var ob in scene1)
                {
                    var copy = new Mesh3d(ob.MainObjectInfo, ob.MainMaterial);
                    copy.Translate(rand.Next(-40, 40), 10, rand.Next(-40, 40));
                    trees.Add(copy);
                }
            }
            InstancedMesh3d.FromMesh3dList(trees).ForEach((a) =>
            {
                a.UpdateMatrix();
                Add(a);
            });*/
            /*var grasslod0 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod1.obj"));
            var grasslod1 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod1.obj"));
            var grasslod2 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod2.obj"));
            var grasslod3 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod3.obj"));
            //var grassBlock3dInfo = Object3dInfo.LoadFromRaw(Media.Get("grassblock2.vbo.raw"), Media.Get("grassblock2.indices.raw"));
            //grassBlock3dInfo.MakeDoubleFaced();
            var grasscolor = new SolidColorMaterial(Color.DarkGreen);
            var grassInstanced = new InstancedMesh3d(grasslod0, grasscolor);
            //grassInstanced.AddLodLevel(15, grasslod0, grasscolor);
            //grassInstanced.AddLodLevel(60, grasslod1, grasscolor);
            //grassInstanced.AddLodLevel(200, grasslod2, grasscolor);
            //grassInstanced.AddLodLevel(600, grasslod3, grasscolor);
           // GLThread.CreateTimer(() =>
            //{
            //    grassInstanced.RecalculateLod();
            //}, 100).Start();
            for(int x = -15; x < 15; x++)
            {
                for(int y = -15; y < 15; y++)
                {
                    grassInstanced.Transformations.Add(new TransformationManager(new Vector3(x, 0, y), Quaternion.FromAxisAngle(Vector3.UnitY, (float)rand.NextDouble() * MathHelper.Pi), new Vector3(1, 1, 1)));
                    grassInstanced.Instances++;
                }
            }
            grassInstanced.UpdateMatrix();
            Add(grassInstanced);*/
            /*
            var grassBlock3dInfo2 = Object3dInfo.LoadFromRaw(Media.Get("grassblock2.vbo.raw"), Media.Get("grassblock2.indices.raw"));
            //grassBlock3dInfo2.MakeDoubleFaced();
            var grassInstanced2 = new InstancedMesh3d(grassBlock3dInfo2, new SolidColorMaterial(Color.DarkGoldenrod));
            for(int x = -20; x < 20; x++)
            {
                for(int y = -20; y < 20; y++)
                {
                    grassInstanced2.Transformations.Add(new TransformationManager(new Vector3(x * 2, 0, y * 2), Quaternion.FromAxisAngle(Vector3.UnitY, (float)rand.NextDouble() * MathHelper.Pi), 2.3f));
                    grassInstanced2.Instances++;
                }
            }
            grassInstanced2.UpdateMatrix();
            Add(grassInstanced2);*/
            /*var scene = Object3dInfo.LoadSceneFromObj(Media.Get("cryteksponza.obj"), Media.Get("cryteksponza.mtl"), 0.03f);
            //var instances = InstancedMesh3d.FromMesh3dList(testroom);
            foreach(var ob in scene)
            {
                //ob.SetMass(0);
                // ob.SetCollisionShape(ob.ObjectInfo.GetAccurateCollisionShape());
                //ob.SpecularComponent = 0.1f;
                this.Add(ob);
            }*/
            
        }

    }
}
