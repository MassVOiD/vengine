using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDGTech;
using System.Threading.Tasks;
using System.Drawing;
using VDGTech.Generators;
using BEPUphysics.Entities.Prefabs;

namespace ShadowsTester
{
    class Program
    {
        static void Main(string[] args)
        {
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

            var freeCamera = new FreeCamera();


            Object3dInfo icosphereInfo = Object3dInfo.LoadFromCompressed(Media.Get("Icosphere.o3i"));
            Mesh3d icosphere = new Mesh3d(icosphereInfo, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(icosphere);

            Object3dInfo cube1Info = Object3dInfo.LoadFromCompressed(Media.Get("Cube_Cube.001.o3i"));
            Mesh3d cube1 = new Mesh3d(cube1Info, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(cube1);

            Object3dInfo cube2Info = Object3dInfo.LoadFromCompressed(Media.Get("Cube.001_Cube.002.o3i"));
            Mesh3d cube2 = new Mesh3d(cube2Info, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(cube2);

            Object3dInfo ballInfo = Object3dInfo.LoadFromCompressed(Media.Get("Mball_Meta.o3i"));
            Mesh3d ball = new Mesh3d(ballInfo, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(ball);

            Object3dInfo textInfo = Object3dInfo.LoadFromCompressed(Media.Get("Text.o3i"));
            Mesh3d text = new Mesh3d(textInfo, new SolidColorMaterial(Color.Yellow));
            World.Root.Add(text);

            Object3dInfo suzanneInfo = Object3dInfo.LoadFromCompressed(Media.Get("Suzanne.o3i"));
            Mesh3d suzanne = new Mesh3d(suzanneInfo, new SolidColorMaterial(Color.Red));
            World.Root.Add(suzanne);

            /*Object3dInfo coneInfo = Object3dInfo.LoadFromCompressed(Media.Get("Cone.o3i"));
            Mesh3d cone = new Mesh3d(coneInfo, new SolidColorMaterial(Color.White));
            cone.Translate(new Vector3(20, 20, 20));
            cone.Rotate(Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi/3));
            World.Root.Add(cone);*/

            var datetex = Texture.FromText("Date", "Impact", 160.0f, Color.Black, Color.White);


            Random rand = new Random();

            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                float h =
                    SimplexNoise.Noise.Generate(x, y) +
                    (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 11.5f) +
                    (SimplexNoise.Noise.Generate((float)x / 14, (float)y / 14) * 33) +
                    (SimplexNoise.Noise.Generate((float)x / 126, (float)y / 126) * 87);
                return h;
            };

            //Object3dInfo groundInfo = Object3dGenerator.CreateGround(new Vector2(-150, -150), new Vector2(150, 150), new Vector2(1000, 1000), Vector3.UnitY);
            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-1000, -1000), new Vector2(1000, 1000), new Vector2(10, 10), Vector3.UnitY, 256, terrainGen);
            //Object3dInfo groundInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain4.o3i"));


            Mesh3d ground = new Mesh3d(groundInfo, new SolidColorMaterial(Color.Green));
            ground.SetStaticCollisionMesh(groundInfo.GetAccurateCollisionShape(Vector3.Zero));
            ground.GetStaticCollisionMesh().Material.Bounciness = 1.0f;
            World.Root.Add(ground);

            /*Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-10000, -10000), new Vector2(10000, 10000), new Vector2(20, 20), Vector3.UnitY);

            Mesh3d water = new Mesh3d(waterInfo, new SolidColorMaterial(Color.FromArgb(140, Color.Blue)));
            water.SetPosition(new Vector3(0, -150, 0));
            World.Root.Add(water);*/

            FOVLight coneLight = new FOVLight(new Vector3(60, 30, 60), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 2048, 2048, 3.14f / 2.0f, 1.0f, 10000.0f);
            //var ms = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 10000.0f;
            ///coneLight.SetPosition(new Vector3((float)Math.Sin(ms) * 20, 15, (float)Math.Cos(-ms)) * 20, Vector3.Zero);
            //System.Timers.Timer lightMovement = new System.Timers.Timer(40);
            //lightMovement.Elapsed += (o, e) =>
            //{
               //var ms = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 10000.0f;
               // coneLight.SetPosition(new Vector3((float)Math.Sin(ms) * 20, 15, (float)Math.Cos(-ms)) * 20, Vector3.Zero);
           // };
            //lightMovement.Start();
            LightPool.Add(coneLight);
            
            var color = new SolidColorMaterial(Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255)));
            for(int i = 0; i < 130; i++)
            {
                Mesh3d a = new Mesh3d(icosphereInfo, color);
                a.SetScale(1);
                a.SetPosition(new Vector3(rand.Next(-150, 150), 25, rand.Next(-150, 150)));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Sphere(a.GetPosition(), 0.8f, 20.0f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 0.5f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;
                
                World.Root.Add(a);
            }
            
            Object3dInfo simplecubeInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[1];
            SingleTextureMaterial cubewood = new SingleTextureMaterial(new Texture(Media.Get("wood.jpg")));
            float cubesize = 0.384960f * 6.0f;
            for(int i = 0; i < 22; i++)
                for(int g = 0; g < 32; g++)
            {
                Mesh3d a = new Mesh3d(simplecubeInfo, color);
                a.SetScale(3);
                a.SetPosition(new Vector3(i * cubesize, g * cubesize, -40));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Box(a.GetPosition(), cubesize, cubesize, cubesize, 20.0f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 0.0f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;

                World.Root.Add(a);
            }

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Left)
                {
                    Mesh3d mesh = Camera.Current.RayCast();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.GetCollisionShape().LinearVelocity += (Vector3.UnitY * 20.0f).ToBepu();
                    }
                }
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    coneLight.SetPosition(freeCamera.Cam.Position, freeCamera.Cam.Orientation);
                }
            };
            /*
            System.Timers.Timer datetimer = new System.Timers.Timer(1000);
            datetimer.Elapsed += (o, e) =>
            {
                datetex.UpdateFromText(DateTime.Now.ToShortTimeString() + ":" + DateTime.Now.Second.ToString(), "Segoe UI", 160.0f, Color.Black, Color.White);
            };*/
            //datetimer.Start();
            GLThread.Invoke(() => window.StartPhysicsThread());
            GLThread.Invoke(() => window.SetDefaultPostProcessingMaterial());
            renderThread.Wait();
        }
    }
}
