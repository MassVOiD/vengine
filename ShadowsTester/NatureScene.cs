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
            var sun = new Sun(new Vector3(0.1f, -1, 0).ToQuaternion(Vector3.UnitY), new Vector4(1, 0.97f, 0.92f, 120), 100, 70, 10, 4, 1);
            GLThread.OnUpdate += (o, e) =>
            {
                var kb = OpenTK.Input.Keyboard.GetState();
                if(kb.IsKeyDown(OpenTK.Input.Key.U))
                {
                    var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), -0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.J))
                {
                    var quat = Quaternion.FromAxisAngle(sun.Orientation.GetTangent(MathExtensions.TangentDirection.Left), 0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.H))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, -0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.K))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f);
                    sun.Orientation = Quaternion.Multiply(sun.Orientation, quat);
                }
            };
            int scale = 2;
            /*
            var protagonist = Object3dInfo.LoadSceneFromObj(Media"protagonist.obj", "protagonist.mtl", 1.0f);
            foreach(var o in protagonist)
                Add(o);*/
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(300);
            whitebox.Translate(0, -2, 0);
            Add(whitebox);

            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-1500, -1000), new Vector2(1500, 1000), new Vector2(1096, 1096), Vector3.UnitY, 121, (x, y) => 0);


            var waterMat = new GenericMaterial(new Vector4(0.55f, 0.74f, 0.97f, 1.0f));
            waterMat.SetNormalMapFromMedia("151_norm.JPG");
            waterMat.Type = GenericMaterial.MaterialType.Water;
            var water = new Mesh3d(waterInfo, waterMat);
            water.Transformation.Translate(0, 15, 0);
            //water.DisableDepthWrite = true;
            //water.ReflectionStrength = 1;
            water.MainMaterial.TesselationMultiplier = 0.1f;
            Add(water);
            /*
            var dragon3dInfo = Object3dInfo.LoadFromRaw(Media.Get("lucyhires.vbo.raw"), Media.Get("lucyhires.indices.raw"));
            dragon3dInfo.ScaleUV(0.1f);
            var dragon = new Mesh3d(dragon3dInfo, SingleTextureMaterial.FromMedia("180.jpg", "180_norm.jpg"));
            Add(dragon);
            */
           // var daetest = Object3dInfo.LoadFromObjSingle(Media.Get("carreragt.obj"));
          //  daetest.CorrectFacesByNormals();
           // var car = new Mesh3d(daetest, new SolidColorMaterial(Color.Red));
           // Add(car);
           /* var rand = new Random();
            
            var scene1 = Object3dInfo.LoadSceneFromObj(Media.Get("tree1.obj"), Media.Get("tree1.mtl"), 12);
            List<Mesh3d> trees = new List<Mesh3d>();
            for(int i = 0; i < 44; i++)
            {
                foreach(var ob in scene1)
                {
                    var copy = new Mesh3d(ob.MainObjectInfo, ob.MainMaterial);
                    copy.Translate(rand.Next(-40, 40), 10, rand.Next(-40, 40));
                    copy.Scale((float)rand.NextDouble() + 1.0f);
                    trees.Add(copy);
                }
            }
            InstancedMesh3d.FromMesh3dList(trees).ForEach((a) =>
            {
                a.UpdateMatrix();
                Add(a);
            });
            var grasslod0 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod1.obj"));
            var grasslod1 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod1.obj"));
            var grasslod2 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod2.obj"));
            var grasslod3 = Object3dInfo.LoadFromObjSingle(Media.Get("grasslod3.obj"));
            //var grassBlock3dInfo = Object3dInfo.LoadFromRaw(Media.Get("grassblock2.vbo.raw"), Media.Get("grassblock2.indices.raw"));
            //grassBlock3dInfo.MakeDoubleFaced();
            var grasscolor = new GenericMaterial(Color.DarkGreen);
            var grassInstanced = new InstancedMesh3d(grasslod3, grasscolor);
            //grassInstanced.AddLodLevel(15, grasslod0, grasscolor);
            //grassInstanced.AddLodLevel(60, grasslod1, grasscolor);
            //grassInstanced.AddLodLevel(200, grasslod2, grasscolor);
            //grassInstanced.AddLodLevel(600, grasslod3, grasscolor);
           // GLThread.CreateTimer(() =>
            //{
            //    grassInstanced.RecalculateLod();
            //}, 100).Start();
            /*for(int x = -500; x < 500; x++)
            {
                for(int y = -500; y < 500; y++)
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
