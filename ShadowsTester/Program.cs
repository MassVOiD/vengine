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
        private class Config
        {
            public static int Width = 1920;
            public static int Height = 1040;
            public static string MediaPath = "media";
        }
        

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

                window = new VEngineWindowAdapter("VENGINE Initializing", Config.Width, Config.Height, GameWindowFlags.Default);
                window.Title = "VENGINE@" + GL.GetString(StringName.Vendor) + " " + GL.GetString(StringName.Renderer);

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
            
            new OldCityScene();
          // new DragonScene();

            System.Windows.Forms.Application.Run(new SettingsController());
            renderThread.Wait();
        }
    }
}