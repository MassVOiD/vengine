using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            public static int Width = 1280;
            public static string MediaPath = "media";
            public static int Height = 768;
        }

        [STAThread]
        private static void Main(string[] args)
        {
            Game.Initialize(new Size(Config.Width, Config.Height), 8, Config.MediaPath, GameWindowFlags.FixedWindow);

            var freeCamera = Commons.SetUpFreeCamera();
            //System.Threading.Thread.Sleep(1100);
            Commons.AddControllableLight();
            Commons.SetUpInputBehaviours();

            new OldCityScene();
            //new LightningTestScene();

            //new DragonScene();
            
            System.Threading.Thread.CurrentThread.Join();
        }
    }
}