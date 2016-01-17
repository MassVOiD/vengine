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
            
            
        }
    }
}