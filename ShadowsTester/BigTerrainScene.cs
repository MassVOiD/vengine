using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using VEngine;
using VEngine.FileFormats;
using VEngine.Generators;

namespace ShadowsTester
{
    public class BigTerrainScene
    {
        struct TerrainState
        {
            public byte[] State;
            public TerrainState(byte level)
            {
                State = new byte[64 * 64];
                for(int i = 0; i < 64 * 64; i++)
                    State[i] = level;
            }
            public TerrainState(TerrainState state)
            {
                State = new byte[64 * 64];
                for(int i = 0; i < 64 * 64; i++)
                    State[i] = state.State[i];
            }
        }

        public BigTerrainScene()
        {
            var whiteboxInfo = Object3dManager.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = Mesh3d.Create(new Object3dInfo(whiteboxInfo.Vertices), new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.GetInstance(0).Scale(3000);
            whitebox.GetInstance(0).Translate(0, -1500, 0);
            Game.World.Scene.Add(whitebox);
            


        }
    }
}