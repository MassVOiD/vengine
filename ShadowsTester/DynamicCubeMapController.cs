using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VEngine;

namespace ShadowsTester
{
    class DynamicCubeMapController
    {
        public static void Create()
        {

            var lucyobj = new Object3dInfo(Object3dManager.LoadFromObjSingle(Media.Get("sph1.obj")).Vertices);

            var bbmaterial = new GenericMaterial();

            var lucy = Mesh3d.Create(lucyobj, bbmaterial);
            lucy.ClearInstances();
            lucy.AutoRecalculateMatrixForOver16Instances = true;
            Game.World.Scene.Add(lucy);
            Commons.PickedMesh = lucy;
            Commons.Picked = null;

            int cnt = 16, din = 64;
            Renderer pp = new Renderer(din, din, 1);
            Game.DisplayAdapter.MainRenderer.CubeMaps.Clear();
            for(int i = 0; i < cnt; i++)
            {
                CubeMapFramebuffer cubens = new CubeMapFramebuffer(din, din);
                var tex = new CubeMapTexture(cubens.TexColor);

                if(i < 8)
                    cubens.SetPosition(new Vector3((i - 4) * 2.8f, 1.5f, 0));
                else if(i < 16)
                    cubens.SetPosition(new Vector3((i - 12) * 1.8f, 4.3f, 0));
                else if(i < 24)
                    cubens.SetPosition(new Vector3((i - 20) * 2.8f, 1.5f, -3.5f));
                else if(i < 32)
                    cubens.SetPosition(new Vector3((i - 28) * 2.8f, 1.5f, 3.5f));
                else if(i < 40)
                    cubens.SetPosition(new Vector3((i - 36) * 2.8f, 4.5f, -3.5f));
                else if(i < 48)
                    cubens.SetPosition(new Vector3((i - 44) * 2.8f, 4.5f, 3.5f));
                else if(i < 56)
                    cubens.SetPosition(new Vector3((i - 52) * 2.8f, 4.5f, 0));
                else if(i < 64)
                    cubens.SetPosition(new Vector3((i - 60) * 1.8f, 6.3f, 0));


                lucy.AddInstance(new Mesh3dInstance(new TransformationManager(cubens.GetPosition(), Quaternion.Identity, 0.02f), "cubemap-marker-" + i.ToString()));
                Game.DisplayAdapter.MainRenderer.CubeMaps.Add(new Renderer.CubeMapInfo()
                {
                    FalloffScale = (i < 24) ? 3.0f : 1.0f,
                    Framebuffer = cubens,
                    Position = cubens.GetPosition()
                });
            }
            int index = 0;
            bool livemode = false;
            Game.OnKeyUp += (xa, eargs) =>
            {
                if(eargs.Key == OpenTK.Input.Key.PageUp)
                    index++;
                if(eargs.Key == OpenTK.Input.Key.PageDown)
                    index--;
                if(index > 9)
                    index = 0;
                if(index < 0)
                    index = 9;
                if(Commons.Picked == null)
                    index = 0;
                Commons.Picked = lucy.GetInstance(index);
                TitleOutput.Message = "Picked cubemap ID " + index.ToString();
                if(eargs.Key == OpenTK.Input.Key.End)
                {
                    livemode = !livemode;
                }
                if(eargs.Key == OpenTK.Input.Key.Insert)
                {
                    for(int i = 0; i < cnt; i++)
                    {
                        Game.DisplayAdapter.MainRenderer.CubeMaps[i].Position *= -1;
                    }
                }
                if(eargs.Key == OpenTK.Input.Key.Home)
                {
                    var cpos = Camera.MainDisplayCamera.GetPosition();

                    float angle = 0;
                    for(int i = 0; i < cnt; i++)
                    {
                        float prc = (float)i / (float)cnt;
                        angle += 2.39996322f;
                        var disp = new Vector3(prc * 15.0f * (float)Math.Sin(angle), 1.0f - prc * 3.0f, prc * 15.0f * (float)Math.Cos(angle));
                        lucy.GetInstance(i).SetPosition(cpos + disp);
                    }
                }
                if(eargs.Key == OpenTK.Input.Key.Up)
                    Game.DisplayAdapter.MainRenderer.CubeMaps[index].FalloffScale += 0.1f;
                if(eargs.Key == OpenTK.Input.Key.Down)
                {
                    Game.DisplayAdapter.MainRenderer.CubeMaps[index].FalloffScale -= 0.1f;
                    if(Game.DisplayAdapter.MainRenderer.CubeMaps[index].FalloffScale < 0.02)
                        Game.DisplayAdapter.MainRenderer.CubeMaps[index].FalloffScale = 0.02f;
                }
            };
            int ix = 0;
            Game.OnBeforeDraw += (od, dsd) =>
            {
                //  for(int i = 0; i < 23; i++)
                // {
                //  if((lucy.GetInstance(i).GetPosition() - Game.DisplayAdapter.MainRenderer.CubeMaps[i].Position).Length > 0.01f)
                // {
                if(!livemode)
                {
                    for(int iz = 0; iz < cnt; iz++)
                    {
                        if((lucy.GetInstance(iz).GetPosition() - Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Position).Length > 0.01f)
                        {
                            Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Framebuffer.Clear();
                        }
                    }
                    for(int iz = 0; iz < cnt; iz++)
                    {
                        if((lucy.GetInstance(iz).GetPosition() - Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Position).Length > 0.01f)
                        {
                            Game.World.CurrentlyRenderedCubeMap = iz;
                            pp.CubeMaps = Game.DisplayAdapter.MainRenderer.CubeMaps;
                            Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Position = lucy.GetInstance(iz).GetPosition();
                            Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Framebuffer.SetPosition(lucy.GetInstance(iz).GetPosition());
                            pp.RenderToCubeMapFramebuffer(Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Framebuffer);
                            Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Framebuffer.GenerateMipMaps();
                            Game.World.CurrentlyRenderedCubeMap = -1;
                        }
                    }

                    return;
                }
                int i = ix++;
                if(ix >= cnt)
                    ix = 0;
                Game.World.CurrentlyRenderedCubeMap = i;
                pp.CubeMaps = Game.DisplayAdapter.MainRenderer.CubeMaps;
                Game.DisplayAdapter.MainRenderer.CubeMaps[i].Position = lucy.GetInstance(i).GetPosition();
                Game.DisplayAdapter.MainRenderer.CubeMaps[i].Framebuffer.SetPosition(lucy.GetInstance(i).GetPosition());
                pp.RenderToCubeMapFramebuffer(Game.DisplayAdapter.MainRenderer.CubeMaps[i].Framebuffer);
                Game.DisplayAdapter.MainRenderer.CubeMaps[i].Framebuffer.GenerateMipMaps();
                Game.World.CurrentlyRenderedCubeMap = -1;
                //  }
                //  }
            };
        }
    }
}
