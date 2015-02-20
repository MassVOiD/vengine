using OpenTK;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDGTech;
using System.Drawing;
using BEPUphysics.Entities.Prefabs;
using VDGTech.Generators;

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

            Camera camera = new Camera(new Vector3(52, 22, 5), new Vector3(0, 2, 0), 1600.0f / 900.0f, 3.14f / 2.0f, 5.0f, 200000.0f);

            Object3dInfo postPlane3dInfo = new Object3dInfo(postProcessingPlaneVertices.ToList(), postProcessingPlaneIndices.ToList());
            postPlane = new Mesh3d(postPlane3dInfo, new PostProcessLoadingMaterial());

            World.Root.UpdateMatrix();
            World.Root.Add(postPlane);

            Object3dInfo ball3dInfo = Object3dInfo.LoadFromObj(Media.Get("cube_simple.obj"))[0];
            Object3dInfo terrain3dInfo = Object3dInfo.LoadFromCompressed(Media.Get("terrain.rend"));
            Random rand = new Random();


            //Texture tex = new Texture(Media.Get("earthsmall.png")); 
            

            /*
            Mesh3d terrain = new Mesh3d(terrain3dInfo, ManualShaderMaterial.FromName("Mountains"));
            terrain.SetScale(100);
            terrain.SetPosition(new Vector3(0, -1900, 0));
            terrain.SetMass(0);
           // terrain.Instances = 256;
            var terrainShape = terrain3dInfo.GetAccurateCollisionShape(terrain.GetPosition(), 100.0f);
            terrain.SetStaticCollisionMesh(terrainShape);
            World.Root.Add(terrain);*/

            Func<uint, uint, float> terrainGen = (x, y) =>
            {
                return
                    SimplexNoise.Noise.Generate(x, y) +
                    (SimplexNoise.Noise.Generate((float)x / 4, (float)y / 4) * 11) +
                    (SimplexNoise.Noise.Generate((float)x / 14, (float)y / 14) * 160) +
                    (SimplexNoise.Noise.Generate((float)x / 26, (float)y / 26) * 300);
            };

            Object3dInfo groundInfo = Object3dGenerator.CreateTerrain(new Vector2(-6000.0f, -6000.0f), new Vector2(6000.0f, 6000.0f), new Vector2(20, 20), Vector3.UnitY, 333, terrainGen);

            Mesh3d ground = new Mesh3d(groundInfo, new SolidColorMaterial(Color.Green));
            //ground.SetStaticCollisionMesh(groundInfo.GetAccurateCollisionShape(Vector3.Zero));
            //ground.GetStaticCollisionMesh().Material.Bounciness = 1.0f;
            World.Root.Add(ground);

            Object3dInfo waterInfo = Object3dGenerator.CreateGround(new Vector2(-6000.0f, -6000.0f), new Vector2(6000.0f, 6000.0f), new Vector2(20, 20), Vector3.UnitY);

            Mesh3d water = new Mesh3d(waterInfo, new SolidColorMaterial(Color.FromArgb(140, Color.Blue)));
            water.SetPosition(new Vector3(0, 0, 0));
            World.Root.Add(water);



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

            ProjectionLight coneLight = new ProjectionLight(new Vector3(1500, 1500, 1500), Quaternion.FromAxisAngle(new Vector3(1, 0, -1), MathHelper.Pi / 3), 1024, 1024, 3.14f / 2.0f, 1.0f, 10000.0f);
            LightPool.Add(coneLight);


            GLThread.OnMouseUp += (o, e) =>
            {
                if(e.Button == OpenTK.Input.MouseButton.Left)
                {
                    Mesh3d mesh = Camera.Current.RayCastMesh3d();
                    if(mesh != null && mesh.GetCollisionShape() != null)
                    {
                        Console.WriteLine(mesh.GetCollisionShape().ToString());
                        mesh.GetCollisionShape().LinearVelocity += (Vector3.UnitY * 20.0f).ToBepu();
                    }
                }
                if(e.Button == OpenTK.Input.MouseButton.Right)
                {
                    coneLight.SetPosition(Camera.Current.Position, Camera.Current.Orientation);
                }
            };

            GLThread.Invoke(() =>  window.StartPhysicsThread());
            terrain3dInfo.Dispose();
            renderThread.Wait();
        }
    }
}