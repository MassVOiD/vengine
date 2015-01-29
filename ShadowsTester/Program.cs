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

            World.Root = new World();

            new FreeCamera();


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

            Object3dInfo groundInfo = Object3dGenerator.CreateGround(new Vector2(-100, -100), new Vector2(100, 100), new Vector2(20, 20), Vector3.UnitY);
            Mesh3d ground = new Mesh3d(groundInfo, new SingleTextureMaterial(new Texture(Media.Get("road.png"))));
            ground.SetStaticCollisionMesh(groundInfo.GetAccurateCollisionShape(Vector3.Zero));
            World.Root.Add(ground);

            FOVLight coneLight = new FOVLight(new Vector3(0, 5, 5), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 1000, 1000, 3.14f / 2.0f, 1.0f, 100.0f);
            LightPool.Add(coneLight);

            Random rand = new Random();
            for(int i = 0; i < 112; i++)
            {
                Mesh3d a = new Mesh3d(icosphereInfo, new SolidColorMaterial(Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255))));
                a.SetScale(1);
                a.SetPosition(new Vector3(rand.Next(-5, 5), 5, rand.Next(-5, 5)));
                a.SetMass(15.5f);
                a.SetCollisionShape(new Sphere(a.GetPosition(), 2.0f, 20.0f));
                var s = a.GetCollisionShape();
                s.Material.Bounciness = 1.0f;
                s.LinearDamping = 0;
                s.Material.KineticFriction = 1.0f;
                s.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
                
                World.Root.Add(a);
            }
            
            GLThread.Invoke(() => window.StartPhysicsThread());
            renderThread.Wait();
        }
    }
}
