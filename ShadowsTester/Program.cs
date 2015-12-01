using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VEngine;

namespace ShadowsTester
{
    internal class Program
    {
        public static FreeCamera FreeCam;

        private class Config
        {
            public static int Height = 1020;
            public static string MediaPath = "media";
            public static int Width = 1920;
        }

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

                window = new VEngineWindowAdapter("VENGINE Initializing", Config.Width, Config.Height, GameWindowFlags.Default);
                window.Title = "VEngine App";

                GLThread.GraphicsSettings.UseDeferred = true;
                GLThread.GraphicsSettings.UseRSM = false;
                GLThread.GraphicsSettings.UseVDAO = true;
                GLThread.GraphicsSettings.UseFog = false;
                GLThread.GraphicsSettings.UseBloom = false;
                GLThread.GraphicsSettings.UseLightPoints = true;

                window.CursorVisible = false;

                window.Run(60);
            });
            World.Root = new World();

            var freeCamera = Commons.SetUpFreeCamera();
            Commons.AddControllableLight();
            Commons.SetUpInputBehaviours();

            new OldCityScene();
            
           // new DragonScene();

            System.Windows.Forms.Application.Run(new SettingsController());
            renderThread.Wait();
        }
    }
}