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

        private static void Main(string[] args)
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
            var freeCamera = new FreeCamera((float)Config.Width / (float)Config.Height, MathHelper.PiOver3);
            FreeCam = freeCamera;

            GLThread.Invoke(() =>
            {
                window.Resize += (o, e) =>
                {
                    float aast = window.Height > window.Width ? window.Height + 1 / window.Width + 1 : window.Width + 1 / window.Height + 1;
                    freeCamera = new FreeCamera(aast, MathHelper.PiOver3);
                    FreeCam = freeCamera;
                    GLThread.Resolution.X = window.Width;
                    GLThread.Resolution.Y = window.Height;
                };
            });

            Random rand = new Random();
           
            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-200, -200), new Vector2(200, 200), new Vector2(100, 100), Vector3.UnitY);


            var color = SingleTextureMaterial.FromMedia("151.jpg", "151_norm.JPG");
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
             water.DiffuseComponent = 0.2f;
           // World.Root.Add(water);

             ProjectionLight redConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 2048, 2048, MathHelper.PiOver3, 1.0f, 10000.0f);
            redConeLight.LightColor = new Vector4(1, 1, 1, 200);

            LightPool.Add(redConeLight);
            
            GLThread.OnUpdate += (o, e) =>
            {
                var kb = OpenTK.Input.Keyboard.GetState();
                if(kb.IsKeyDown(OpenTK.Input.Key.Left))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitX / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Right))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitX / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Up))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitZ / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Down))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitZ / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.PageUp))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos + Vector3.UnitY / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.PageDown))
                {
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos - Vector3.UnitY / 12.0f);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.U))
                {
                    var quat = Quaternion.FromAxisAngle(redConeLight.camera.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left), -0.01f);
                    redConeLight.camera.Transformation.Rotate(quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.J))
                {
                    var quat = Quaternion.FromAxisAngle(redConeLight.camera.Transformation.GetOrientation().GetTangent(MathExtensions.TangentDirection.Left), 0.01f);
                    redConeLight.camera.Transformation.Rotate(quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.H))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, -0.01f);
                    redConeLight.camera.Transformation.Rotate(quat);
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.K))
                {
                    var quat = Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f);
                    redConeLight.camera.Transformation.Rotate(quat);
                }
            };

            //new SculptScene().Create();
            new SponzaScene().Create();
            //new OldCityScene().Create();
            //new NatureScene().Create();
            //new IndirectTestScene().Create();
            //new DragonScene().Create();
            //new ManyCubesScene().Create();
            //new CarScene().Create();

            //new HallScene().Create();
            //new RoadScene().Create();
            //new HomeScene().Create();

            World.Root.SortByDepthMasking();

            System.Threading.Thread.Sleep(500);
            window.PostProcessor.UseFog = false;
            window.PostProcessor.UseBloom = false;
            window.PostProcessor.UseLightPoints = false;
            window.PostProcessor.UseMSAA = false;
            window.PostProcessor.UseSimpleGI = false;
            window.PostProcessor.UseBilinearGI = false;

            var sphere3dInfo = Object3dInfo.LoadFromObjSingle(Media.Get("lightsphere.obj"));
            sphere3dInfo.Normalize();
            
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
                    Mesh3d m = new Mesh3d(sphere3dInfo, new SolidColorMaterial(new Vector4(1,1,1,0.1f)));
                    m.SetScale(0.3f);
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

            MeshLinker.LinkInfo pickedLink = null;
            bool inPickingMode = false;
            GLThread.OnMouseDown += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh != water && mesh.GetCollisionShape() != null && !inPickingMode)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        pickedLink = MeshLinker.Link(freeCamera.Cam, mesh, new Vector3(0, 0, -(freeCamera.Cam.Transformation.GetPosition() - mesh.Transformation.GetPosition()).Length),
                            Quaternion.Identity);
                        pickedLink.UpdateRotation = false;
                        inPickingMode = true;
                    }
                }
            };
            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    MeshLinker.Unlink(pickedLink);
                    pickedLink = null;
                    inPickingMode = false;
                }
            };

            GLThread.OnMouseWheel += (o, e) =>
            {
                if(e.Delta != 0 && inPickingMode)
                {
                    pickedLink.Offset.Z -= e.Delta * 3.0f;
                    if(pickedLink.Offset.Z > -5)
                        pickedLink.Offset.Z = -5;
                }
            };

            /*System.Timers.Timer lensFocusTimer = new System.Timers.Timer();
            lensFocusTimer.Interval = 300;
            lensFocusTimer.Elapsed += (o, e) =>
            {
                window.PostProcessor.UpdateCameraFocus(freeCamera.Cam);
                Console.WriteLine(Camera.MainDisplayCamera.LensBlurAmount);
            };
            lensFocusTimer.Start();*/

            
            Object3dInfo skydomeInfo = Object3dInfo.LoadFromObjSingle(Media.Get("skydome.obj"));
            var skydomeMaterial = SingleTextureMaterial.FromMedia("sky_povray.jpg");
            var skydome = new Mesh3d(skydomeInfo, skydomeMaterial);
            skydome.Transformation.Scale(1000);
            skydome.IgnoreLighting = true;
            World.Root.Add(skydome);
            
            GLThread.OnMouseWheel += (o, e) =>
            {
                if(!inPickingMode)
                    Camera.Current.LensBlurAmount -= e.Delta / 20.0f;
            };

            World.Root.SimulationSpeed = 1.0f;

            GLThread.OnKeyDown += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.T)
                {
                    World.Root.SimulationSpeed = 0.4f;
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    World.Root.SimulationSpeed = 0.10f;
                }

            };
            GLThread.OnKeyUp += (o, e) =>
            {
                if(e.Key == OpenTK.Input.Key.Tab)
                {
                    window.IsCursorVisible = !window.IsCursorVisible;
                }
                if(e.Key == OpenTK.Input.Key.Pause)
                {
                    ShaderProgram.RecompileAll();
                }
                if(e.Key == OpenTK.Input.Key.T)
                {
                    World.Root.SimulationSpeed = 1.0f;
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    World.Root.SimulationSpeed = 1.0f;
                }
                if(e.Key == OpenTK.Input.Key.Number1)
                {
                    //redConeLight.SetPosition(freeCamera.Cam.Transformation.GetPosition(), freeCamera.Cam.Transformation.GetPosition() + freeCamera.Cam.Transformation.GetOrientation().ToDirection());
                    redConeLight.GetTransformationManager().SetPosition(freeCamera.Cam.Transformation.GetPosition());
                    redConeLight.GetTransformationManager().SetOrientation(freeCamera.Cam.Transformation.GetOrientation());
                }
                if(e.Key == OpenTK.Input.Key.Number2)
                {
                    freeCamera.Cam.LookAt(new Vector3(0));
                }
                if(e.Key == OpenTK.Input.Key.Number3)
                {
                    Interpolator.Interpolate<Vector3>(redConeLight.GetTransformationManager().Position, redConeLight.GetTransformationManager().Position.R, freeCamera.Cam.GetPosition(), 8.0f, Interpolator.Easing.EaseInOut);
                }
                if(e.Key == OpenTK.Input.Key.Number0)
                    window.PostProcessor.UseBilinearGI = !window.PostProcessor.UseBilinearGI;
                if(e.Key == OpenTK.Input.Key.Number9)
                    window.PostProcessor.UseBloom = !window.PostProcessor.UseBloom;
                if(e.Key == OpenTK.Input.Key.Number8)
                    window.PostProcessor.UseDeferred = !window.PostProcessor.UseDeferred;
                if(e.Key == OpenTK.Input.Key.Number7)
                    window.PostProcessor.UseDepth = !window.PostProcessor.UseDepth;
                if(e.Key == OpenTK.Input.Key.Number6)
                    window.PostProcessor.UseFog = !window.PostProcessor.UseFog;
                if(e.Key == OpenTK.Input.Key.Number5)
                    window.PostProcessor.UseLightPoints = !window.PostProcessor.UseLightPoints;
                if(e.Key == OpenTK.Input.Key.Number4)
                    window.PostProcessor.UseSimpleGI = !window.PostProcessor.UseSimpleGI;
            };

            World.Root.SortByObject3d();

            GLThread.Invoke(() => window.StartPhysicsThread());
            //GLThread.Invoke(() => window.SetDefaultPostProcessingMaterial());
            renderThread.Wait();
        }
    }
}