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
        public LightningTestScene()
        {
            var scene = Game.World.Scene;

            var terrain3dManager = Object3dGenerator.CreateGround(new Vector2(-200), new Vector2(200), new Vector2(200), Vector3.UnitY);
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = Vector3.One;
            terrainMaterial.SpecularColor = Vector3.Zero;
            terrainMaterial.Roughness = 1.0f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            var terrainShape = new BulletSharp.StaticPlaneShape(Vector3.UnitY, terrainMesh.GetInstance(0).GetPosition().Y);
            var terrainBody = Game.World.Physics.CreateBody(0.0f, terrainMesh.GetInstance(0), terrainShape);
           // scene.Add(terrainMesh);
            terrainBody.Enable();

            var invisibleBoundingBoxShape = new BulletSharp.BoxShape(25, 1111, 25);
            var invisibleBoundingBoxBody = Game.World.Physics.CreateBody(0.0f, new TransformationManager(new Vector3(0, 400, 0)), invisibleBoundingBoxShape);
          //  invisibleBoundingBoxBody.Enable();
            
            var invisibleBoundingBoxBody2 = Game.World.Physics.CreateBody(0.0f, new TransformationManager(new Vector3(50, 0, 0)), invisibleBoundingBoxShape);
            invisibleBoundingBoxBody2.Enable();
            
            var invisibleBoundingBoxBody3 = Game.World.Physics.CreateBody(0.0f, new TransformationManager(new Vector3(-50, 0, 0)), invisibleBoundingBoxShape);
            invisibleBoundingBoxBody3.Enable();
            
            var invisibleBoundingBoxBody4 = Game.World.Physics.CreateBody(0.0f, new TransformationManager(new Vector3(0, 0, 50)), invisibleBoundingBoxShape);
            invisibleBoundingBoxBody4.Enable();
            
            var invisibleBoundingBoxBody5 = Game.World.Physics.CreateBody(0.0f, new TransformationManager(new Vector3(0, 0, -50)), invisibleBoundingBoxShape);
            invisibleBoundingBoxBody5.Enable();


            var box3dManager = Object3dManager.LoadFromObjSingle(Media.Get("icosphere.obj"));
            var box3dInfo = new Object3dInfo(box3dManager.Vertices);
            var boxMaterial = new GenericMaterial();
            boxMaterial.Roughness = 0.5f;
            boxMaterial.DiffuseColor = new Vector3(0.1f, 0.5f, 0.9f);
            boxMaterial.SpecularColor = new Vector3(0.5f);
            var boxMesh = Mesh3d.Create(box3dInfo, boxMaterial);
            boxMesh.AutoRecalculateMatrixForOver16Instances = true;
            boxMesh.ClearInstances();
            var boxShape = new BulletSharp.SphereShape(1.0f);
           // scene.Add(boxMesh);

            Game.OnUpdate += (ox, oe) =>
            {
                var state = OpenTK.Input.Keyboard.GetState();
                if(state.IsKeyDown(OpenTK.Input.Key.Keypad0))
                {
                    var instance = boxMesh.AddInstance(new TransformationManager(Camera.MainDisplayCamera.GetPosition()));
                    var phys = Game.World.Physics.CreateBody(0.7f, instance, boxShape);
                    phys.Enable();
                    phys.Body.LinearVelocity += Camera.MainDisplayCamera.GetDirection() * 36;
                }
                if(state.IsKeyDown(OpenTK.Input.Key.Keypad1))
                {
                    var instance = boxMesh.AddInstance(new TransformationManager(Camera.MainDisplayCamera.GetPosition()));
                    var phys = Game.World.Physics.CreateBody(0.7f, instance, boxShape);
                    phys.Enable();
                    phys.Body.LinearVelocity += Camera.MainDisplayCamera.GetDirection() * 1;
                }
            };
            /*Game.OnKeyUp += (ox, oe) =>
            {
                
            };*/




            var buildings3dManager = Object3dManager.LoadFromObjSingle(Media.Get("tesstest.obj"));
          //  buildings3dManager.TryToFixVertexWinding();
            var buildings3dInfo = new Object3dInfo(buildings3dManager.Vertices);
            var buildingsMaterial = new GenericMaterial();
            buildingsMaterial.Type = GenericMaterial.MaterialType.TessellatedTerrain;
            buildingsMaterial.DiffuseColor = Vector3.One;
            buildingsMaterial.SpecularColor = Vector3.One;
            buildingsMaterial.SetDiffuseTexture("cat.png");
            buildingsMaterial.Roughness = 1f;
            var buildingsMesh = Mesh3d.Create(buildings3dInfo, buildingsMaterial);
            scene.Add(buildingsMesh);
            
            DynamicCubeMapController.Create();

        }
    }
}