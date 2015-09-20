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
        public static
            ConsoleManager cmg = new ConsoleManager();

        class Config
        {   
            public static string MediaPath = "media";
            public static int Width = 1920;
            public static int Height = 1010;
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
                GLThread.SetCurrentThreadCores(2);

                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height);

                GLThread.GraphicsSettings.UseDeferred = true;
                GLThread.GraphicsSettings.UseRSM = true;
                GLThread.GraphicsSettings.UseFog = false;
                GLThread.GraphicsSettings.UseBloom = false;
                GLThread.GraphicsSettings.UseLightPoints = false;

                window.Run(60);
            });
            World.Root = new World();

            var freeCamera = Commons.SetUpFreeCamera();
            Commons.AddControllableLight();


            //new PlanetScene().Create();
            //   new SculptScene().Create();
            //   new SponzaScene().Create();
            new OldCityScene();
          // new PathTraceTest().Create();
          //  new NatureScene().Create();
          //  new IndirectTestScene().Create();
        //  new DragonScene().Create();
           // new ManyCubesScene().Create();
          //  new ComputeBallsScene().Create();
           //new CarScene().Create();

           // new FortressScene().Create();

           // new HallScene().Create();
           // new RoadScene().Create();
           // new HomeScene().Create();


            World.Root.SortByDepthMasking();

            System.Threading.Thread.Sleep(1000);

            Commons.SetUpInputBehaviours();
            
            //World.Root.SortByObject3d();

            GLThread.Invoke(() => window.StartPhysicsThread());
            renderThread.Wait();
        }
    }
}