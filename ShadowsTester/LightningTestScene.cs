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
            terrainMaterial.Roughness = 1.0f;
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
            terrainMaterial.Roughness = 0.1f;
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
                var green = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90)), Vector3.Zero, new Vector3(0.2f, 1, 0.3f));
                var red = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(90)), Vector3.Zero, new Vector3(1, 0.2f, 0.2f));
                green.GetInstance(0).Translate(0, 0, -15);
                red.GetInstance(0).Translate(15, 0, 0);

                var lucy = CreateDiffuseModelFromRaw("lucy.vbo.raw", new Vector3(1));

                scene.Add(ground);
                scene.Add(green);
                scene.Add(red);

                scene.Add(lucy);
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



                for(int i = 0; i < 100000; i++)
                {
                    var pos = new Vector3(rand(-1000, 1000), 0, rand(-1000, 1000));
                    float uniscale = rand(0.7f, 1.5f);
                    var scale = new Vector3(uniscale, rand(0.7f, 3.0f), uniscale);
                    trm.AddInstance(new TransformationManager(pos, scale));
                    tlm.AddInstance(new TransformationManager(pos, scale));
                }
                trm.UpdateMatrix();
                tlm.UpdateMatrix();*/

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
                var cityObj = Object3dManager.LoadFromObjSingle(Media.Get("somecity.obj")).AsObject3dInfo();
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
                GenericMaterial.UpdateMaterialsBuffer();
            });
        }
    }
}