using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;
using VEngine.Generators;

namespace ShadowsTester
{
    public class VictorianHouseScene
    {
        dynamic max(params dynamic[] obj)
        {
            dynamic m = obj[0];
            foreach(dynamic a in obj)
                if(a > m)
                    m = a;
            return m;
        }
        dynamic min(params dynamic[] obj)
        {
            dynamic m = obj[0];
            foreach(dynamic a in obj)
                if(a < m)
                    m = a;
            return m;
        }
        Mesh3d CreateDiffuseModelFromRaw(string obj, Vector3 color)
        {
            var terrain3dManager = Object3dManager.LoadFromRaw(Media.Get(obj));
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = new Vector3(0);
            terrainMaterial.SpecularColor = new Vector3(1);
            terrainMaterial.Roughness = 0.5f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            return terrainMesh;
        }

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
        public VictorianHouseScene()
        {
            var scene = new GameScene("victorianhouse.scene");
            var ground = CreateWall(new Vector2(-1000), new Vector2(1000), Quaternion.Identity, Vector3.Zero, new Vector3(0.1f, 0.4f, 1));
            //Game.Invoke(() => DynamicCubeMapController.Create());
            Game.Invoke(() => Game.World.Scene.Add(ground));
            scene.OnObjectFinish += (ox, e) =>
            {
                if(e is Mesh3d)
                {
                    Game.Invoke(() =>
                    {
                        var o = e as Mesh3d;
                        Game.World.Scene.Add(o);
                        o.GetLodLevel(0).DisableFaceCulling = true;

                        if(o.GetLodLevel(0).Info3d.Manager.Name.ToLower().Contains("glass"))
                        {
                            o.GetLodLevel(0).Material.UseForwardRenderer = true;
                            o.GetLodLevel(0).Material.Alpha = 0.2f;
                        }

                        o.GetLodLevel(0).Info3d.Manager = null;
                        GenericMaterial.UpdateMaterialsBuffer();
                    });
                }
            };
            scene.Load();
        }
    }
}