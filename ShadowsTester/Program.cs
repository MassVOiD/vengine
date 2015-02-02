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
using BEPUphysics.Vehicle;

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


            Texture tankTex = new Texture(Media.Get("tank.png"));

            Object3dInfo tankHeadInfo = Object3dInfo.LoadFromCompressed(Media.Get("tank_head.o3i"));
            Mesh3d tankHead = new Mesh3d(tankHeadInfo, new SingleTextureMaterial(tankTex));
            World.Root.Add(tankHead);

            Object3dInfo tankBodyInfo = Object3dInfo.LoadFromCompressed(Media.Get("tank_body.o3i"));
            Mesh3d tankBody = new Mesh3d(tankBodyInfo, new SingleTextureMaterial(tankTex));

            tankBody.SetCollisionShape(new Box(tankBody.GetPosition(), 12, 4, 14, 1200.0f));
            tankBody.GetCollisionShape().CollisionInformation.LocalPosition += new Vector3(0, 2, 0).ToBepu();

            World.Root.Add(tankBody);

            var tankLinked = MeshLinker.Link(tankBody, tankHead, Vector3.Zero, Quaternion.Identity);

            System.Timers.Timer tankMover = new System.Timers.Timer(16);
            tankMover.Elapsed += (o, e) =>
            {
                tankLinked.Rotation = Quaternion.Multiply(tankLinked.Rotation, Quaternion.FromAxisAngle(Vector3.UnitY, 0.01f));
            };
            tankMover.Start();



            Object3dInfo fiatBodyInfo = Object3dInfo.LoadFromCompressed(Media.Get("cinkus_body.o3i"));
            Object3dInfo fiatWheelInfo = Object3dInfo.LoadFromCompressed(Media.Get("cinkus_wheel.o3i"));

            Mesh3d fiatBody = new Mesh3d(fiatBodyInfo, new SolidColorMaterial(Color.Cyan));
            fiatBody.SetCollisionShape(new Box(tankBody.GetPosition(), 12, 4, 14, 1200.0f));
            fiatBody.GetCollisionShape().CollisionInformation.LocalPosition += new Vector3(0, 2, 0).ToBepu();
            World.Root.Add(fiatBody);

            Mesh3d fiatWheelFR = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            World.Root.Add(fiatWheelFR);

            Mesh3d fiatWheelFL = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            fiatWheelFR.SetPosition(new Vector3(0.83068f, -0.05185f, -1.31027f));
            World.Root.Add(fiatWheelFL);

            Mesh3d fiatWheelRR = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            fiatWheelFR.SetPosition(new Vector3(0.83068f, -0.05185f, 1.24699f));
            World.Root.Add(fiatWheelRR);

            Mesh3d fiatWheelRL = new Mesh3d(fiatWheelInfo, new SolidColorMaterial(Color.Brown));
            fiatWheelFR.SetPosition(new Vector3(0.83068f, -0.05185f, 1.24699f));
            World.Root.Add(fiatWheelRL);

            var whellFR = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(-0.83068f, 0.05185f, -1.31027f), Quaternion.Identity);
            var whellFL = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(0.83068f, 0.05185f, -1.31027f), Quaternion.Identity);
            var whellRR = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(-0.83068f, 0.05185f, 1.24699f), Quaternion.Identity);
            var whellRL = MeshLinker.Link(fiatBody, fiatWheelFR, new Vector3(0.83068f, 0.05185f, 1.24699f), Quaternion.Identity);
            


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
                    (SimplexNoise.Noise.Generate((float)x / 126, (float)y / 126) * 187);
                return h * 1.3f - 30.0f;
            };

            //Object3dInfo groundInfo = Object3dGenerator.CreateGround(new Vector2(-150, -150), new Vector2(150, 150), new Vector2(1000, 1000), Vector3.UnitY);
            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-100, -100), new Vector2(100, 100), new Vector2(10, 10), Vector3.UnitY, 44, (x, y) => 0);
            //Object3dInfo groundInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain4.o3i"));


            Mesh3d ground = new Mesh3d(groundInfo, new SingleTextureMaterial(new Texture(Media.Get("tiles.jpg"))));
            ground.SetStaticCollisionMesh(groundInfo.GetAccurateCollisionShape(Vector3.Zero));
            ground.GetStaticCollisionMesh().Material.Bounciness = 1.0f;
            World.Root.Add(ground);

            /*Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-10000, -10000), new Vector2(10000, 10000), new Vector2(20, 20), Vector3.UnitY);

            Mesh3d water = new Mesh3d(waterInfo, new SolidColorMaterial(Color.FromArgb(140, Color.Blue)));
            water.SetPosition(new Vector3(0, -150, 0));
            World.Root.Add(water);*/

            FOVLight coneLight = new FOVLight(new Vector3(60, 30, 60), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 6700, 6700, 3.14f / 2.0f, 1.0f, 300.0f);
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
            for(int i = 0; i < 33; i++)
            {
                Mesh3d a = new Mesh3d(icosphereInfo, color);
                a.SetScale(1);
                a.SetPosition(new Vector3(rand.Next(-150, 150), 25, rand.Next(-150, 150)));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Sphere(a.GetPosition(), 0.8f, 20.0f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 1.0f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;
                
                World.Root.Add(a);
            }
            
            Object3dInfo simplecubeInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[1];
            SingleTextureMaterial cubewood = new SingleTextureMaterial(new Texture(Media.Get("wood.jpg")));
            
            for(int i = 0; i < 12; i++)
                for(int g = 0; g < 12; g++)
            {
                float scale = i;
                float cubesize = 0.384960f * scale;
                Mesh3d a = new Mesh3d(simplecubeInfo, color);
                a.SetScale(scale / 2.0f);
                a.SetPosition(new Vector3(i * cubesize, g * cubesize, 10.0f));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Box(a.GetPosition(), cubesize, cubesize, cubesize, 0.1f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 0.0f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Discrete;

                World.Root.Add(a);
            }

            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Middle)
                {
                    Mesh3d mesh = Camera.Current.RayCast();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.GetCollisionShape().LinearVelocity += (Camera.Current.GetDirection() * 20.0f).ToBepu();
                    }
                }
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
