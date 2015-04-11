using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using System.IO;
using VDGTech;
using BulletSharp;
using VDGTech.Generators;
using UI = VDGTech.UI;
using System.Threading;

namespace SimpleCarGame
{
    class Program
    {
        static void Main(string[] args)
        {
            VEngineWindowAdapter window = null;
            var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));
            Media.SearchPath = Config.MediaPath;

            GLThread.SetCurrentThreadCores(1);

            var renderThread = Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height);
                window.Run(60);
                GLThread.SetCurrentThreadCores(2);
            });
            /*GLThread.Invoke(() =>
            {
                window.SetCustomPostProcessingMaterial(new PostProcessLoadingMaterial());
            });*/
            World.Root = new World();

            // System.Threading.Thread.Sleep(1000);

            float aspect = Config.Height > Config.Width ? Config.Height / Config.Width : Config.Width / Config.Height;

            Random rand = new Random();
            /*
            Matrix4 mirrorMatrix = Matrix4.CreateScale(1, -1, 1);

            GLThread.OnAfterDraw += (o, e) => {
                var oldm = Camera.Current.ViewMatrix;
                Camera.Current.ViewMatrix = Matrix4.Mult(mirrorMatrix, Camera.Current.ViewMatrix);
                window.DrawAll();
                Camera.Current.ViewMatrix = oldm;
            };*/

            /* Func<uint, uint, float> terrainGen = (x, y) =>
             {
                 float h =
                     (SimplexNoise.Noise.Generate((float)x, (float)y)) +
                     (SimplexNoise.Noise.Generate((float)x / 6, (float)y / 7) * 8) +
                     (SimplexNoise.Noise.Generate((float)x / 24, (float)y / 23) * 31) +
                     (SimplexNoise.Noise.Generate((float)x / 35, (float)y / 66) * 80) +
                     (SimplexNoise.Noise.Generate((float)x / 99, (float)y / 111) * 122);
                 float dist = ((new Vector2(150, 150) - new Vector2(x, y)).Length) / 150.0f;
                 return h * dist * 5.0f;
             };
             Object3dGenerator.UseCache = false;*/
            /*
            short[,] hmap = new short[6001, 6001];
            FileStream hmapfile = File.OpenRead(Media.Get("SAfull.hmap"));
            int xx = 0, yy = 0;
            byte[] buffer = new byte[2];
            while(hmapfile.CanRead)
            {
                hmapfile.Read(buffer, 0, 2);
                hmap[xx, yy] = BitConverter.ToInt16(buffer, 0);
                xx++;
                if(xx >= 6000)
                {
                    xx = 0;
                    yy++;
                    if(yy >= 6000)
                        break;
                }
            }
            hmapfile.Close();
            
            Func<uint, uint, float> waterGen = (x, y) =>
            {
                x = (uint)(x * (6000.0f / 1000.0f));
                y = (uint)(y * (6000.0f / 1000.0f));
                return Math.Abs((float)hmap[6000 - x, 6000 - y] / 100.0f);
            };

            Object3dGenerator.UseCache = false;
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-2500, -2500), new Vector2(2500, 2500), new Vector2(-1, 1), Vector3.UnitY, 512, waterGen);
            */
            //  var color = SingleTextureMaterial.FromMedia("gtamap.jpg");

            //bject3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-2500, -2500), new Vector2(2500, 2500), new Vector2(1000, 1000), Vector3.UnitY, 300, terrainGen);
            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY);


            var color = SingleTextureMaterial.FromMedia("177.jpg", "177_norm.JPG");
            //var color = new SolidColorMaterial(Color.Silver);
            //color.SetBumpMapFromMedia("177_norm.JPG");
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            //water.SetCollisionShape(waterInfo.GetAccurateCollisionShape());
            //water.SpecularSize = 15.0f;
            // water.DiffuseComponent = 0.5f;
            //water.DisableDepthWrite = true;
            World.Root.Add(water);
            //World.Root.PhysicalWorld.AddCollisionObject(water.CreateRigidBody());

            /*GLThread.Invoke(() =>
            {
                GLThread.CreateTimer(() =>
                {
                    window.PostProcessor.UpdateCameraBrightness(freeCamera.Cam);
                }, 100).Start();
            });*/


            ProjectionLight redConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 1024, 1024, MathHelper.PiOver2, 1.0f, 10000.0f);
            redConeLight.LightColor = new Vector4(1, 1, 1, 1);


            //redConeLight.SetProjection(Matrix4.CreateOrthographic(200, 200, -500, 500));
            LightPool.Add(redConeLight);
            
        }
    }
}
