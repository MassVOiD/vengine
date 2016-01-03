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
            public static int Width = 670;
            public static string MediaPath = "media";
            public static int Height = 960;
        }

        [STAThread]
        private static void Main(string[] args)
        {
            Game.Initialize(new Size(Config.Width, Config.Height), Config.MediaPath, GameWindowFlags.FixedWindow);

            var freeCamera = Commons.SetUpFreeCamera();
            System.Threading.Thread.Sleep(1100);
            Commons.AddControllableLight();
            Commons.SetUpInputBehaviours();

            new OldCityScene();
            //  new LightningTestScene();

            //new DragonScene();

            var samples = VEngine.PathTracing.JitterRandomSequenceGenerator.EvenlySampledHemisphere(16, 16);
            var os = "";
            foreach(var s in samples)
            {
                s.Normalize();
                os += string.Format("vec3({0}, {1}, {2}),\r\n", s.X.ToString(System.Globalization.CultureInfo.InvariantCulture), s.Y.ToString(System.Globalization.CultureInfo.InvariantCulture), s.Z.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            System.IO.File.WriteAllText("dupa.txt", os);

            System.Windows.Forms.Application.Run(new SettingsController());
            //renderThread.Wait();
        }
    }
}