using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using System.IO;
using VEngine;
using BulletSharp;
using VEngine.Generators;
using UI = VEngine.UI;
using System.Threading;

namespace ShadowsTester
{
    internal class Program
    {
        public static FreeCamera FreeCam;

        class Config
        {
            public static string MediaPath = "media";
            public static int Width = 1366;
            public static int Height = 768;
        }

        private static void Main(string[] args)
        {
            VEngineWindowAdapter window = null;
            //var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));

            Media.SearchPath = Config.MediaPath;
            GLThread.Resolution = new Size(Config.Width, Config.Height);

            GLThread.SetCurrentThreadCores(1);

            var renderThread = Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height);

                GLThread.GraphicsSettings.UseDeferred = true;
                GLThread.GraphicsSettings.UseFog = false;
                GLThread.GraphicsSettings.UseBloom = false;
                GLThread.GraphicsSettings.UseLightPoints = false;
                GLThread.GraphicsSettings.UseMSAA = false;
                GLThread.GraphicsSettings.UseSimpleGI = true;
                GLThread.GraphicsSettings.UseBilinearGI = false;

                window.Run(60);
                GLThread.SetCurrentThreadCores(2);
            });
            World.Root = new World();

            var freeCamera = Commons.SetUpFreeCamera();
            Commons.AddControllableLight();


            //new SculptScene().Create();
            //new SponzaScene().Create();
            new OldCityScene().Create();
            //new NatureScene().Create();
            //new IndirectTestScene().Create();
            //new DragonScene().Create();
            //new ManyCubesScene().Create();
            //new ComputeBallsScene().Create();

            //new HallScene().Create();
            //new RoadScene().Create();
            //new HomeScene().Create();


            World.Root.SortByDepthMasking();

            System.Threading.Thread.Sleep(1000);

            Commons.SetUpInputBehaviours();
            
            World.Root.SortByObject3d();

            GLThread.Invoke(() => window.StartPhysicsThread());
            renderThread.Wait();
        }
    }
}