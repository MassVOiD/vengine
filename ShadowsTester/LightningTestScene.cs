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

        public LightningTestScene()
        {
            var scene = Game.World.Scene;

            Game.Invoke(() =>
            {/*
                var ground = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.Identity, Vector3.Zero, new Vector3(0.1f, 0.4f, 1));
                var green = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90)), Vector3.Zero, new Vector3(0.2f, 1, 0.3f));
                var red = CreateWall(new Vector2(-100), new Vector2(100), Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(90)), Vector3.Zero, new Vector3(1, 0.2f, 0.2f));
                green.GetInstance(0).Translate(0, 0, -15);
                red.GetInstance(0).Translate(15, 0, 0);

                var lucy = CreateDiffuseModelFromRaw("lucy.vbo.raw", new Vector3(1));

                scene.Add(ground);
                scene.Add(green);
                scene.Add(red);

                scene.Add(lucy);*/
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

                scene.Add(cubes);
                */

                var terrain = new Object3dInfo( Object3dGenerator.CreateTerrain(new Vector2(-10000, -10000), new Vector2(10000, 10000), new Vector2(1, -1), Vector3.UnitY, 512, (x,y) => 0).Vertices );
                var terrainMaterial = new GenericMaterial();
                terrainMaterial.Type = GenericMaterial.MaterialType.TessellatedTerrain;
                terrainMaterial.TessellationMultiplier = 1.0f;
                terrainMaterial.ParallaxHeightMultiplier = 200.0f;
                terrainMaterial.SetBumpTexture("ee.png");
                var terrainMesh = Mesh3d.Create(terrain, terrainMaterial);
                scene.Add(terrainMesh);

                GenericMaterial.UpdateMaterialsBuffer();
            });
        }
    }
}