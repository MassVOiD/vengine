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
            var whiteboxInfo = Object3dManager.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = Mesh3d.Create(new Object3dInfo(whiteboxInfo.Vertices), new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.GetInstance(0).Scale(3000);
            whitebox.GetInstance(0).Translate(0, -1500, 0);
            Game.World.Scene.Add(whitebox);

            Object3dInfo groundInfo = new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get("someterrain.obj")).Vertices);
            var w5 = Mesh3d.Create(groundInfo, GenericMaterial.FromColor(Color.Green));
            w5.GetLodLevel(0).Material.Roughness = 0.9f;
            w5.GetLodLevel(0).Material.Metalness = 0.0f;
            w5.GetInstance(0).Translate(0, -2, 0);
            w5.GetInstance(0).Scale(100);
            Game.World.Scene.Add(w5);

            var lucy2 = Mesh3d.Create(new Object3dInfo(Object3dManager.LoadFromRaw(Media.Get("lucy.vbo.raw")).Vertices), GenericMaterial.FromColor(Color.White));
            lucy2.GetInstance(0).Scale(0.2f);
            lucy2.GetInstance(0).Translate(0,0,10);
            lucy2.GetLodLevel(0).Material.Roughness = 0.2f;
            lucy2.GetLodLevel(0).Material.Metalness = 0.01f;
            Game.World.Scene.Add(lucy2);

            var testScene = new Scene();
            //var lucyobj = Object3dInfo.LoadFromRaw(Media.Get("lucy.vbo.raw"), Media.Get("lucy.indices.raw"));
            var lucyobj = new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get("sph1.obj")).Vertices);
            var lucy = Mesh3d.Create(lucyobj, GenericMaterial.FromColor(Color.White));
            lucy.GetInstance(0).Scale(0.3f);
            testScene.Add(lucy);
            Game.World.Scene.Add(testScene);
            Commons.PickedMesh = lucy;
            Commons.Picked = lucy.GetInstance(0);

            PostProcessing pp = new PostProcessing(512, 512);
            CubeMapFramebuffer cubens = new CubeMapFramebuffer(512, 512);
            var tex = new CubeMapTexture(cubens.TexColor);
            Game.Invoke(() => Game.DisplayAdapter.Pipeline.PostProcessor.CubeMap = tex);
            cubens.SetPosition(new Vector3(0, 1, 0));
            Game.OnKeyPress += (o, e) =>
            {
                if(e.KeyChar == 'z')
                    cubens.SetPosition(Camera.MainDisplayCamera.GetPosition());
            };
            Game.OnBeforeDraw += (od, dsd) =>
            {
                if((lucy.GetInstance(0).GetPosition() - cubens.GetPosition()).Length > 0.01f)
                {
                    cubens.SetPosition(lucy.GetInstance(0).GetPosition());
                    pp.RenderToCubeMapFramebuffer(cubens);
                    Game.DisplayAdapter.Pipeline.PostProcessor.CubeMap = tex;
                    tex.Handle = cubens.TexColor;
                }
            };
        }
    }
}