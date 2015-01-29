using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BEPUphysics.Entities.Prefabs;

namespace Tester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            VEngineWindowAdapter window = null;
            var Config = SharpScript.CreateClass(System.IO.File.ReadAllText("Config.css"));
            Media.SearchPath = Config.MediaPath;

            var renderThread = Task.Factory.StartNew(() =>
            {
                window = new VEngineWindowAdapter("Test", Config.Width, Config.Height);
                window.Run(60);
            });

            float[] postProcessingPlaneVertices = {
                -1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f
            };
            uint[] postProcessingPlaneIndices = {
                0, 1, 2, 3, 2, 1
            };

            World.Root = new World();
            Mesh3d postPlane = null;

            Camera camera = new Camera(new Vector3(52, 22, 5), new Vector3(0, 2, 0), 1600.0f / 900.0f, 3.14f / 2.0f, 20.0f, 10000.0f);

            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
            postPlane = new Mesh3d(postPlane3dInfo, new PostProcessLoadingMaterial());

            World.Root.UpdateMatrix();
            World.Root.Add(postPlane);

            Object3dInfo ball3dInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[0];
            Object3dInfo terrain3dInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain.rend"));
            Random rand = new Random();


            //Texture tex = new Texture(Media.Get("earthsmall.png")); 
            


            Mesh3d terrain = new Mesh3d(terrain3dInfo, ManualShaderMaterial.FromName("Mountains"));
            terrain.SetScale(100);
            terrain.SetPosition(new Vector3(0, -1900, 0));
            terrain.SetMass(0);
           // terrain.Instances = 256;
            var terrainShape = terrain3dInfo.GetAccurateCollisionShape(terrain.GetPosition(), 100.0f);
            terrain.SetStaticCollisionMesh(terrainShape);
            World.Root.Add(terrain);


            Airplane copter = new Airplane();
            World.Root.Add(copter);

            bool Shooting = false;
            GLThread.OnKeyDown += (o, e) =>
            {
                if (e.Key == OpenTK.Input.Key.Space)
                {
                    //if (Shooting) return;
                    Shooting = true;
                    Mesh3d ball = new Mesh3d(ball3dInfo, new SolidColorMaterial(Color.Yellow));
                    ball.SetScale(10.0f);
                    ball.SetPosition(copter.Body.GetPosition() + copter.Body.GetOrientation().ToDirection() * 20.0f);
                    ball.SetCollisionShape(new Sphere(ball.GetPosition(), 10.0f, 4.1f));
                    ball.SetMass(4.1f);
                    World.Root.Add(ball);
                    Vector3 direction = copter.Body.GetOrientation().ToDirection() * 1140.0f;
                    ball.GetCollisionShape().ApplyImpulse(Vector3.Zero, direction);
                }
            };
            GLThread.OnKeyUp += (o, e) =>
            {
                if (e.Key == OpenTK.Input.Key.Space)
                {
                    Shooting = false;
                }
            };

            World.Root.Remove(postPlane);

            Skybox skybox = new Skybox(ManualShaderMaterial.FromName("Skybox"));
            skybox.Use();

            GLThread.Invoke(() =>  window.StartPhysicsThread());
            renderThread.Wait();
        }
    }
}