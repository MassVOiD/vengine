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
            bbmaterial.Roughness = 0;
            bbmaterial.DiffuseColor = Vector3.Zero;
            bbmaterial.SpecularColor = Vector3.One;

            var lucy = Mesh3d.Create(lucyobj, bbmaterial);
            lucy.ClearInstances();
            lucy.AutoRecalculateMatrixForOver16Instances = true;
            Game.World.Scene.Add(lucy);
            Commons.PickedMesh = lucy;
            Commons.Picked = null;

            int cnt = 0, din = 256;
            Renderer pp = new Renderer(din, din, 1);
            pp.GraphicsSettings.UseDeferred = true;
            pp.GraphicsSettings.UseRSM = false;
            pp.GraphicsSettings.UseVDAO = true;
            pp.GraphicsSettings.UseFog = false;
            pp.GraphicsSettings.UseBloom = false;
            pp.GraphicsSettings.UseCubeMapGI = false;
            Game.DisplayAdapter.MainRenderer.CubeMaps.Clear();
            
            for(float x = -10; x < 13; x += 4.0f)
            {
                for(float y = 0.2f; y < 15.3f; y += 6.0f)
                {
                    for(float z = -3; z < 3.5; z += 3.0f)
                    {
                        CubeMapFramebuffer cubens = new CubeMapFramebuffer(din, din);
                        var tex = new CubeMapTexture(cubens.TexColor);

                        cubens.SetPosition(x*0.98f - 0.35f, y, z - 0.25f);

                        lucy.AddInstance(new Mesh3dInstance(new TransformationManager(cubens.GetPosition(), Quaternion.Identity, 0.1f), "cubemap-marker-" + cnt.ToString()));
                        Game.DisplayAdapter.MainRenderer.CubeMaps.Add(new Renderer.CubeMapInfo()
                        {
                            FalloffScale = 1.0f,
                            Framebuffer = cubens,
                            Position = cubens.GetPosition()
                        });
                        cnt++;
                    }
                }
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
                            //Game.DisplayAdapter.MainRenderer.CubeMaps[iz].Framebuffer.Clear();
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
