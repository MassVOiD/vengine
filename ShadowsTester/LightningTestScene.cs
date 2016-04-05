using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;
using VEngine.Generators;

namespace ShadowsTester
{
    public class LightningTestScene
    {

        Mesh3d CreateWall(Vector2 start, Vector2 end, Quaternion rotation, Vector3 position, Vector3 color)
        {
            var terrain3dManager = Object3dGenerator.CreateGround(start, end, new Vector2(1), Vector3.UnitY);
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = color;
            terrainMaterial.SpecularColor = Vector3.Zero;
            terrainMaterial.Roughness = 0f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            terrainMesh.GetInstance(0).Rotate(rotation);
            return terrainMesh;
        }

        Mesh3d CreateDiffuseModelFromRaw(string obj, Vector3 color)
        {
            var terrain3dManager = Object3dManager.LoadFromRaw(Media.Get(obj));
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = color;
            terrainMaterial.SpecularColor = color;
            terrainMaterial.Roughness = 0.0f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            return terrainMesh;
        }
        Mesh3d CreateModel(Object3dInfo obj, Vector3 diffuse, Vector3 specular, float roughness)
        {
            var terrain3dInfo = obj;
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = diffuse;
            terrainMaterial.SpecularColor = specular;
            terrainMaterial.Roughness = roughness;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            return terrainMesh;
        }
        static Random rdz = new Random();
        static float rand(float min, float max)
        {
            return ((float)rdz.NextDouble()) * (max - min) + min;
        }

        public LightningTestScene()
        {
            var scene = Game.World.Scene;

            Game.Invoke(() =>
            {
                var ground = CreateWall(new Vector2(-1000), new Vector2(1000), Quaternion.Identity, Vector3.Zero, new Vector3(0.1f, 0.4f, 1));
                // scene.Add(ground);

                var obj = Object3dManager.LoadFromObjSingle(Media.Get("emily.obj"));
                obj.RecalulateNormals(Object3dManager.NormalRecalculationType.Smooth, 1);
                var t1 = CreateModel(obj.AsObject3dInfo(), new Vector3(0.8f), new Vector3(0.2f), 0.1f);

                t1.GetLodLevel(0).Material.SetDiffuseTexture("00_diffuse_unlit_unpainted.png");
                t1.GetLodLevel(0).Material.SetSpecularTexture("00_specular_unlit_unpainted.png");
                t1.GetLodLevel(0).Material.SetBumpTexture("00_displacement_misdfcro.png");
                
                scene.Add(t1);

                var obj2 = Object3dManager.LoadFromObjSingle(Media.Get("emilylashes.obj"));
                var t2 = CreateModel(obj2.AsObject3dInfo(), new Vector3(0.8f), new Vector3(0.2f), 0.1f);

                t2.GetLodLevel(0).Material.DiffuseColor = Vector3.Zero;
                t2.GetLodLevel(0).Material.SpecularColor = Vector3.Zero;

                scene.Add(t2);

               // var obj2 = Object3dManager.LoadFromObjSingle(Media.Get("emilyeyes.obj"));
              //  var t2 = CreateModel(obj2.AsObject3dInfo(), new Vector3(0.8f), new Vector3(0.2f), 0.1f);

              //  t2.GetLodLevel(0).Material.DiffuseColor = Vector3.Zero;
              //  t2.GetLodLevel(0).Material.SpecularColor = Vector3.Zero;

              //  scene.Add(t2);

                /*
                var m = CreateDiffuseModelFromObj("hipolysphere.obj", new Vector3(1));
                m.GetLodLevel(0).Material.SpecularColor = new Vector3(1.0f);
                m.GetLodLevel(0).Material.DiffuseColor= new Vector3(0.0f);
                m.GetLodLevel(0).Material.Roughness = 0.9f;
                scene.Add(m);*/

                /* var green = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90)), Vector3.Zero, new Vector3(0.2f, 1, 0.3f));
                 var red = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(90)), Vector3.Zero, new Vector3(1, 0.2f, 0.2f));
                 green.GetInstance(0).Translate(0, 0, -15);
                 red.GetInstance(0).Translate(15, 0, 0);

                 var lucy = CreateDiffuseModelFromRaw("lucy.vbo.raw", new Vector3(1));
                 //scene.Add(green);
                 //scene.Add(red);

                 Game.CascadeShadowMaps.SetDirection(Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(-25)));

                 scene.Add(lucy);

                 float[] billboardfloats = {
                     -1.0f, 0, 0, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                     1.0f, 0, 0, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                     -1.0f, 1.0f, 0, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                     1.0f, 1.0f, 0, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
                 };

                 var billboardObj = new Object3dManager(VertexInfo.FromFloatArray(billboardfloats)).AsObject3dInfo();
                 billboardObj.DrawMode = OpenTK.Graphics.OpenGL4.PrimitiveType.TriangleStrip;
                 string[] trees = new string[] { "vurt_PineSnowy.dds" };
                 for(int id = 0; id < 1; id++)
                 {
                     var billboardMaterial = new GenericMaterial();
                     billboardMaterial.UseForwardRenderer = true;
                     billboardMaterial.IsBillboard = true;
                     billboardMaterial.SetDiffuseTexture(trees[id]);
                     //billboardMaterial.SetNormalsTexture("alphatest_n.png");
                     billboardMaterial.Roughness = 0.8f;
                     billboardMaterial.InvertUVy = true;

                     var billboardMesh = Mesh3d.Create(billboardObj, billboardMaterial);
                     billboardMesh.ClearInstances();

                     for(int i = 0; i < 30000; i++)
                     {
                         var pos = new Vector3(rand(-1000, 1000), 0, rand(-1000, 1000));
                         float uniscale = rand(1.7f, 2.5f);
                         var scale = new Vector3(uniscale, rand(12.3f, 12.4f), uniscale);
                         billboardMesh.AddInstance(new TransformationManager(pos, scale));
                     }
                     billboardMesh.UpdateMatrix();

                     Game.CreateThread(() =>
                     {
                         while(true)
                         {
                             //billboardMesh.IterationSortInstancesByDistanceFrom(Camera.MainDisplayCamera.Transformation.Position, 50);
                             billboardMesh.FullSortInstancesByDistanceFrom(Camera.MainDisplayCamera.Transformation.Position);
                             billboardMesh.UpdateMatrix(false);
                         }
                     });



                     scene.Add(billboardMesh);
                 }
                 string[] vegs = new string[] { "fieldgrassobj01.dds", "vurt_brownplants.dds" };
                 for(int id = 0; id < 1; id++)
                 {
                     var billboardMaterial = new GenericMaterial();
                     billboardMaterial.UseForwardRenderer = true;
                     billboardMaterial.IsBillboard = true;
                     billboardMaterial.SetDiffuseTexture(vegs[id]);
                     //billboardMaterial.SetNormalsTexture("alphatest_n.png");
                     billboardMaterial.Roughness = 0.8f;
                     billboardMaterial.InvertUVy = true;
                     billboardMaterial.Blending = GenericMaterial.BlendingEnum.Alpha;

                     var billboardMesh = Mesh3d.Create(billboardObj, billboardMaterial);
                     billboardMesh.ClearInstances();

                     for(int i = 0; i < 3000000; i++)
                     {
                         var pos = new Vector3(rand(-1000, 1000), 0, rand(-1000, 1000));
                         float uniscale = rand(1.7f, 2.5f);
                         var scale = new Vector3(uniscale, rand(2.3f, 2.4f), uniscale);
                         billboardMesh.AddInstance(new TransformationManager(pos, scale));
                     }
                     billboardMesh.UpdateMatrix();

                     Game.CreateThread(() =>
                     {
                         while(true)
                         {
                             //billboardMesh.IterationSortInstancesByDistanceFrom(Camera.MainDisplayCamera.Transformation.Position, 50);
                             billboardMesh.FullSortInstancesByDistanceFrom(Camera.MainDisplayCamera.Transformation.Position);
                             billboardMesh.UpdateMatrix(false);
                         }
                     });



                     scene.Add(billboardMesh);
                 }*/

                /*
                var trootobj = new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get("tree2r.obj")).Vertices);
                var tleavobj = new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get("tree2l.obj")).Vertices);
                var rootmaterial = new GenericMaterial()
                {
                    Roughness = 0.9f,
                    DiffuseColor = Vector3.One,
                    SpecularColor = Vector3.Zero
                };
                var leavmaterial = new GenericMaterial()
                {
                    Roughness = 0.9f,
                    DiffuseColor = Vector3.One,
                    SpecularColor = Vector3.Zero
                };
              //  leavmaterial.SetAlphaTexture("Branches0018_1_S_mask.png");
                leavmaterial.SetDiffuseTexture("Hedge 00 seamless.jpg");

                var trm = Mesh3d.Create(trootobj, rootmaterial);
                var tlm = Mesh3d.Create(tleavobj, leavmaterial);
                scene.Add(trm);
                scene.Add(tlm);

                trm.ClearInstances();
                tlm.ClearInstances();



                for(int i = 0; i < 100; i++)
                {
                    var pos = new Vector3(rand(-100, 100), 0, rand(-100, 100));
                    float uniscale = rand(0.7f, 1.5f);
                    var scale = new Vector3(uniscale, rand(0.7f, 3.0f), uniscale);
                    trm.AddInstance(new TransformationManager(pos, scale));
                    tlm.AddInstance(new TransformationManager(pos, scale));
                }
                trm.UpdateMatrix();
                tlm.UpdateMatrix();
                */
                /*
                var cubeMaterial = new GenericMaterial(new Vector3(1, 0, 0));
                var cubeObj3d = new Object3dInfo(Object3dGenerator.CreateCube(new Vector3(1), new Vector2(1)).Vertices);
                //var cubeObj3d = new Object3dInfo(Object3dGenerator.CreateGround(new Vector2(-1), new Vector2(1), new Vector2(1), Vector3.UnitY).Vertices);
                
                var cubes = Mesh3d.Create(cubeObj3d, cubeMaterial);
                cubes.ClearInstances();

                for(int x = 0; x < 100; x++)
                    for(int y = 0; y < 100; y++)
                        for(int z = 0; z < 100; z++)
                        {
                            cubes.AddInstance(new TransformationManager(new Vector3(x, y, z) * 4));
                        }
                cubes.UpdateMatrix();

                scene.Add(cubes);*/

                /*
                var cityMgr = Object3dManager.LoadFromObjSingle(Media.Get("somecity.obj"));
                cityMgr.TryToFixVertexWinding();
                cityMgr.RecalulateNormals(Object3dManager.NormalRecalculationType.Flat);
                var cityObj = cityMgr.AsObject3dInfo();
                var cityMat = new GenericMaterial()
                {
                    DiffuseColor = new Vector3(1, 0.89f, 0.97f),
                    Roughness = 0.2f
                };
                var cityMesh = Mesh3d.Create(cityObj, cityMat);
                cityMesh.GetInstance(0).Translate(0, 0.1f, 0);
                scene.Add(cityMesh);*/
                //
                //  var citimul = Object3dManager.LoadSceneFromObj("somecity.obj", "somecity.mtl");
                //  foreach(var m in citimul)
                //      scene.Add(m);
                /*
                var terrain = new Object3dInfo( Object3dGenerator.CreateTerrain(new Vector2(-10000, -10000), new Vector2(10000, 10000), new Vector2(1, -1), Vector3.UnitY, 512, (x,y) => 0).Vertices );
                var terrainMaterial = new GenericMaterial();
                terrainMaterial.Type = GenericMaterial.MaterialType.TessellatedTerrain;
                terrainMaterial.TessellationMultiplier = 1.0f;
                terrainMaterial.ParallaxHeightMultiplier = 200.0f;
                terrainMaterial.SetBumpTexture("ee.png");
                var terrainMesh = Mesh3d.Create(terrain, terrainMaterial);
                scene.Add(terrainMesh);*/

                // DynamicCubeMapController.Create();
            });
        }
    }
}