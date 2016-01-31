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


            Renderer pp = new Renderer(256, 256, 1);
            Game.DisplayAdapter.MainRenderer.CubeMaps.Clear();
            for(int i = 0; i < 23; i++)
            {
                CubeMapFramebuffer cubens = new CubeMapFramebuffer(48, 48);
                var tex = new CubeMapTexture(cubens.TexColor);
                if(i < 10)
                    cubens.SetPosition(new Vector3((i - 5) * 1.3f, 1, 0));
                else if(i < 20)
                    cubens.SetPosition(new Vector3((i - 15) * 1.3f, 5, 0));
                else
                    cubens.SetPosition(new Vector3((i - 20) * 5, 7, 0));
                lucy.AddInstance(new Mesh3dInstance(new TransformationManager(cubens.GetPosition(), Quaternion.Identity, 0.001f), "cubemap-marker-" + i.ToString()));
                Game.DisplayAdapter.MainRenderer.CubeMaps.Add(new Renderer.CubeMapInfo()
                {
                    FalloffScale = 4.0f,
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
                    for(int i = 0; i < 23; i++)
                    {
                        Game.DisplayAdapter.MainRenderer.CubeMaps[i].Position *= -1;
                    }
                }
                if(eargs.Key == OpenTK.Input.Key.Home)
                {
                    var cpos = Camera.MainDisplayCamera.GetPosition();
                    var displacements = new List<Vector3> {
                        Vector3.Zero,
                        new Vector3(-2, 0, 0),
                        new Vector3(-1, 0, 0),
                        new Vector3(1, 0, 0),
                        new Vector3(2, 0, 0),

                        new Vector3(0, 0, -2),
                        new Vector3(0, 0, -1),
                        new Vector3(0, 0, 1),
                        new Vector3(0, 0, 2),

                        new Vector3(-2, 0, -2),
                        new Vector3(-1, 0, -1),
                        new Vector3(1, 0, 1),
                        new Vector3(2, 0, 2),

                        new Vector3(2, 0, -2),
                        new Vector3(1, 0, -1),
                        new Vector3(-1, 0, 1),
                        new Vector3(-2, 0, 2),


                        new Vector3(1, 0, -2),
                        new Vector3(-1, 0, -2),

                        new Vector3(1, 0, -1),
                        new Vector3(-1, 0, -1),

                        new Vector3(1, 0, 1),
                        new Vector3(-1, 0, 1)

                     //   new Vector3(1, 0, 2),
                     //   new Vector3(-1, 0, 2),

                    };
                    for(int i = 0; i < 23; i++)
                    {
                        lucy.GetInstance(i).SetPosition(cpos + displacements[i]);
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
                    for(int iz = 0; iz < 23; iz++)
                    {
                        if((lucy.GetInstance(iz).GetPosition() - Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Position).Length > 0.01f)
                        {
                            Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Framebuffer.Clear();
                        }
                    }
                    for(int iz = 0; iz < 23; iz++)
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
                if(ix >= 23)
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
