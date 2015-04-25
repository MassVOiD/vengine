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
using VEngine.GameConsole;

namespace SimpleCarGame
{
    class Program
    {
        public static CarCamera FreeCam;
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

            World.Root = new World();

            float aspect = Config.Height > Config.Width ? Config.Height / Config.Width : Config.Width / Config.Height;

            var freeCamera = new CarCamera((float)Config.Width / (float)Config.Height, MathHelper.PiOver3);
            FreeCam = freeCamera;
            Random rand = new Random();


            new CarScene().Create();
            GLThread.Invoke(() =>
            {
                window.PostProcessor.UseFog = false;
                window.PostProcessor.UseSimpleGI = false;
                window.PostProcessor.UseBilinearGI = false;
                window.PostProcessor.UseBloom = false;
            });

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Middle)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.PhysicalBody.LinearVelocity += (Camera.Current.GetDirection() * 120.0f);

                    }
                }
                if(e.Button == OpenTK.Input.MouseButton.Left)
                {
                    var sphere = new BulletSharp.SphereShape(0.3f);
                    Mesh3d m = new Mesh3d(Object3dInfo.Empty, new SolidColorMaterial(Color.White));
                    m.Transformation.SetPosition(freeCamera.Cam.Transformation.GetPosition() + freeCamera.Cam.Transformation.GetOrientation().ToDirection() * 2.0f);
                    m.SetMass(11.0f);
                    m.SetCollisionShape(sphere);
                    var sl = new SimplePointLight(m.Transformation.GetPosition(),
                        Color.FromArgb(
                        rand.Next(33, 66), rand.Next(33, 66), rand.Next(33, 66)));
                    LightPool.Add(sl);
                    World.Root.Add(m);
                    MeshLinker.Link(m, sl, Vector3.Zero, Quaternion.Identity);
                    m.PhysicalBody.LinearVelocity = freeCamera.Cam.Transformation.GetOrientation().ToDirection() * 10.0f;

                }
            };

            Object3dInfo skydomeInfo = Object3dInfo.LoadFromObjSingle(Media.Get("skydome.obj"));
            var skydomeMaterial = new SolidColorMaterial(Color.White);
            var skydome = new Mesh3d(skydomeInfo, skydomeMaterial);
            skydome.Transformation.Scale(1000);
            skydome.IgnoreLighting = true;
            World.Root.Add(skydome);

            GameConsole console = new GameConsole(25);


            World.Root.SimulationSpeed = 3.0f;

            World.Root.SortByObject3d();

            GLThread.Invoke(() => window.StartPhysicsThread());
            renderThread.Wait();

        }
    }
}
