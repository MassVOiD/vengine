using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using VEngine;

namespace ShadowsTester
{
    internal class Program
    {
        private class Config
        {
            public static int Width = 1920;
            public static int Height = 1079;
            public static string MediaPath = "media";
        }

        public static
            ConsoleManager cmg = new ConsoleManager();

        public static FreeCamera FreeCam;

        [STAThread]
        private static void Main(string[] args)
        {
            VEngineWindowAdapter window = null;
            //var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));

            Media.SearchPath = Config.MediaPath;
            Media.LoadFileMap();

            GLThread.Resolution = new Size(Config.Width, Config.Height);

            GLThread.SetCurrentThreadCores(1);


            var renderThread = Task.Factory.StartNew(() =>
            {
                GLThread.SetCurrentThreadCores(2);

                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height, GameWindowFlags.Default);

                GLThread.GraphicsSettings.UseDeferred = true;
                GLThread.GraphicsSettings.UseRSM = false;
                GLThread.GraphicsSettings.UseVDAO = true;
                GLThread.GraphicsSettings.UseFog = false;
                GLThread.GraphicsSettings.UseBloom = false;
                GLThread.GraphicsSettings.UseLightPoints = false;

                window.CursorVisible = false;

                window.Run(60);
            });
            World.Root = new World();

            var freeCamera = Commons.SetUpFreeCamera();
            Commons.AddControllableLight();
            Commons.SetUpInputBehaviours();
            GLThread.Invoke(() => window.StartPhysicsThread());

            //new PlanetScene().Create();
            //   new SculptScene().Create();
            //   new SponzaScene().Create();
           // new OldCityScene();
            // new PathTraceTest().Create();
            //  new NatureScene().Create();
            //  new IndirectTestScene().Create();
           new DragonScene();
          //   new ManyCubesScene().Create();
            //  new ComputeBallsScene().Create();
            //new CarScene().Create();

            // new FortressScene().Create();

            // new HallScene().Create(); new RoadScene().Create(); new HomeScene().Create();

            World.Root.SortByDepthMasking();


            //World.Root.SortByObject3d();

            System.Windows.Forms.Application.Run(new SettingsController());
            renderThread.Wait();
        }
    }
}