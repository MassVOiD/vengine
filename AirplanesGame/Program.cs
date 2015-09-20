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
using VEngine.FileFormats;

namespace AirplanesGame
{
    internal class Program
    {
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
            Media.LoadFileMap();
            GLThread.Resolution = new Size(Config.Width, Config.Height);

            GLThread.SetCurrentThreadCores(1);

            var renderThread = Task.Factory.StartNew(() =>
            {
                GLThread.SetCurrentThreadCores(2);
                window = new VEngineWindowAdapter("Airplanes", Config.Width, Config.Height);

                GLThread.GraphicsSettings.UseDeferred = true;
                GLThread.GraphicsSettings.UseRSM = false;
                GLThread.GraphicsSettings.UseFog = false;
                GLThread.GraphicsSettings.UseBloom = false;
                GLThread.GraphicsSettings.UseLightPoints = false;
                GLThread.GraphicsSettings.UseVDAO = true;
                GLThread.GraphicsSettings.UseDepth = false;

                Commons.AddControllableLight();
                window.Run(60);
            });

            World.Root = new World();
            Scene nn = new Scene();
            var camera = new Camera(new Vector3(0, 0, 20), new Vector3(0, 2, 0), Config.Width / Config.Height, MathHelper.DegreesToRadians(100), 0.1f, 10000.0f);
            camera.Brightness = 6.0f;
            Camera.MainDisplayCamera = camera;
            Camera.Current = camera;
            var airplan = new Airplane(nn);
            World.Root.Add(airplan.Body);
            var whiteboxInfo = Object3dInfo.LoadFromObjSingle(Media.Get("whiteroom.obj"));
            var whitebox = new Mesh3d(whiteboxInfo, new GenericMaterial(new Vector4(1000, 1000, 1000, 1000)));
            whitebox.Scale(3000);
            whitebox.Translate(0, -1500, 0);
            World.Root.Add(whitebox);
            var scene = new GameScene("home1.scene");
            scene.Load();
            var cnt = scene.Meshes.Count;
            for(var i = 0; i < cnt; i++)
            {
                var o = scene.Meshes[i];
                //var b = o.Transformation.GetPosition();
                //b = new opentk.Vector3(b.X*0.5, b.Y*0.5, b.Z*0.5);
                o.Transformation.Position *= new Vector3(5);
                o.Transformation.Scale(5);
                o.SetMass(0);
                o.MainMaterial.Roughness = (o.MainMaterial.Roughness + 0.5f) / 2.0f;
                o.SetCollisionShape(o.MainObjectInfo.GetAccurateCollisionShape(5));
                World.Root.Add(o);
            }

           /* var lucyobj = VEngine.Object3dInfo.LoadFromRaw(VEngine.Media.Get("lucy.vbo.raw"), VEngine.Media.Get("lucy.indices.raw"));
            lucyobj.ScaleUV(8.0f);
            var lucy = new VEngine.Mesh3d(lucyobj, VEngine.GenericMaterial.FromMedia("168.JPG"));
            lucy.Transformation.Scale(0.4f);
            lucy.SetCollisionShape(lucy.MainObjectInfo.GetAccurateCollisionShape(0.4f));
            World.Root.Add(lucy);*/

            Commons.SetUpInputBehaviours();

            World.Root.SortByDepthMasking();

            System.Threading.Thread.Sleep(1000);

            window.StartPhysicsThread();
            renderThread.Wait();
        }
    }
}