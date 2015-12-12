using System;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
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
            Game.Initialize(new Size(Config.Width, Config.Height), Config.MediaPath);

            var freeCamera = Commons.SetUpFreeCamera();
            Commons.AddControllableLight();
            Commons.SetUpInputBehaviours();

            new OldCityScene();

            //new DragonScene();

            System.Windows.Forms.Application.Run(new SettingsController());
            //renderThread.Wait();
        }
    }
}