using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;

namespace ShadowsTester
{
    public class OldCityScene
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
            terrain3dManager.ScaleUV(20);
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = new Vector3(0.7f);
            terrainMaterial.SpecularColor = new Vector3(0.7f);
            terrainMaterial.Roughness = 0.5f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            return terrainMesh;
        }
        Mesh3d CreateDiffuseModelFromObj(string obj, Vector3 color)
        {
            var terrain3dManager = Object3dManager.LoadFromObjSingle(Media.Get(obj));
            terrain3dManager.ScaleUV(20);
            var terrain3dInfo = new Object3dInfo(terrain3dManager.Vertices);
            var terrainMaterial = new GenericMaterial();
            terrainMaterial.DiffuseColor = new Vector3(0.7f);
            terrainMaterial.SpecularColor = new Vector3(0.7f);
            terrainMaterial.Roughness = 0.5f;
            var terrainMesh = Mesh3d.Create(terrain3dInfo, terrainMaterial);
            return terrainMesh;
        }
        public OldCityScene()
        {
            var objectLucy = Object3dManager.LoadFromObjSingle(Media.Get("cerb.obj"));
            var meshLucy = Mesh3d.Create(objectLucy.AsObject3dInfo(), new GenericMaterial(Color.Red));
            Game.World.Scene.Add(meshLucy);
        }
    }
}