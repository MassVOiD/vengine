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

            Object3dInfo groundInfo = new Object3dInfo(Object3dGenerator.CreateTerrain(new Vector2(-112, -112), new Vector2(112, 112), new Vector2(1, 1), Vector3.UnitY, 32, (x, y) => 0).Vertices);
            var color2 = GenericMaterial.FromColor(Color.Green);
            color2.Roughness = 1.0f;
            color2.Metalness = 0.0f;
            color2.Type = GenericMaterial.MaterialType.Grass;
            var w5 = Mesh3d.Create(groundInfo, color2);
            scene.Add(w5);
            Mesh3d water4 = Mesh3d.Create(groundInfo, new GenericMaterial(Color.White));
            water4.GetInstance(0).Scale(1);
            scene.Add(water4);
            
        }
    }
}