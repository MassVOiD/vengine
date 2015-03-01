using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDGTech;
using System.Threading.Tasks;
using System.Drawing;
using VDGTech.Generators;
using VDGTech.Particles;

namespace ShadowsTester
{
    class Program
    {
        public static FreeCamera FreeCam;
        static void Main(string[] args)
        {
            //System.Threading.Thread.Sleep(1000);
            VEngineWindowAdapter window = null;
            var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));
            Media.SearchPath = Config.MediaPath;

            var renderThread = Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height);
                window.Run(60);
            });
            GLThread.Invoke(() =>
            {
                window.SetCustomPostProcessingMaterial(new PostProcessLoadingMaterial());
            });
            World.Root = new World();

           // System.Threading.Thread.Sleep(1000);

            var freeCamera = new FreeCamera();
            FreeCam = freeCamera;

            Random rand = new Random();

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
            Func<uint, uint, float> waterGen = (x, y) =>
            {
                float h =
                    (SimplexNoise.Noise.Generate((float)x, (float)y) / 2) +
                    (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4));
                return h - 5.0f;
            };

            Object3dInfo waterInfo = Object3dGenerator.CreateTerrain(new Vector2(-500, -501), new Vector2(500, 500), new Vector2(100, 100), Vector3.UnitY, 16, (x, y) => 0);

            var color = SingleTextureMaterial.FromMedia("grass.jpg");

            Mesh3d water = new Mesh3d(waterInfo, color);
            water.SetMass(0);
            water.SetCollisionShape(new BulletSharp.StaticPlaneShape(Vector3.UnitY, 0));
            //water.SetCollisionShape(waterInfo.GetAccurateCollisionShape());
            //water.SpecularSize = 15.0f;
            // water.DiffuseComponent = 0.5f;
            World.Root.Add(water);


            ProjectionLight redConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 2048, 2048, MathHelper.PiOver2, 1.0f, 10100.0f);
            redConeLight.LightColor = Color.White;
            LightPool.Add(redConeLight);

           // ProjectionLight greenConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 4000, 4000, MathHelper.PiOver2, 1.0f, 13000.0f);
           // greenConeLight.LightColor = Color.Green;
           // LightPool.Add(greenConeLight);

           // ProjectionLight blueConeLight = new ProjectionLight(new Vector3(65, 0, 65), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 2), 4000, 4000, MathHelper.PiOver2, 1.0f, 13000.0f);
            //blueConeLight.LightColor = Color.Blue;
           // LightPool.Add(blueConeLight);


            //var gen = ParticleGenerator.CreateBox(new Vector3(0, 100, 0), new Vector3(1000, 100, 1000), Quaternion.Identity, Vector3.UnitZ, Vector3.UnitZ * 50.0f, 1.0f, 1.0f, 2.0f);
            //ParticleSystem.Generators.Add(gen);

           // var testroom = Object3dInfo.LoadSceneFromObj(Media.Get("testroom.obj"), Media.Get("testroom.mtl"), 50.0f);
           // foreach(var ob in testroom)
           //     World.Root.Add(ob);

            //MeshLinker.Link(freeCamera.Cam, redConeLight, Vector3.Zero, Quaternion.Identity);

            Object3dInfo carInfo = Object3dInfo.LoadFromObjSingle(Media.Get("aston.obj"));
            carInfo.OriginToCenter();
            carInfo.Normalize();
            //tankBodyInfo.Normalize();
            Mesh3d car = new Mesh3d(carInfo, new SolidColorMaterial(Color.LightCyan));
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

            var raisingblockInfo = Object3dInfo.LoadFromObjSingle(Media.Get("raisingblock.obj"));
            raisingblockInfo.FlipFaces();
            raisingblockInfo.FlipNormals();
            raisingblockInfo.Normalize();
            var fullblockInfo = Object3dInfo.LoadFromObjSingle(Media.Get("fulllblock.obj"));
            fullblockInfo.FlipFaces();
            fullblockInfo.FlipNormals();
            fullblockInfo.Normalize();

            Object3dInfo simplecubeInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[0];
            Vector3 cubeBox = simplecubeInfo.GetAxisAlignedBox();
            var purpleColor = new SolidColorMaterial(Color.Purple);
            for(int i = -2; i < 2; i++)
            {

                for(int g = -2; g < 2; g++)
                {
                    for(int y = 0; y < 12; y++)
                    {
                        Mesh3d mesh = new Mesh3d(simplecubeInfo, purpleColor);
                        mesh.Transformation.SetScale(10.0f);
                        mesh.SetMass(100.0f);
                        mesh.Transformation.SetPosition(new Vector3(i * (cubeBox.X / 2.0f * 10.0f), (y * (cubeBox.X / 2.0f * 10.0f)) + cubeBox.Y / 2.0f * 10.0f, g * (cubeBox.X / 2.0f * 10.0f)));
                        mesh.SetCollisionShape(new BulletSharp.BoxShape(cubeBox * 10.0f));
                        World.Root.Add(mesh);
                    }
                }
            }

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Middle)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.PhysicalBody.LinearVelocity += (Camera.Current.GetDirection() * 20.0f);
                    }
                }
                if(e.Button == OpenTK.Input.MouseButton.Left)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.PhysicalBody.LinearVelocity += (Vector3.UnitY * 20.0f);
                    }
                }

            };
            
            System.Timers.Timer lensFocusTimer = new System.Timers.Timer();
            lensFocusTimer.Interval = 100;
            lensFocusTimer.Elapsed += (o, e) =>
            {
                GLThread.Invoke(() =>
                Camera.Current.CurrentDepthFocus = (Camera.Current.CurrentDepthFocus * 4.0f + window.PostProcessFramebuffer.GetDepth(0.5f, 0.5f)) / 5.0f);
            };
            lensFocusTimer.Start();


            Skybox skybox = new Skybox(ManualShaderMaterial.FromName("Skybox"));
            skybox.Use();
            
            GLThread.OnMouseWheel += (o, e) =>
            {
                Camera.Current.LensBlurAmount -= e.Delta / 20.0f;
            };
            
            GLThread.OnKeyUp += (o, e) =>
            {
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
        
            GLThread.Invoke(() => window.StartPhysicsThread());
            GLThread.Invoke(() => window.SetDefaultPostProcessingMaterial());
            renderThread.Wait();
        }
    }
}
