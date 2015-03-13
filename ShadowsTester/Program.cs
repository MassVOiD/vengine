using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using System.IO;
using VDGTech;
using BulletSharp;
using VDGTech.Generators;
using UI = VDGTech.UI;
using System.Threading;

namespace ShadowsTester
{
    internal class Program
    {
        public static FreeCamera FreeCam;

        private static void Main(string[] args)
        {
            //System.Threading.Thread.Sleep(1000);
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
            GLThread.Invoke(() =>
            {
                window.SetCustomPostProcessingMaterial(new PostProcessLoadingMaterial());
            });
            World.Root = new World();

            // System.Threading.Thread.Sleep(1000);

            float aspect = Config.Height > Config.Width ? Config.Height / Config.Width : Config.Width / Config.Height;
            var freeCamera = new FreeCamera((float)Config.Width / (float)Config.Height, MathHelper.PiOver3);
            FreeCam = freeCamera;

            Object3dInfo infocube = Object3dInfo.LoadFromObjSingle(Media.Get("cube.obj"));

            GLThread.Invoke(() =>
            {
                window.Resize += (o, e) =>
                {
                    float aast = window.Height > window.Width ? window.Height / window.Width : window.Width / window.Height;
                    freeCamera = new FreeCamera(aast, MathHelper.PiOver3);
                    FreeCam = freeCamera;
                    GLThread.Resolution.X = window.Width;
                    GLThread.Resolution.Y = window.Height;
                };
            });

            Random rand = new Random();
            /*
            Matrix4 mirrorMatrix = Matrix4.CreateScale(1, -1, 1);

            GLThread.OnAfterDraw += (o, e) => {
                var oldm = Camera.Current.ViewMatrix;
                Camera.Current.ViewMatrix = Matrix4.Mult(mirrorMatrix, Camera.Current.ViewMatrix);
                window.DrawAll();
                Camera.Current.ViewMatrix = oldm;
            };*/

            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                float h =
                    (SimplexNoise.Noise.Generate((float)x, (float)y)) +
                    (SimplexNoise.Noise.Generate((float)x / 6, (float)y / 7) * 8) +
                    (SimplexNoise.Noise.Generate((float)x / 24, (float)y / 23) * 31) +
                    (SimplexNoise.Noise.Generate((float)x / 35, (float)y / 66) * 80) +
                    (SimplexNoise.Noise.Generate((float)x / 99, (float)y / 111) * 122);
                return h;
            };
            /*
            short[,] hmap = new short[6001, 6001];
            FileStream hmapfile = File.OpenRead(Media.Get("SAfull.hmap"));
            int xx = 0, yy = 0;
            byte[] buffer = new byte[2];
            while(hmapfile.CanRead)
            {
                hmapfile.Read(buffer, 0, 2);
                hmap[xx, yy] = BitConverter.ToInt16(buffer, 0);
                xx++;
                if(xx >= 6000)
                {
                    xx = 0;
                    yy++;
                    if(yy >= 6000)
                        break;
                }
            }
            hmapfile.Close();
            
            Func<uint, uint, float> waterGen = (x, y) =>
            {
                x = (uint)(x * (6000.0f / 1000.0f));
                y = (uint)(y * (6000.0f / 1000.0f));
                return Math.Abs((float)hmap[6000 - x, 6000 - y] / 100.0f);
            };

            Object3dGenerator.UseCache = false;
            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-2500, -2500), new Vector2(2500, 2500), new Vector2(-1, 1), Vector3.UnitY, 512, waterGen);
            */
          //  var color = SingleTextureMaterial.FromMedia("gtamap.jpg");

            //Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-2500, -2500), new Vector2(2500, 2500), new Vector2(-1, 1), Vector3.UnitY, 512, terrainGen);
            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-1000, -1000), new Vector2(1000, 1000), new Vector2(1000, 1000), Vector3.UnitY);


            var color = SingleTextureMaterial.FromMedia("158.JPG", "158_norm.JPG");
            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            //water.SetCollisionShape(waterInfo.GetAccurateCollisionShape());
            //water.SpecularSize = 15.0f;
            // water.DiffuseComponent = 0.5f;
            //water.DisableDepthWrite = true;
            World.Root.Add(water);
            //World.Root.PhysicalWorld.AddCollisionObject(water.CreateRigidBody());

            ProjectionLight redConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 5000, 5000, MathHelper.PiOver3, 1.0f, 10000.0f);
            redConeLight.LightColor = Color.FromArgb(255, 255, 255, 255);
            //redConeLight.SetProjection(Matrix4.CreateOrthographic(200, 200, -500, 500));
            LightPool.Add(redConeLight);
            /*
            GLThread.OnUpdate += (o, e) =>
            {
                var kb = OpenTK.Input.Keyboard.GetState();
                if(kb.IsKeyDown(OpenTK.Input.Key.Minus))
                {
                    redConeLight.camera.ViewMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, 0.01f) * redConeLight.camera.ViewMatrix;
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, 0.01f)));
                    redConeLight.camera.Transformation.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, 0.01f));
                }
                if(kb.IsKeyDown(OpenTK.Input.Key.Plus))
                {
                    redConeLight.camera.ViewMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, -0.01f) * redConeLight.camera.ViewMatrix;
                    var pos = redConeLight.camera.Transformation.GetPosition();
                    redConeLight.camera.Transformation.SetPosition(pos.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -0.01f)));
                    redConeLight.camera.Transformation.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, -0.01f));
                }
            };*/

            // ProjectionLight greenConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 4000, 4000, MathHelper.PiOver2, 1.0f, 13000.0f); greenConeLight.LightColor = Color.Green; LightPool.Add(greenConeLight);

            // ProjectionLight blueConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 4000, 4000, MathHelper.PiOver2, 1.0f, 13000.0f);
            //blueConeLight.LightColor = Color.Blue;
            // LightPool.Add(blueConeLight);

            //var gen = ParticleGenerator.CreateBox(new Vector3(0, 100, 0), new Vector3(1000, 100, 1000), Quaternion.Identity, Vector3.UnitZ, Vector3.UnitZ * 50.0f, 1.0f, 1.0f, 2.0f);
            //ParticleSystem.Generators.Add(gen);

            //var cubesScene = new ManyCubesScene();
            //new ManyCubesScene().Create();
            //var homeScene = new HomeScene();
           // homeScene.Create();

            //new SculptScene().Create();
            new HallScene().Create();

            //MeshLinker.Link(freeCamera.Cam, redConeLight, Vector3.Zero, Quaternion.Identity);

            Object3dInfo portalgunInfo = Object3dInfo.LoadFromObjSingle(Media.Get("portalgun.obj"));
            Mesh3d portalgun = new Mesh3d(portalgunInfo, SingleTextureMaterial.FromMedia("portalgun_col.jpg", "portalgun_nor.jpg"));
            portalgun.Transformation.SetScale(0.1f);
            World.Root.Add(portalgun);
            MeshLinker.Link(freeCamera.Cam, portalgun, new Vector3(0.1f, -0.07f, -0.1f), Quaternion.Identity);
            /*
            Object3dInfo carInfo = Object3dInfo.LoadFromObjSingle(Media.Get("aston.obj"));
            carInfo.OriginToCenter();
            carInfo.Normalize();
            //tankBodyInfo.Normalize();
            Mesh3d car = new Mesh3d(carInfo, SingleTextureMaterial.FromMedia("carbon.jpg"));
            car.Transformation.SetScale(20.0f);
            car.SetMass(1000.0f);
            car.Transformation.SetPosition(new Vector3(66, 33, 0));

            Vector3 aabox = carInfo.GetAxisAlignedBox();
            car.SetCollisionShape(new BulletSharp.BoxShape(aabox.X * 20.0f, aabox.Y * 13.1f, aabox.Z * 20.0f));
            World.Root.Add(car);

            //tankBody.GetCollisionShape().CollisionInformation.LocalPosition += new Vector3(0, 2, 0).ToBepu();
            car.PhysicalBody.SetDamping(0.12f, 0.3f);
            car.PhysicalBody.Friction = 0.9f;
            car.PhysicalBody.RollingFriction = 0.8f;

            GLThread.OnUpdate += (o, e) =>
            {
                var keyboard = OpenTK.Input.Keyboard.GetState();
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Up))
                {
                    var dir = car.Transformation.GetOrientation().ToDirection();
                    dir.Y = 0;
                    car.PhysicalBody.ApplyCentralImpulse(-dir * 800.0f);
                }
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Down))
                {
                    var dir = car.Transformation.GetOrientation().ToDirection();
                    dir.Y = 0;
                    car.PhysicalBody.ApplyCentralImpulse(dir * 400.0f);
                }
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Left))
                {
                    car.PhysicalBody.AngularVelocity = car.PhysicalBody.AngularVelocity + new Vector3(0, 0.05f, 0);
                }
                if(keyboard.IsKeyDown(OpenTK.Input.Key.Right))
                {
                    car.PhysicalBody.AngularVelocity = car.PhysicalBody.AngularVelocity + new Vector3(0, -0.05f, 0);
                }
                if(car.PhysicalBody.AngularVelocity.Length > 0.5f)
                {
                    //car.GetCollisionShape().AngularVelocity = car.GetCollisionShape().AngularVelocity.ToOpenTK() * 0.4f;
                }
                if(car.PhysicalBody.LinearVelocity.Length > 60.0f)
                {
                    // tankBody.GetCollisionShape().LinearVelocity = tankBody.GetCollisionShape().LinearVelocity.ToOpenTK() * 0.9f;
                }
            };
            */
        //    Line2d line = new Line2d(new Vector3(-10, 30, -20), new Vector3(20, 40, 30), Color.Red);
        //    World.Root.LinesPool.Add(line);
            /*
            var chainColor = new SolidColorMaterial(Color.FromArgb(255, Color.LightBlue));
            Object3dInfo[] wallInfos = Object3dInfo.LoadFromObj(Media.Get("fract.obj"));
            List<Mesh3d> inserted = new List<Mesh3d>();
            for(int i = 0; i < wallInfos.Length - 1; i++)
            {
                var info = wallInfos[i];
                var offset = info.GetAverageTranslationFromZero();
                info.OriginToCenter();
                Mesh3d mesh = new Mesh3d(info, chainColor);
                mesh.SetMass(11111f);
                mesh.Transformation.SetScale(100.0f);
                mesh.Transformation.SetPosition(offset*100.0f + new Vector3(0, 0, -100));
                mesh.SetCollisionShape(info.GetConvexHull(100.0f));
                World.Root.Add(mesh);
                mesh.PhysicalBody.Friction = 6696;
                mesh.PhysicalBody.RollingFriction = 3393;
                mesh.PhysicalBody.ForceActivationState(BulletSharp.ActivationState.IslandSleeping);
                //mesh.PhysicalBody.SetSleepingThresholds(555, 555);
                // mesh.PhysicalBody.ContactProcessingThreshold = 10000.0f;

                inserted.Add(mesh);
            }*/
            Object3dInfo icosphere = Object3dInfo.LoadFromCompressed(Media.Get("Icosphere.o3i"));
            /*
            Mesh3d firstmesh = null;
            Object3dInfo simplecubeInfo = Object3dInfo.LoadFromObjSingle(Media.Get("chain.obj"));
            Vector3 box = simplecubeInfo.GetAxisAlignedBox();


            Mesh3d ball = new Mesh3d(icosphere, new SolidColorMaterial(Color.Blue));
            ball.Transformation.SetScale(14.0f);
            ball.SetMass(10100);
            ball.SetCollisionShape(new BulletSharp.SphereShape(14.0f));
            World.Root.Add(ball);

            // attach pos = x 0.2198 y 20.194 z -0.3562
            var railinfo = Object3dInfo.LoadFromObjSingle(Media.Get("rail.obj"));
            Mesh3d rail = new Mesh3d(railinfo, new SolidColorMaterial(Color.Red));
            rail.Transformation.SetPosition(new Vector3(0, 7.0f, 0));
            rail.Transformation.SetScale(10.0f);
            rail.SetMass(0);
            rail.SetCollisionShape(railinfo.GetAccurateCollisionShape(100.0f));
            World.Root.Add(rail);

            for(int y = -5; y < 60; y++)
            {
                //simplecubeInfo.OriginToCenter();
               // simplecubeInfo.Normalize();
                Mesh3d mesh = new Mesh3d(simplecubeInfo, chainColor);
                mesh.SetMass(115.3f);
                mesh.Transformation.SetScale(1.0f);
                // mesh.DisableDepthWrite = true;
                mesh.Transformation.SetPosition(new Vector3(40, 10, y * box.Z * 0.56f));
                mesh.SetCollisionShape(new BulletSharp.BoxShape(1.0f, 1.0f, box.Z * 0.1f));
                World.Root.Add(mesh);
                if(firstmesh != null)
                {
                    var axis1 = y % 2 == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);
                    var axis2 = y % 2 == 0 ? new Vector3(0, 1, 0) : new Vector3(1, 0, 0);
                    var cst = new BulletSharp.HingeConstraint(firstmesh.PhysicalBody, mesh.PhysicalBody,
                       new Vector3(0, 0, box.Z * 0.56f), new Vector3(0, 0, -box.Z * 0.56f), axis1, axis2);
                    // cst.BreakingImpulseThreshold = 4000.0f;
                    //cst.Setting.ImpulseClamp = 1000.0f;
                    //cst.
                    cst.EnableFeedback(false);
                    cst.SetLimit(-9999, 9991, 0, 0.5f, 0);
                    cst.BuildJacobian();
                    World.Root.PhysicalWorld.AddConstraint(cst, true);
                }
                else
                {
                    var cst = new BulletSharp.Point2PointConstraint(rail.PhysicalBody, mesh.PhysicalBody,
                       new Vector3(0.2198f, 20.194f, -0.3562f) * 10.0f, new Vector3(0, 0, -box.Z * 0.56f));
                    // cst.BreakingImpulseThreshold = 4000.0f;
                    //cst.Setting.ImpulseClamp = 1000.0f;
                    //cst.
                    World.Root.PhysicalWorld.AddConstraint(cst, true);
                }
                firstmesh = mesh;
            }
            var cstf = new BulletSharp.Point2PointConstraint(firstmesh.PhysicalBody, ball.PhysicalBody,
                new Vector3(0, 0, 0), new Vector3(0, 14, 0));
            // cst.BreakingImpulseThreshold = 4000.0f;
            //cst.Setting.ImpulseClamp = 1000.0f;
            //cst.
            cstf.EnableFeedback(false);
            cstf.BuildJacobian();
            World.Root.PhysicalWorld.AddConstraint(cstf, true);*/
            

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
                    var sphere = new BulletSharp.SphereShape(1.1f);
                    Mesh3d m = new Mesh3d(icosphere, new SolidColorMaterial(Color.Black));
                    m.Transformation.SetPosition(freeCamera.Cam.Transformation.GetPosition() + freeCamera.Cam.Transformation.GetOrientation().ToDirection() * 2.0f);
                    m.SetMass(11.0f);
                    m.SetCollisionShape(sphere);
                    World.Root.Add(m);
                    m.PhysicalBody.LinearVelocity = freeCamera.Cam.Transformation.GetOrientation().ToDirection() * 101.0f;
                    World.Root.MeshCollide += (m1, m2, v, n) =>
                    {
                        if(m1 == m || m2 == m)
                        {
                            World.Root.Explode(m1.Transformation.GetPosition(), 10.0f);
                            World.Root.Remove(m);
                        }
                    };
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

            UI.Text testLabel = new UI.Text(0.85f, 0.95f, "VEngine Test", "Segoe UI", 24, Color.White);
            World.Root.UI.Elements.Add(testLabel);

          //  UI.Text fpsLabel = new UI.Text(0.85f, 0.85f, " ", "Segoe UI", 24, Color.White);
          //  World.Root.UI.Elements.Add(fpsLabel);

          //  UI.Rectangle rect = new UI.Rectangle(0.85f, 0.85f, 1.0f, 1.0f, Color.FromArgb(70, Color.Red));
         //   World.Root.UI.Elements.Add(rect);

            var size = UI.UIRenderer.PixelsToScreenSpace(new Vector2(78, 88));
            var pos = new Vector2(0.5f) - size / 2.0f;
            UI.Picture smok = new UI.Picture(pos, size, new Texture(Media.Get("portal_crosshair.png")), 0.5f);
            World.Root.UI.Elements.Add(smok);
            
            System.Timers.Timer lensFocusTimer = new System.Timers.Timer();
            lensFocusTimer.Interval = 100;
            lensFocusTimer.Elapsed += (o, e) =>
            {
                GLThread.Invoke(() =>
                Camera.Current.CurrentDepthFocus = (Camera.Current.CurrentDepthFocus * 4.0f + window.PostProcessFramebuffer1.GetDepth(0.5f, 0.5f)) / 5.0f);
            };
            lensFocusTimer.Start();

           /* GLThread.OnBeforeDraw += (o, e) =>
            {
                var campos = freeCamera.Cam.Transformation.GetPosition();
                fpsLabel.Update(1.0f, 0.90f, campos.ToString(), "Segoe UI", 24, Color.White);
                fpsLabel.Position.X -= fpsLabel.Size.X;
                rect.Update(fpsLabel.Position.X, fpsLabel.Position.Y, fpsLabel.Size.X, fpsLabel.Size.Y, rect.Color);
            };*/

            Skybox skybox = new Skybox(ManualShaderMaterial.FromName("Skybox"));
            skybox.Use();

            GLThread.OnMouseWheel += (o, e) =>
            {
                if(!inPickingMode)
                    Camera.Current.LensBlurAmount -= e.Delta / 20.0f;
            };

            World.Root.SimulationSpeed = 8.0f;

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
                if(e.Key == OpenTK.Input.Key.T)
                {
                    World.Root.SimulationSpeed = 8.0f;
                }
                if(e.Key == OpenTK.Input.Key.Y)
                {
                    World.Root.SimulationSpeed = 8.0f;
                }
                if(e.Key == OpenTK.Input.Key.Number1)
                {
                    //redConeLight.SetPosition(freeCamera.Cam.Transformation.GetPosition(), freeCamera.Cam.Transformation.GetPosition() + freeCamera.Cam.Transformation.GetOrientation().ToDirection());
                    redConeLight.GetTransformationManager().SetPosition(freeCamera.Cam.Transformation.GetPosition());
                    redConeLight.GetTransformationManager().SetOrientation(freeCamera.Cam.Transformation.GetOrientation().Inverted());
                }
                if(e.Key == OpenTK.Input.Key.Number2)
                {
                    // greenConeLight.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.ToDirection());
                }
                if(e.Key == OpenTK.Input.Key.Number3)
                {
                    // blueConeLight.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Position + freeCamera.Cam.Orientation.ToDirection());
                }
            };

            World.Root.SortByObject3d();

            GLThread.Invoke(() => window.StartPhysicsThread());
            GLThread.Invoke(() => window.SetDefaultPostProcessingMaterial());
            renderThread.Wait();
        }
    }
}